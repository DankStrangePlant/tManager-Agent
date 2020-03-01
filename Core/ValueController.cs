using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Map;

namespace TManagerAgent.Core
{
    public class ValueController
    {

        #region API

        #region Get
        public object GetMember(MemberRoute route)
        {
            if(route.Instance)
            {
                return GetMember(route.ContextType, route.Context, route.AccessorKey);
            }
            return GetStaticMember(route.ContextType, route.AccessorKey);
        }

        public object GetMember(MemberContextType contextType, object context, string key)
        {
            switch(contextType)
            {
                case MemberContextType.World:
                    return GetWorldMember(context as WorldMap, key);
                case MemberContextType.Player:
                    return GetPlayerMember(context as Player, key);
                default:
                    throw new ArgumentException($"Cannot get non-static member in context '{contextType}'");
            }
        }

        public object GetStaticMember(MemberContextType contextType, string key)
        {
            switch (contextType)
            {
                case MemberContextType.Main:
                    return GetStaticMainMember(key);
                case MemberContextType.World:
                    return GetStaticWorldMember(key);
                case MemberContextType.Player:
                    return GetStaticPlayerMember(key);
                default:
                    throw new ArgumentException($"Cannot get static member in context '{contextType}'");
            }
        }

        public object GetStaticMainMember(string key) => MainMembers[key].Get(null);

        public object GetWorldMember(WorldMap context, string key) => WorldMembers[key].Get(context);
        public object GetStaticWorldMember(string key) => WorldMembers[key].Get(null);

        public object GetPlayerMember(Player context, string key) => PlayerMembers[key].Get(context);
        public object GetStaticPlayerMember(string key) => PlayerMembers[key].Get(null);
        #endregion

        #region Set
        public void SetMember(MemberRoute route, object value)
        {
            if(route.Instance)
            {
                SetMember(route.ContextType, route.Context, route.AccessorKey, value);
            }
            SetStaticMember(route.ContextType, route.AccessorKey, value);
        }

        public void SetMember(MemberContextType contextType, object context, string key, object value)
        {
            switch (contextType)
            {
                case MemberContextType.World:
                    SetWorldMember(context as WorldMap, key, value);
                    break;
                case MemberContextType.Player:
                    SetPlayerMember(context as Player, key, value);
                    break;
                default:
                    throw new ArgumentException($"Cannot set non-static member in context '{contextType}'");
            }
        }

        public void SetStaticMember(MemberContextType contextType, string key, object value)
        {
            switch (contextType)
            {
                case MemberContextType.Main:
                    SetStaticMainMember(key, value);
                    break;
                case MemberContextType.World:
                    SetStaticWorldMember(key, value);
                    break;
                case MemberContextType.Player:
                    SetStaticPlayerMember(key, value);
                    break;
                default:
                    throw new ArgumentException($"Cannot set static member in context '{contextType}'");
            }
        }

        public void SetStaticMainMember(string key, object value) => MainMembers[key].Set(null, value);

        public void SetWorldMember(WorldMap context, string key, object value) => WorldMembers[key].Set(context, value);
        public void SetStaticWorldMember(string key, object value) => WorldMembers[key].Set(null, value);

        public void SetPlayerMember(Player context, string key, object value) => PlayerMembers[key].Set(context, value);
        public void SetStaticPlayerMember(string key, object value) => PlayerMembers[key].Set(null, value);
        #endregion

        #endregion

        #region Singleton
        private static ValueController mInstance;
        public static ValueController Instance
        {
            get
            {
                if (mInstance is null)
                {
                    mInstance = new ValueController();
                }
                return mInstance;
            }
        }
        #endregion

        #region Constructor
        private ValueController()
        {
            Init();
        }
        #endregion


        #region Private
        private Dictionary<string, IMemberAccessor<Main>> MainMembers = new Dictionary<string, IMemberAccessor<Main>>();
        private Dictionary<string, IMemberAccessor<WorldMap>> WorldMembers = new Dictionary<string, IMemberAccessor<WorldMap>>();
        private Dictionary<string, IMemberAccessor<Player>> PlayerMembers = new Dictionary<string, IMemberAccessor<Player>>();

