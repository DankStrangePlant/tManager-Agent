using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TManagerAgent.Net
{
    public class MSG
    {
        #region Universal (alphabetized)
        public const string PING = "ping";
        public const string PING_RESPONSE = "ping-response";
        #endregion

        #region Sent (alphabetized)
        public const string CONNECT = "connect";
        public const string DISCONNECT = "disconnect";
        public const string MEMBER_VALUE = "member-value";
        public const string PLAYER_CONNECT = "player-connect";
        public const string PLAYER_DISCONNECT = "player-disconnect";
        #endregion

        #region Received (alphabetized)
        public const string SET = "set";
        public const string WATCH = "watch";
        #endregion
    }
}
