using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TManagerAgent.Core
{
    public struct MemberRoute
    {
        public MemberContextType ContextType { get; set; }
        public object Context { get; set; }
        public string AccessorKey { get; set; }
        public bool Instance { get; set; }

        #region Constructor
        public MemberRoute(MemberContextType contextType, object context, string key, bool instance = true)
        {
            ContextType = contextType;
            Context = context;
            AccessorKey = key;
            Instance = instance;
        }
        #endregion
    }

    public enum MemberContextType
    {
        Main,
        World,
        Player
    }
}
