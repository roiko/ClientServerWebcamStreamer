using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace UWPServerNS
{
    public class UWPServer
    {

        private string LISTENING_PORT = "55555";
        private StreamSocketListener _streamSL;
        private ManualResetEvent _mreListeningOnPort;

        public UWPServer()
        {
            _mreListeningOnPort = new ManualResetEvent(false);
            _mreListeningOnPort.Reset();
            System.Diagnostics.Debug.WriteLine("Server UWPServer creato!");
        }

        public void StartListening()
        {
            while (true)
            { 
                _streamSL = new StreamSocketListener();
            _streamSL.Control.KeepAlive = true;
            Task theTask = Task.Factory.StartNew(()=> _streamSL.BindServiceNameAsync(LISTENING_PORT).AsTask());
            theTask.Wait();
            
            _streamSL.ConnectionReceived += _streamSL_ConnectionReceived;
            
                _mreListeningOnPort.Reset();
                System.Diagnostics.Debug.WriteLine("Server in attesa di connessioni...");
                _mreListeningOnPort.WaitOne();
                System.Diagnostics.Debug.WriteLine("Connesso!");
            }

            


        }

        private void _streamSL_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            _mreListeningOnPort.Set();
            System.Diagnostics.Debug.WriteLine("Client connesso!!!");
            //throw new NotImplementedException();
        }
    }
}
