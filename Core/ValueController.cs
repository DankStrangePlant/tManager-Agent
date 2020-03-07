using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Map;

namespace tManagerAgent.Core
{
    public class ValueController
    {

        #region API

        #region Get
        public object GetMember(MemberRoute route)
        {
            if(route.Instance)
            {
                return GetMember(route.Context, route.AccessorKey);
            }
            return GetStaticMember(route.Context.ContextType, route.AccessorKey);
        }

        public object GetMember(MemberContext context, string key)
        {
            switch(context.ContextType)
            {
                case MemberContextType.World:
                    return GetWorldMember(context.ResolvedContext as WorldMap, key);
                case MemberContextType.Player:
                    return GetPlayerMember(context.ResolvedContext as Player, key);
                default:
                    throw new ArgumentException($"Cannot get non-static member in context '{context.ContextType}'");
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
                SetMember(route.Context, route.AccessorKey, value);
            }
            SetStaticMember(route.Context.ContextType, route.AccessorKey, value);
        }

        public void SetMember(MemberContext context, string key, object value)
        {
            switch (context.ContextType)
            {
                case MemberContextType.World:
                    SetWorldMember(context.ResolvedContext as WorldMap, key, value);
                    break;
                case MemberContextType.Player:
                    SetPlayerMember(context.ResolvedContext as Player, key, value);
                    break;
                default:
                    throw new ArgumentException($"Cannot set non-static member in context '{context.ContextType}'");
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
        /// <summary>
        /// This is currently where we actually add entries for each
        /// game variable we want to expose.
        /// </summary>
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
}
