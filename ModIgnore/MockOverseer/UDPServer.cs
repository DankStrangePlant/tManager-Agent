using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MockOverseer
{
    public class UDPServer
    {
        #region Public Delegate
        public delegate void MessageHandler(string message);

        public event MessageHandler OnMessageReceived;
        #endregion

        private UdpClient Server { get; set; }
        private IPEndPoint RemoteEP { get; set; }
        public UDPServer()
        {
            var localEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3008);
            Server = new UdpClient(localEP); // listen on localhost port 11001

            Task t = new Task(() =>
            {
                while (true)
                {
                    var remoteEP = new IPEndPoint(IPAddress.Any, 11000);
                    var data = Server.Receive(ref remoteEP);
                    RemoteEP = remoteEP; // cache whoever talked to us last

                    var message = Encoding.ASCII.GetString(data, 0, data.Length);

                    Console.WriteLine("Received message: " + message);
                    OnMessageReceived?.Invoke(message);
                }
            });
            t.Start();

        }

        public bool SendMessage(string message)
        {
            try
            {
                // Encode the data string into a byte array.  
                byte[] msg = Encoding.ASCII.GetBytes(message);

                // Send the data through the socket.  
                Server.Send(msg, msg.Length, RemoteEP);
            }
            catch { return false; }

            return true;
        }

        public bool SendObject(object obj) => SendMessage(JsonConvert.SerializeObject(obj));
    }
}
