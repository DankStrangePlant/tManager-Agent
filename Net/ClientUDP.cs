using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tManagerAgent.Net
{
    /// <summary>
    /// Wrapper around a UDP socket which listens asynchronously
    /// and can also send messages synchronously.
    /// </summary>
    public class ClientUDP
    {
        #region Public Delegate
        public delegate void MessageHandler(string message);

        public event MessageHandler OnMessageReceived;
        #endregion

        #region Public Properties
        public bool Connected => Client != null && Client.Connected;
        public bool Listening => CurrentListenLoop != null && CurrentListenLoop.Status == TaskStatus.Running;
        #endregion

        #region Constructors
        public ClientUDP(string remoteHost, ushort remotePort) :
            this(new IPEndPoint(IPAddress.Parse(remoteHost), remotePort))
        { }

        public ClientUDP(IPEndPoint endpoint)
        {
            RemoteEP = endpoint;

            ReInit();
        }
        #endregion

        #region Public Functions
        public void Reconfigure(string remoteHost, ushort remotePort) =>
            new IPEndPoint(IPAddress.Parse(remoteHost), remotePort);

        public void Reconfigure(IPEndPoint endpoint)
        {
            RemoteEP = endpoint;
        }

        public void ReInit()
        {
            Client = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);
        }

        public void Connect()
        {
            Client.Connect(RemoteEP);
        }

        /// <summary>
        /// Start asynchronously listening for messages
        /// </summary>
        public void StartListening()
        {
            StopListenTokenSource = new CancellationTokenSource();

            CurrentListenLoop = new Task(ListenLoop, StopListenTokenSource.Token);
            CurrentListenLoop.Start();
        }

        public void StopListening()
        {
            if (Listening)
            {
                StopListenTokenSource.Cancel();
            }
        }

        public bool SendMessage(string message)
        {
            try
            {
                // Encode the data string into a byte array.  
                byte[] msg = Encoding.ASCII.GetBytes(message);

                // Send the data through the socket.  
                Client.Send(msg);
            }
            catch { return false; }

            return true;
        }

        public bool SendObject(object obj) => SendMessage(JsonConvert.SerializeObject(obj));
        #endregion

        #region Private
        private Socket Client { get; set; }
        private IPEndPoint RemoteEP { get; set; }


        private Task CurrentListenLoop { get; set; }
        private CancellationTokenSource StopListenTokenSource { get; set; }

        private void ListenLoop()
        {
            CancellationToken cancelToken = StopListenTokenSource.Token;

            // Buffer for incoming data
            byte[] bytes = new byte[1024];


            try
            {
                while (true)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    // Timeout timer
                    CancellationTokenSource timeoutCancelSource = new CancellationTokenSource();

                    Task timeoutTask = new Task(() =>
                    {
                        for (int count = 0; count < 5; count++)
                        {
                            Thread.Sleep(1000);
                            if (timeoutCancelSource.Token.IsCancellationRequested)
                            {
                                return;
                            }
                        }
                        Disconnect();
                    }, timeoutCancelSource.Token);

                    // start timeout countdown
                    timeoutTask.Start();

                    // Receive the response from the remote device.  
                    int bytesRec = Client.Receive(bytes);

                    // Cancel timeout countdown
                    timeoutCancelSource.Cancel();

                    string message = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    OnMessageReceived?.Invoke(message);
                }
            }
            catch { }
        }

        /// <summary>
        /// Shutdown and close the client socket.
        /// </summary>
        public void Disconnect()
        {
            StopListening();
            Client.Shutdown(SocketShutdown.Both);
            Client.Close();
        }
        #endregion
    }
}
