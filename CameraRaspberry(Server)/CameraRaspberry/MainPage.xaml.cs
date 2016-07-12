using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UWPServerNS;
using CameraManagerNS;


// Il modello di elemento per la pagina vuota è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x410

namespace CameraRaspberry
{
    /// <summary>
    /// Pagina vuota che può essere utilizzata autonomamente oppure esplorata all'interno di un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int MAX_NUMBER_OF_SERVERS = 1;
        private CameraManagerNS.CameraManager _cameraManager = new CameraManagerNS.CameraManager();
        private int numberOfServers = 0;

        public MainPage()
        {
            this.InitializeComponent();

            Application.Current.Resuming += Application_Resuming;
            Application.Current.Suspending += Application_Suspending;
        }


        //When the app starts
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                await InitialiseCameraPreview();
                if (numberOfServers < MAX_NUMBER_OF_SERVERS)
                    startNewServerInThread();
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine("Unable to init the camera: " + ex.Message);
            }
            
        }

        //When the app resumes (after it has been suspended)
        private async void Application_Resuming(object sender, object o)
        {
            try
            {
                await InitialiseCameraPreview();
                if (numberOfServers < MAX_NUMBER_OF_SERVERS)
                    startNewServerInThread();
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine("Unable to init the camera: " + ex.Message);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //_cameraManager.Dispose();
        }

        private void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            try
            {
                _cameraManager.Dispose();
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine("Unable to dispose the camera: " + ex.Message);
            }
            
        }



        private async Task InitialiseCameraPreview()
        {
            await _cameraManager.InitialiseCameraAsync(await GetCamera());

            // Set the preview source for the CaptureElement
            PreviewControl.Source = _cameraManager.ViewFinder;

            // Start viewing through the CaptureElement 
            await _cameraManager.ViewFinder.StartPreviewAsync();
        }

        private async Task<DeviceInformation> GetCamera()
        {
            var rearCamera = await _cameraManager.GetCameraAtPanelLocation(Windows.Devices.Enumeration.Panel.Back);

            var defaultCamera = await _cameraManager.GetDefaultCamera();

            return rearCamera ?? defaultCamera;
        }

        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            startNewServerInThread();
        }

        private void startNewServerInThread()
        {
            Task.Factory.StartNew(() => startNewServer());
            numberOfServers++;
        }

        private void startNewServer()
        {
            UWPServerNS.UWPServer tcpServer = new UWPServer();
            tcpServer.StartListening();
            txtGeneric.Text = "Server creato!";
        }
    }
}
