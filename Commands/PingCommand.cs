using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace TManagerAgent.Commands
{
    public class PingCommand : ModCommand
    {
        public override string Command => "ping";

        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            caller.Reply("Here is where I would ping the server... IF I HAD ONE!");
        }
    }
}
