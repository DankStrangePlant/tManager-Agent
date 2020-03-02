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

namespace TManagerAgent
{
    public class TManagerAgentWorld : ModWorld
    {
        TManagerAgent Mod { get; set; }

        public static string hostAddress = "127.0.0.1";
        public static int tcpPort = 31416;
        public static int udpPort = 31417;

        public ClientTCP ClientTCP;
        public ClientUDP ClientUDP;

        Stopwatch pingStopWatch = new Stopwatch();

        public List<AgentAction> ActiveActions { get; set; } = new List<AgentAction>();

        public override void Initialize()
        {
            base.Initialize();

            Mod = (TManagerAgent)ModLoader.GetMod("TerrariaManagementSuite");

            // Initialize connection to tOverseer
            ClientTCP = new ClientTCP(this);
            ClientTCP.OpenConnection(hostAddress, tcpPort);
            ClientTCP.SendString("Terraria greets you via TCP.\n" + Environment.StackTrace);

            ClientUDP = new ClientUDP(this);
            ClientUDP.OpenSocket(hostAddress, udpPort);
            ClientUDP.SendString("Terraria greets you via UDP.");

            switch (Main.netMode)
            {
                case NetmodeID.SinglePlayer:
                case NetmodeID.Server:
                    // allow everything
                    break;
                case NetmodeID.MultiplayerClient:
                default:
                    // disallow write, only allow read
                    break;
            }

        }

        public override void PostUpdate()
        {
            base.PostUpdate();

        }

        public void PreCloseWorld()
        {
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

                    if (p["command"].Equals("ping"))
                    {
                        pingStopWatch.Stop();
                        Main.NewText(pingStopWatch.ElapsedMilliseconds + " ms");
                    }

                }
                else
                {
                    Debugger.Log(0, "1", "\nSERVER SAYS NULL TERMINATOR\n\n");
                }
            }
        }

        //public void GetMessage(String message)
        //{
        //    //Parse read message here!
        //    //Will want to use this to add/remove elements to modActions List
        //    if (message != null)
        //    {
        //        if (message != "\0")
        //        {
        //            message = message.Trim('\0');

        //            Packet p = JsonConvert.DeserializeObject<Packet>(message);
        //            //TODO Deal with the packet here

        //            if (p["type"].Equals("misc"))
        //            {
        //                if (p["text"].Equals("ping"))
        //                {
        //                    pingStopWatch.Stop();
        //                    Main.NewText(pingStopWatch.ElapsedMilliseconds + " ms");
        //                }
        //            }

        //        }
        //        else
        //        {
        //            System.Diagnostics.Debugger.Log(0, "1", "\nSERVER SAYS NULL TERMINATOR\n\n");
        //        }
        //    }
        //}
    }
}
