using System;
using System.Net;
using System.Net.Sockets;

namespace TManagerAgent.Net
{
    public class ClientUDP : IDisposable
    {
        byte[] m_dataBuffer = new byte[256];
        public Socket socketUDP;
        private IPEndPoint endPoint;

        private static string targetKey;

        public static void SetTargetKey(string target)
        {
            targetKey = target;
        }

        public void setEndPointIP(string name)
        {
            IPAddress ip = IPAddress.Parse(name);
            endPoint.Address = ip;
        }

        public void setEndPointPort(int port)
        {
            endPoint.Port = port;
        }

        public void CloseSocket()
        {
            if (socketUDP != null)
            {
                socketUDP.Shutdown(SocketShutdown.Both);
                socketUDP.Dispose();
                socketUDP = null;
            }
        }

        public void OpenSocket(string name, int port)
        {
            // Create the socket instance
            socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //// Cet the remote IP address
            IPAddress ip = IPAddress.Parse(name);
            //// Create the end point 
            endPoint = new IPEndPoint(ip, port);
        }

        public void SendMessage(Packet p)
        {
            SendString(p.Serialize());
        }

        public void SendString(string message)
        {
            try
            {
                object objData = message;
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                if (socketUDP != null)
                {
                    socketUDP.SendTo(byData, endPoint);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        public void BroadcastMessage(string message)
        {
            try
            {
                object objData = message;
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                if (socketUDP != null)
                {
                    socketUDP.Send(byData);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        public void Dispose()
        {
            ((IDisposable)socketUDP).Dispose();
        }
    }
}
