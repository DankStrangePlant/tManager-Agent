using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Newtonsoft.Json;
using tManagerAgent.Net;
using Terraria.ID;
using tManagerAgent.Core;
using static Terraria.ModLoader.ModContent;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace tManagerAgent
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
            ClientTCP = new ClientTCP(Mod.OverseerAddress, Mod.OverseerTCPPort);
            ClientTCP.OnMessageReceived += ParseServerMessage;
            bool success = ClientTCP.TryConnect();
            if(success)
            {
                ClientTCP.StartListening();
                ClientTCP.SendMessage("Terraria greets you via TCP.");
            }


            ClientUDP = new ClientUDP(Mod.OverseerAddress, Mod.OverseerUDPPort);
            ClientUDP.OnMessageReceived += ParseServerUDPMessage;
            ClientUDP.Connect();
            ClientUDP.StartListening();
            ClientUDP.SendMessage("Terraria greets you via UDP.");
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

        }

        public void PreCloseWorld()
        {
            ClientTCP.OnMessageReceived -= ParseServerMessage;
            ClientUDP.OnMessageReceived -= ParseServerUDPMessage;

            ClientTCP.SendMessage("Closing World: " + Main.worldName);
            if (ClientTCP.Connected)
            {
                ClientTCP.Disconnect();
            }
            if(ClientUDP.Listening)
            {
                ClientUDP.Disconnect();
            }
        }

        public void ParseServerMessage(string message)
        {
            if (message != null)
            {
                if (message != "\0")
                {
                    message = message.Trim('\0');

                    try
                    {
                        Packet p = JsonConvert.DeserializeObject<Packet>(message);

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
                    catch
                    {
                        UnsupportedDataReceived(message);
                    }
                    

                }
                else
                {
                    Debugger.Log(0, "1", "\nSERVER SAYS NULL TERMINATOR\n\n");
                }
            }
        }

        public void ParseServerUDPMessage(string message)
        {
            // maybe do something different here
            ParseServerMessage(message);
        }

        #region Handlers

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

        static Random rand = new Random();
        private void UnsupportedDataReceived(string data)
        {
            Main.NewText($"Overseer said: {data}");
            ClientTCP.SendMessage("Sorry server, I didn't understand that." + rand.Next());
        }

        #endregion
    }
}
