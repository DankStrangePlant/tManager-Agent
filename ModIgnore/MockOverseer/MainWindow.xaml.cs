using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MockOverseer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string LOCALHOST = "127.0.0.1";
        public const ushort TCP_LISTENING_PORT = 3007;
        public const ushort UDP_LISTENING_PORT = 3008;

        public Socket MySocket;
        public Socket ServerSocket;
        IAsyncResult m_result;

        public MainWindow()
        {
            InitializeComponent();

            new Task(() => StartTCPSocket()).Start();
        }

        public void StartTCPSocket()
        {
            // Establish the local endpoint  
            // for the socket. Dns.GetHostName 
            // returns the name of the host  
            // running the application. 
            IPAddress ipAddr = IPAddress.Parse(LOCALHOST);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, TCP_LISTENING_PORT);

            // Creation TCP/IP Socket using  
            // Socket Class Costructor 
            MySocket = new Socket(ipAddr.AddressFamily,
                         SocketType.Stream, ProtocolType.Tcp);
            try
            {

                // Using Bind() method we associate a 
                // network address to the Server Socket 
                // All client that will connect to this  
                // Server Socket must know this network 
                // Address 
                MySocket.Bind(localEndPoint);

                // Using Listen() method we create  
                // the Client list that will want 
                // to connect to Server 
                MySocket.Listen(1);

                Console.WriteLine("Waiting connection ... ");

                // Suspend while waiting for 
                // incoming connection Using  
                // Accept() method the server  
                // will accept connection of client 
                ServerSocket = MySocket.Accept();

                Dispatcher.Invoke(() => { ConnectedText.Visibility = Visibility.Visible; });
                Dispatcher.Invoke(() => { LastMessageText.Visibility = Visibility.Visible; });

                WaitForData();
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public AsyncCallback m_pfnCallBack;
        public void WaitForData()
        {
            try
            {
                if (m_pfnCallBack == null)
                {
                    m_pfnCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.thisSocket = ServerSocket;
                // Start listening to the data asynchronously
                if (ServerSocket != null)
                    m_result = ServerSocket.BeginReceive(theSocPkt.dataBuffer,
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

                Debugger.Log(0, "1", $"Received data: {szData}");
                Dispatcher.Invoke(() => { LastMessageText.Text = $"Last message received: {szData}"; });

                Task.Delay(5000).ContinueWith((_) => {
                    SendData("Hi client! I got your message!");
                });


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

        public void SendData(string str)
        {
            byte[] message = Encoding.ASCII.GetBytes(str);

            // Send a message to Client  
            // using Send() method 
            ServerSocket.Send(message);
        }

        public void SendObject(object data)
        {
            SendData(JsonConvert.SerializeObject(data));
        }
    }
    public class SocketPacket
    {
        public Socket thisSocket;
        public byte[] dataBuffer = new byte[256];
    }
}
