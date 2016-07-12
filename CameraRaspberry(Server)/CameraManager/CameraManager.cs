using Magellanic.Camera.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;

namespace CameraManagerNS
{
    public class CameraManager : ICameraDevice
    {

        public MediaCapture ViewFinder { get; set; }

        public CameraManager()
        {
            bool init = true;
        }

        public void Dispose()
        {
            ViewFinder?.Dispose();
            ViewFinder = null;
        }

        public async Task<DeviceInformation> GetCameraAtPanelLocation(Panel cameraPosition)
        {
            var cameraDevices = await GetCameraDevices();

            return cameraDevices.FirstOrDefault(c => c.EnclosureLocation?.Panel == cameraPosition);
        }

        public async Task<DeviceInformation> GetDefaultCamera()
        {
            var cameraDevices = await GetCameraDevices();

            return cameraDevices.FirstOrDefault();
        }

        public async Task InitialiseCameraAsync(DeviceInformation cameraToInitialise)
        {
            ViewFinder = new MediaCapture();
            await ViewFinder.InitializeAsync(
                new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = cameraToInitialise.Id
                });

        }

        private async Task<DeviceInformationCollection> GetCameraDevices()
        {
            return await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
        }

    }
}
