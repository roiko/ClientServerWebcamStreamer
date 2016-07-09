using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace WindowsServer
{
   

    public class Server
    {

        // State object for reading client data asynchronously
        protected class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        private Thread _tRead;
        private int _connectionCount = 0;

        // Thread signal.
        public ManualResetEvent allDone = new ManualResetEvent(false);
        private int listeningPort = -1;


        // Declare the event using EventHandler<T>
        public event CustomEventHandler ThereIsMessage;
        public delegate void CustomEventHandler(string message);
        private void Notify(string message)
        {
            // Make sure someone is listening to event
            if (ThereIsMessage != null)
            {
               ThereIsMessage(message);
            }
        }


        public Server(int port)
        {
            System.Threading.Thread.Sleep(3000);
            listeningPort = port;
        }


        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, this.listeningPort);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection on port "+listeningPort+"...") ;
                    Notify("Waiting for a connection on port "+listeningPort+"...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Notify("\nPress ENTER to continue...");
            Console.Read();

        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            IPEndPoint remoteEP = (IPEndPoint)handler.RemoteEndPoint;
            
            Notify(string.Format("Client {0} Connected!", remoteEP.Address.ToString()));

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;

            _tRead = new Thread(()=> keepReading(state, handler));
            _tRead.Name = "Thread_" + _connectionCount;
            _tRead.Start();
            _connectionCount++;
            //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        private void keepReading(StateObject state, Socket handler)
        {
            while (true) //condition for stop thread here
            {
                try
                {
                    int read = handler.Receive(state.buffer);
                    string recMessage = Encoding.ASCII.GetString(state.buffer, 0, read);
                    Console.WriteLine("[Thread {2}]Read {0} bytes from socket. Data : {1}\n",
                            recMessage, read, System.Threading.Thread.CurrentThread.Name);

                }
                catch (System.Net.Sockets.SocketException e)
                {
                    if (!handler.Connected)
                    {
                        Console.WriteLine(string.Format("[Thread {0}] Connection closed!", System.Threading.Thread.CurrentThread.Name));
                        break;
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
                
            }

            Console.WriteLine(string.Format("[Thread {0}] Ended!", System.Threading.Thread.CurrentThread.Name));
        }
        

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("\0") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    content.Remove(content.IndexOf('\0'), 1);
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    // Echo the data back to the client.
                    Send(handler, content);
                }
                else {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}