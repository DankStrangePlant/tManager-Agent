using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TManagerAgent.Core
{
    public abstract class AgentAction
    {
        public MemberListener MemberListener { get; set; }

        public virtual bool Finished =>
            (TimesToExecute > 0 && TimesExecuted >= TimesToExecute) ||
            Cancelled;
        public bool Cancelled { get; set; } = false;

        /// <summary>
        /// Number of times to execute this. -1 means unlimited
        /// </summary>
        public long TimesToExecute { get; set; }
        public long TimesExecuted { get; set; } = 0;

        public List<GameCondition> Conditions { get; set; } = new List<GameCondition>();

        public AgentAction(MemberListener memberListener, long timesToExecute = -1, IEnumerable<GameCondition> conditions = null)
        {
            MemberListener = memberListener;
            TimesToExecute = timesToExecute;
            if (conditions != null)
            {
                Conditions.AddRange(conditions);
            }
        }

        public abstract void Evaluate();
        public abstract void Update();
    }

    public class RepeatingAction : AgentAction
    {
        #region Properties
        public long TickInterval { get; set; }
        public long TickCounter { get; set; }
        #endregion

        #region Constructors
        public RepeatingAction(MemberListener memberListener,
            long tickInterval,
            long timesToExecute = -1,
            IEnumerable<GameCondition> conditions = null) : base(memberListener, timesToExecute, conditions)
        {
            TickInterval = tickInterval;
        }
        #endregion

        #region override
        public override void Evaluate()
        {
            if(TickCounter == 0)
            {
                // Only if all conditions evaluate to true...
                if(Conditions.All(cond => cond.Evaluate()))
                {
                    MemberListener.Trigger();
                    TimesExecuted++;
                }
            }
        }

        public override void Update()
        {
            if(!Finished)
            {
                Evaluate();
                TickCounter = (TickCounter + 1) % TickInterval;
            }
        }
        #endregion
    }

    // TODO - be able to attach this class to various hooks/events
    public class TriggeredAction : AgentAction
    {
        // TODO - some representation of hooks/events

        #region Constructors
        public TriggeredAction(MemberListener memberListener,
            long timesToExecute = -1,
            IEnumerable<GameCondition> conditions = null) : base(memberListener, timesToExecute, conditions)
        {
            // TODO - accept input about what hooks/events to attach to
            throw new NotImplementedException();
        }
        #endregion

        #region override
        public override void Evaluate()
        {
            if(!Finished && Conditions.All(cond => cond.Evaluate()))
            {
                MemberListener.Trigger();
                TimesExecuted++;
            }
        }

        public override void Update() { /* Do nothing */ }
        #endregion
    }

}
