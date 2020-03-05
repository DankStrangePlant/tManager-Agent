using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using TManagerAgent.Net;

namespace TManagerAgent.Core
{
    public abstract class AgentAction
    {
        public MemberRoute Route { get; set; }

        public AgentAction(MemberRoute route)
        {
            Route = route;
        }

        public abstract void Trigger();
        public abstract void Update();
    }

    /// <summary>
    /// Action that can last a long time. Will report a specified
    /// game variable's value to the Overseer every <see cref="TickFrequency"/> ticks.
    /// 
    /// If <see cref="HighPriority"/>, will send over TCP. Otherwise UDP.
    /// </summary>
    public class WatchMember : AgentAction
    {
        #region Public
        public long TickFrequency { get; set; }
        public bool HighPriority { get; set; }
        #endregion

        #region Private
        private long TickCount { get; set; } = 0;
        /// <summary>
        /// Network protocol to use when sending data to the tOverseer
        /// </summary>
        private NetworkProtocol NetworkProtocol =>
            HighPriority ? NetworkProtocol.TCP : NetworkProtocol.UDP;
        #endregion

        public WatchMember(MemberRoute route, long tickFrequency) : base(route)
        {
            TickFrequency = tickFrequency;
        }

        #region Override
        public override void Update()
        {
            if(TickCount == 0)
            {
                Trigger();
            }
            TickCount = (TickCount + 1) % TickFrequency;
        }

        public override void Trigger()
        {
            object value = ValueController.Instance.GetMember(Route);

            // Pack the value into a response object, pairing it with
            // some identifiable information to make it meaningful,
            // and send to server, predicating on NetworkProtocol
        }
        #endregion
    }

    public class WriteMember : AgentAction
    {
        public MemberWriterValue Value { get; set; }

        public WriteMember(MemberRoute route, MemberWriterValue value) : base(route)
        {
            Value = value;
        }

        #region Override
        public override void Update() { } // Do nothing

        public override void Trigger()
        {
            // Disallow write if our context is a multiplayer client
            // We only allow write from multiplayer server or in singleplayer
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                ValueController.Instance.SetMember(Route, Value.Value);
            }
            else
            {
                Debugger.Log(0, "1", "\nMultiplayer client attempted to set value.\n\n");
            }
        }
        #endregion
    }

    /// <summary>
    /// Wrapper for values to set game variables to. Can either be
    /// a literal value or a reference to another member.
    /// </summary>
    public class MemberWriterValue
    {
        #region Public Properties
        /// <summary>
        /// Whether or not to evaluate the object as a literal.
        /// If false, will be resolved as a MemberRoute.
        /// </summary>
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
            if (literal)
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
