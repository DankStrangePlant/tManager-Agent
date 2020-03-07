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

        TCPServer TCPServer { get; set; }
        UDPServer UDPServer { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            TCPServer = new TCPServer();

            TCPServer.OnConnect += TCPConnected;
            TCPServer.OnDisconnect += TCPDisconnected;
            TCPServer.OnMessageReceived += TCPMessageReceived;

            // TCP server control loop
            Task tcpServerLoop = new Task(() =>
            {
                while (true)
                {
                    TCPServer.InitServer();
                    TCPServer.WaitForConnection();
                    TCPServer.Listen();
                }
            });
            tcpServerLoop.Start();

            // This is enough to init the udp server
            UDPServer = new UDPServer();
            UDPServer.OnMessageReceived += UDPMessageReceived;
            UDPListening();
        }

        #region TCP events
        public void TCPConnected()
        {
            Dispatcher.Invoke(() => { TCPConnectedText.Text = "Connected"; });
        }

        public void TCPDisconnected()
        {
            Dispatcher.Invoke(() => { TCPConnectedText.Text = "Waiting for connection..."; });
        }

        public void TCPMessageReceived(string message)
        {
            Dispatcher.Invoke(() => { TCPLastMessageReceivedText.Text = message; });


        }
        #endregion

        #region UDP Events
        public void UDPListening()
        {
            Dispatcher.Invoke(() => { UDPListeningText.Text = "Listening..."; });
        }

        public void UDPMessageReceived(string message)
        {
            Dispatcher.Invoke(() => { UDPLastMessageReceivedText.Text = message; });


        }
        #endregion

        #region Send
        public void TCPSendMessage(string message)
        {
            TCPServer.SendMessage(message);
            Dispatcher.Invoke(() => { TCPLastMessageSentText.Text = message; });
        }

        public void UDPSendMessage(string message)
        {
            UDPServer.SendMessage(message);
            Dispatcher.Invoke(() => { UDPLastMessageSentText.Text = message; });
        }
        #endregion

        #region Window Events
        private void TCP_Ping_Button_Click(object sender, RoutedEventArgs e)
        {
            TCPSendMessage("{ \"msg\":\"ping\" }");
        }

        private void TCP_Disconnect_Button_Click(object sender, RoutedEventArgs e)
        {
            TCPServer.Disconnect();
        }
        #endregion
    }
}
