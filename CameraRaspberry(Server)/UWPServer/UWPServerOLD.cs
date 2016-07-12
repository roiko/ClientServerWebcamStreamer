using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Windows.Networking.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.IO;


namespace UWPServerNS
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.GetDeferral();
            UWPServer_OLD server = new UWPServer_OLD();
            while (server.Start() == true)
            { }
        }
    }


    public class UWPServer_OLD
    {
        private ManualResetEvent _mreAcceptConnection;
        private const string LISTENING_PORT = "9000";
        private const uint BufferSize = 8192;
        public bool Start()
        {
            try
            {
                _mreAcceptConnection = new ManualResetEvent(false);
                StreamSocketListener listener = new StreamSocketListener();
                listener.BindServiceNameAsync(LISTENING_PORT).AsTask();
                System.Diagnostics.Debug.WriteLine("Socket listening on port " + LISTENING_PORT);
                listener.ConnectionReceived += Listener_ConnectionReceived;
                while (true)
                {
                    _mreAcceptConnection.Reset();
                    _mreAcceptConnection.WaitOne(); //wait until next connection
                }
                

                return true;

            }

            catch (Exception)

            {

                return false;

            }

        }

        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Connection received!");
            StringBuilder request = new StringBuilder();
            using (Windows.Storage.Streams.IInputStream input = args.Socket.InputStream)
            {
                _mreAcceptConnection.Set();
                byte[] data = new byte[BufferSize];
                Windows.Storage.Streams.IBuffer buffer = data.AsBuffer();
                uint dataRead = BufferSize;
                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    System.Diagnostics.Debug.WriteLine("Received data: " + request.ToString());
                    dataRead = buffer.Length;
                }
                //In the future, maybe we parse the HTTP request and serve different HTML pages for now we just always push index.html
            }
        }

    }
}
