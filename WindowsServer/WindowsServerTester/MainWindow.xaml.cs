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
using WindowsServer;


namespace WindowsServerTester
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Task task = new Task(StartServerInThread);
            task.Start();
        }

        public void StartServerInThread()
        {
            //int port = new Random().Next(9000, 10000);
            int port = 9999;
            Server theServer = new Server(port);
            theServer.ThereIsMessage += MessageReceived;
            theServer.StartListening();

        }

        private void MessageReceived(string message)
        {
            Console.WriteLine("Sono il pulsante e ho ricevuto l'evento server: " + message);
        }
    }
}
