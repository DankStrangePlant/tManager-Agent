using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MockOverseer
{

    public class TCPServer
    {
        Socket Server { get; set; }
        Socket Connection { get; set; }
        IPEndPoint LocalEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3007);

        int Count { get; set; } = 1;


        #region Public Delegate
        public delegate void ConnectCallback();
        public delegate void DisconnectCallback();
        public delegate void MessageHandler(string message);

        public event ConnectCallback OnConnect;
        public event DisconnectCallback OnDisconnect;
        public event MessageHandler OnMessageReceived;
        #endregion

        public void InitServer()
        {
            if (Server != null)
            {
                Server.Close();
            }

            // Create a TCP/IP  socket.  
            Server = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            Server.Bind(LocalEP);
            Server.Listen(10);
        }

        public void WaitForConnection()
        {
            Console.WriteLine("Waiting for a connection...");
            // Program is suspended while waiting for an incoming connection.  
            Connection = Server.Accept();
            OnConnect?.Invoke();
            Console.WriteLine("Connection accepted!");
        }

        private bool KeepListening = true;
        public void Listen()
        {
            KeepListening = true;
            // Buffer for incoming data
            byte[] bytes = new byte[1024];

            try
            {
                while (KeepListening)
                {
                    // Timeout timer
                    //CancellationTokenSource timeoutCancelSource = new CancellationTokenSource();

                    //Task timeoutTask = new Task(() =>
                    //{
                    //    for (int count = 0; count < 5; count++)
                    //    {
                    //        Thread.Sleep(1000);
                    //        if (timeoutCancelSource.Token.IsCancellationRequested)
                    //        {
                    //            return;
                    //        }
                    //    }
                    //    Disconnect();
                    //    KeepListening = false;
                    //    Console.WriteLine("Client closed due to timeout.");
                    //}, timeoutCancelSource.Token);

                    //// start timeout countdown
                    //timeoutTask.Start();

                    // Receive the response from the client
                    int bytesRec = 0;
                    while(bytesRec == 0)
                    {
                        bytesRec = Connection.Receive(bytes);
                    }

                    // Cancel timeout countdown
                    //timeoutCancelSource.Cancel();

                    string message = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    OnMessageReceived?.Invoke(message);
                }
            }
            catch { }


        }

        public void Disconnect()
        {
            Console.WriteLine("Shutting down connection socket");
            try
            {
                Connection.Shutdown(SocketShutdown.Both);
                Connection.Close();
                OnDisconnect?.Invoke();
            }
            catch { }
        }

        public bool SendMessage(string message)
        {
            try
            {
                // Encode the data string into a byte array.  
                byte[] msg = Encoding.ASCII.GetBytes(message);

                // Send the data through the socket.  
                Connection.Send(msg);
            }
            catch { return false; }

            return true;
        }

        public bool SendObject(object obj) => SendMessage(JsonConvert.SerializeObject(obj));
    }
}