        #region Init
        private void Init()
        {
            #region Static Main Members (alphabetized)
            AddMainField(nameof(Main.raining)); //bool
            AddMainField(nameof(Main.moonPhase)); //int
            #endregion

            #region Player Members (alphabetized)
            AddPlayerField(nameof(Player.lastDeathPostion)); //Vector2
            //AddPlayerCustom("lastDeathPositionX",
            //    player => player.lastDeathPostion.X,
            //    (player, value) => player.lastDeathPostion.X = (float)value,
            //    typeof(float)); //float
            //AddPlayerCustom("lastDeathPositionY",
            //    player => player.lastDeathPostion.Y,
            //    (player, value) => player.lastDeathPostion.Y = (float)value,
            //    typeof(float)); //float
            AddPlayerField(nameof(Player.position)); //Vector2
            AddPlayerField(nameof(Player.statLife)); //int
            AddPlayerField(nameof(Player.wingTime)); //float
            #endregion
        }
#endregion

#endregion

        #region Helper functions
        private void AddMainField(string key, string name = null) => MainMembers.Add(key, new FieldAccessor<Main>(name ?? key));
        private void AddMainProperty(string key, string name = null) => MainMembers.Add(key, new PropertyAccessor<Main>(name ?? key));
        private void AddMainCustom(string key, Func<Main, object> getter, Action<Main, object> setter, Type memberType, string name = null) =>
            MainMembers.Add(key, new CustomAccessor<Main>(getter, setter, name ?? key, memberType));

        private void AddPlayerField(string key, string name = null) => PlayerMembers.Add(key, new FieldAccessor<Player>(name ?? key));
        private void AddPlayerProperty(string key, string name = null) => PlayerMembers.Add(key, new PropertyAccessor<Player>(name ?? key));
        private void AddPlayerCustom(string key, Func<Player, object> getter, Action<Player, object> setter, Type memberType, string name = null) =>
            PlayerMembers.Add(key, new CustomAccessor<Player>(getter, setter, name ?? key, memberType));

        #endregion

    }

    public interface IMemberListener
    {
        #region Routing Information
        MemberRoute Route { get; set; }
        #endregion

        void Update();
        bool Completed();
    }

    public abstract class MemberListener : IMemberListener
    {
        #region Constructor
        public MemberListener(MemberRoute route)
        {
            Route = route;
        }
        #endregion

        #region Protected State
        protected bool Complete { get; set; } = false;
        #endregion

        #region IMemberListener
        public MemberRoute Route { get; set; }

        public virtual void Update()
        {
            Complete = true;
        }

        public bool Completed() => Complete;
        #endregion
    }

    public class MemberReader : MemberListener
    {
        #region Constructor
        public MemberReader(MemberRoute route) : base(route) { }
        #endregion

        #region IMemberListener
        public override void Update()
        {
            object value = ValueController.Instance.GetMember(Route);

            // Send value to server

            base.Update();
        }
        #endregion
    }

    public class MemberWriter : MemberListener
    {
        #region Set Details
        public MemberWriterValue WriterValue { get; set; }
        #endregion

        #region Constructor
        public MemberWriter(MemberRoute route, MemberWriterValue value) : base(route)
        {
            WriterValue = value;
        }

        public MemberWriter(MemberRoute route, object value) : base(route)
        {
            WriterValue = new MemberWriterValue(value, true);
        }
        #endregion

        #region IMemberListener
        public override void Update()
        {
            ValueController.Instance.SetMember(Route, WriterValue.Value);

            base.Update();
        }
        #endregion
    }

    /// <summary>
    /// Wrapper for values to set Terraria members to. Can either be
    /// a literal value or a reference to another member.
    /// </summary>
    public class MemberWriterValue
    {
        #region Public Properties
        public bool Literal { get; set; }
        public object Value
        {
            get
            {
                if (Literal) return mValue;

                return ValueController.Instance.GetMember(mRoute);
            }
        }
        #endregion

        #region Constructors
        public MemberWriterValue(object value, bool literal)
        {
            if (!literal && !(value is MemberRoute))
            {
                throw new ArgumentException($"Non-literal MemberWriterValue must use MemberRoute as value. Given '{value.GetType()}'");
            }
            if(literal)
            {
                mValue = value;
            }
            else
            {
                mRoute = (MemberRoute)value;
            }

            Literal = literal;
        }

        public MemberWriterValue(MemberRoute route)
        {
            mValue = route;
            Literal = false;
        }
        #endregion

        #region Private Properties
        private object mValue;
        private MemberRoute mRoute;
        #endregion
    }
}
