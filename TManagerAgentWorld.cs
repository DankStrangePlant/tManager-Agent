using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Newtonsoft.Json;
using TManagerAgent.Net;
using Terraria.ID;
using TManagerAgent.Core;
using static Terraria.ModLoader.ModContent;

namespace TManagerAgent
{
    public class TManagerAgentWorld : ModWorld
    {
        TManagerAgent Mod { get; set; }

        public ClientTCP ClientTCP;
        public ClientUDP ClientUDP;

        Stopwatch pingStopWatch = new Stopwatch();

        public List<AgentAction> ActiveActions { get; set; } = new List<AgentAction>();

        public override void Initialize()
        {
            base.Initialize();

            Mod = GetInstance<TManagerAgent>();

            // Initialize connection to tOverseer
            ClientTCP = new ClientTCP();
            ClientTCP.DataReceived += ParseServerMessage;

            ClientTCP.OpenConnection(Mod.OverseerAddress, Mod.OverseerTCPPort);
            ClientTCP.SendString("Terraria greets you via TCP.\n" + Environment.StackTrace);

            ClientUDP = new ClientUDP();
            ClientUDP.OpenSocket(Mod.OverseerAddress, Mod.OverseerTCPPort);
            ClientUDP.SendString("Terraria greets you via UDP.");
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

        }

        public void PreCloseWorld()
        {
            ClientTCP.DataReceived -= ParseServerMessage;

            ClientTCP.SendString("Closing World: " + Main.worldName);
            if (ClientTCP.Connected())
                ClientTCP.CloseConnection();
            ClientUDP.CloseSocket();
        }

        public void ParseServerMessage(string message)
        {
            Main.NewText("Server said: " + message);
            if (message != null)
            {
                if (message != "\0")
                {
                    message = message.Trim('\0');

                    Packet p = JsonConvert.DeserializeObject<Packet>(message);
                    //TODO Deal with the packet here

                    switch (p[Packet.MSG_FIELD])
                    {
                        case MSG.PING:
                            Handle_PING(p);
                            break;
                        case MSG.PING_RESPONSE:
                            Handle_PING_RESPONSE(p);
                            break;
                        case MSG.SET:
                            Handle_SET(p);
                            break;
                        case MSG.WATCH:
                            Handle_WATCH(p);
                            break;
                        default:
                            UnsupportedMsgReceived(p);
                            break;
                    }

                }
                else
                {
                    Debugger.Log(0, "1", "\nSERVER SAYS NULL TERMINATOR\n\n");
                }
            }
        }

        private void Handle_PING(Packet data)
        {
            // TODO respond to the Overseer with PING_RESPONSE
        }

        private void Handle_PING_RESPONSE(Packet data)
        {
            pingStopWatch.Stop();
            Main.NewText($"Server response: {pingStopWatch.ElapsedMilliseconds}ms");
        }

        private void Handle_SET(Packet data)
        {
            // TODO
        }

        private void Handle_WATCH(Packet data)
        {
            // TODO
            
        }

        private void UnsupportedMsgReceived(Packet data)
        {
            // For debugging purposes, just display unsupported message in game chat
            Main.NewText($"Unsupported message '{data[Packet.MSG_FIELD]}' received from server.");
        }
    }
}
