using System;
using System.Net;
using System.Net.Sockets;

namespace TManagerAgent.Net
{
    public class ClientTCP
    {
        byte[] m_dataBuffer = new byte[10];
        IAsyncResult m_result;
        public AsyncCallback m_pfnCallBack;
        public Socket socketTCP;

        private TManagerAgentWorld WorldInstance;

        public ClientTCP(TManagerAgentWorld world)
        {
            WorldInstance = world;
        }

        public void CloseConnection()
        {
            if (socketTCP != null)
            {
                socketTCP.Shutdown(SocketShutdown.Both);
                socketTCP.Dispose();
                socketTCP = null;
            }
        }

        public void OpenConnection(string hostName, int port)
        {
            try
            {
                // Create the socket instance
                socketTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Cet the remote IP address
                IPAddress ip = IPAddress.Parse(hostName);
                // Create the end point 
                IPEndPoint ipEnd = new IPEndPoint(ip, port);
                // Connect to the remote host
                socketTCP.Connect(ipEnd);
                if (socketTCP.Connected)
                {
                    //Wait for data asynchronously 
                    WaitForData();
                }
            }
            catch (SocketException se)
            {
                string str;
                str = "\nConnection failed, is the server running?\n" + se.Message;
                Console.WriteLine(str);
            }
        }

        public void SendMessage(Packet p)
        {
            SendString(p.Serialize());
        }

        public void SendString(string message)
        {
            if (!Connected()) return; // Do nothing if we're not connected
            try
            {
                object objData = message;
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                if (socketTCP != null)
                {
                    socketTCP.Send(byData);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        public void WaitForData()
        {
            try
            {
                if (m_pfnCallBack == null)
                {
                    m_pfnCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.thisSocket = socketTCP;
                // Start listening to the data asynchronously
                if (socketTCP != null)
                    m_result = socketTCP.BeginReceive(theSocPkt.dataBuffer,
                                                            0, theSocPkt.dataBuffer.Length,
                                                            SocketFlags.None,
                                                            m_pfnCallBack,
                                                            theSocPkt);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }

        }

        public class SocketPacket
        {
            public Socket thisSocket;
            public byte[] dataBuffer = new byte[256];
        }

        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket theSockId = (SocketPacket)asyn.AsyncState;
                int iRx = theSockId.thisSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(theSockId.dataBuffer, 0, iRx, chars, 0);
                string szData = new string(chars);

                WorldInstance.ParseServerMessage(szData);

                WaitForData();
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        public bool Connected()
        {
            return socketTCP.Connected;
        }
    }
}
