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
    /// <summary>
    /// Class that encapsulates a set of game variables and an
    /// action to perform on them (either reading or writing).
    /// </summary>
    public abstract class MemberListener
    {
        #region Routing Information
        public List<MemberRoute> Routes { get; set; }
        #endregion

        #region Constructor
        public MemberListener(MemberRoute route)
        {
            Routes = new List<MemberRoute>();
            Routes.Add(route);
        }

        public MemberListener(IEnumerable<MemberRoute> routes)
        {
            Routes = new List<MemberRoute>();
            Routes.AddRange(routes);
        }
        #endregion

        #region Abstract
        public abstract void Trigger();
        #endregion
    }

    /// <summary>
    /// MemberListener which reads the game variables and
    /// sends the data to the tOverseer
    /// </summary>
    public class MemberReader : MemberListener
    {
        #region Get Details
        /// <summary>
        /// Network protocol to use when sending data to the tOverseer
        /// </summary>
        public NetworkProtocol NetworkProtocol { get; set; }
        #endregion

        #region Constructor
        public MemberReader(MemberRoute route, NetworkProtocol sendingProtocol = NetworkProtocol.TCP) : base(route)
        {
            NetworkProtocol = sendingProtocol;
        }
        public MemberReader(IEnumerable<MemberRoute> routes, NetworkProtocol sendingProtocol = NetworkProtocol.TCP) : base(routes)
        {
            NetworkProtocol = sendingProtocol;
        }
        #endregion

        #region MemberListener
        public override void Trigger()
        {
            foreach(var route in Routes)
            {
                object value = ValueController.Instance.GetMember(route);

                // Pack the value into a response object, pairing it with
                // some identifiable information to make it meaningful,
                // and send to server, predicating on NetworkProtocol

                // Consider packing each result into one big object and
                // sending that after this loop
            }


        }
        #endregion
    }

    /// <summary>
    /// MemberListener that sets the game variables to a specific value.
    /// This value can either be a literal or it can be a reference
    /// to another game variable.
    /// </summary>
    public class MemberWriter : MemberListener
    {
        #region Set Details
        /// <summary>
        /// Container which will resolve the value to set the
        /// game variables to, whether it is a literal or another
        /// game variable.
        /// </summary>
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

        public MemberWriter(IEnumerable<MemberRoute> routes, MemberWriterValue value) : base(routes)
        {
            WriterValue = value;
        }

        public MemberWriter(IEnumerable<MemberRoute> routes, object value) : base(routes)
        {
            WriterValue = new MemberWriterValue(value, true);
        }
        #endregion

        #region MemberListener
        public override void Trigger()
        {
            if(Main.netMode != NetmodeID.MultiplayerClient)
            {
                foreach (var route in Routes)
                {
                    ValueController.Instance.SetMember(route, WriterValue.Value);
                }
            }
            else
            {
                Debugger.Log(0, "1", "\nMultiplayer Client attempted to set value.\n\n");
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
