using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsClient
{
    public class Client
    {

        //Connection parameters
        private int _connectionPort = -1;
        private string _connectionIP = string.Empty;
        private TcpClient _client;
        private NetworkStream myStream;
        private string _clientID;

        private const int BUFF_SIZE = 1024;


        public string ClientID {
            get { return _clientID; }
            set { } }

        public bool IsConnected
        {
            get { return _client.Connected; }
            set { }
        }

        //mre and timeout
        private ManualResetEvent _mreOpenSocket;
        private const int OPEN_SOCKET_TIMEOUT = 5000;

        //Events
        // Declare the event using EventHandler<T>
        public event CustomReadEventHandler MessageRead;
        public delegate void CustomReadEventHandler(string message);
        private void NotifyRead(string message)
        {
            // Make sure someone is listening to event
            if (MessageRead != null)
            {
                MessageRead(message);
            }
        }

        // Declare the event using EventHandler<T>
        public event CustomSentEventHandler MessageSent;
        public delegate void CustomSentEventHandler(string message);
        private void NotifySend(string message)
        {
            // Make sure someone is listening to event
            if (MessageSent != null)
            {
                MessageSent(message);
            }
        }


        public Client(string IP, int PORT)
        {
            _connectionIP = IP;
            _connectionPort = PORT;

            ClientID = Guid.NewGuid().ToString();
            _client = new TcpClient(_connectionIP, _connectionPort);
        }

        public bool Connect()
        {
            System.Threading.Thread.Sleep(200);
            if (_client.Connected)
                return true;


            try
            {
                OpenSocket();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void OpenSocket()
        {
            string method = "OpenSocket";
            try
            {
                _mreOpenSocket = new ManualResetEvent(false);
                _client.ConnectAsync(_connectionIP, _connectionPort);
                bool socketOpen = _mreOpenSocket.WaitOne(OPEN_SOCKET_TIMEOUT);
                if (socketOpen)
                {
                    myStream = _client.GetStream();
                }
                else {
                    //socket timeout
                    Log(method, "Timout when trying to open the socket!");
                    throw new Exception("Timout when trying to open the socket!");
                }
            }
            catch (Exception ex)
            {
                Log(method, "Error when opening the socket: " + ex.Message);
            }
        }

        public bool Disconnect()
        {
            try
            {
                CloseSocket();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private void CloseSocket()
        {
            string method = "CloseSocket";
            if (_client.Connected)
            {
                _client.Close();
                Log(method, "Socket closed.");
            }
            else
            {
                Log(method, "Socket is already closed!");
            }

            _client = null;
        }


        public bool SendMessage(string message)
        {
            message = message + "\0";
            NetworkStream stream = _client.GetStream();
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message);
            stream.WriteAsync(buffer, 0, buffer.Length);
            return true;
        }

        private void Log(string method, string message)
        {
            string time = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
            System.Diagnostics.Debug.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}","Client", time, method, message));
        }


    }
}
