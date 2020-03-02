using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TManagerAgent.Core
{
    /// <summary>
    /// Simple class that is used to determine if a given routed game variable
    /// is equal to a desired value at some point in time.
    /// 
    /// TODO - Support comparison between 2 game variables instead of only
    /// comparing a game variable to a literal
    /// </summary>
    public class GameCondition
    {
        #region Properties
        /// <summary>
        /// Game variable to query
        /// </summary>
        public MemberRoute Route { get; set; }

        /// <summary>
        /// Value the game variable must be for Evaluate() to return true
        /// </summary>
        public object ExpectedValue { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Evaluate this GameCondition
        /// </summary>
        /// <returns>True if the routed game variable currently == the given expected value</returns>
        public bool Evaluate() => ValueController.Instance.GetMember(Route) == ExpectedValue;
        #endregion
    }
}
