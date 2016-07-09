using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using WindowsClient;
using System.Diagnostics;

namespace WindowsClientTester
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Client theClient;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            int port = int.Parse(txtPORT.Text);
            theClient = new Client(txtIP.Text, port);
            theClient.MessageRead += Wclient_MessageRead;
            theClient.MessageSent += Wclient_MessageSent;

            bool connectionRes = theClient.Connect();
            if (connectionRes)
            {
                Debug.WriteLine("Il client è riuscito a connettersi!");
            }
            else
            {
                Debug.WriteLine("Il client non è riuscito a connettersi!");
            }

        }

        private void Wclient_MessageSent(string message)
        {
            System.Diagnostics.Debug.WriteLine("Tester - il client ha inviato: " + message);
        }

        private void Wclient_MessageRead(string message)
        {
            System.Diagnostics.Debug.WriteLine("Tester - il client ha ricevuto: " + message);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (theClient == null)
                MessageBox.Show("Client non inizializzato!!!");
            string message = txtMessage.Text;
            theClient.SendMessage(message);
        }
    }
}
