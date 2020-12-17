  using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

/*
 *  WebCam Manager per gestirla su Windows o Mac 
 *  A parte Init e Start gli unici metodi di interesse sono
 *  GetManagedWebCamTexture: ritorna il puntatore alla texture
 *  UpdateVisibleFrame: aggiorna l'interfaccia
 */

namespace PikkartAR {
    public class WebCamManager : MonoBehaviour {

        public CanvasScaler canvasScaler;

        private static WebCamTexture webcamTexture = null;
        private static Color32[] data;
        private Vector3 portraitScale;
        private Texture2D textureToShow = null;
        private RawImage rawImage = null;
        private IntPtr imagePtr_send;
        private IntPtr imagePtr_receive;

        private bool initialized_manager = false;

        public void InitWebcam(bool front_facing) {
            if (rawImage == null)
            {
                rawImage = GetComponent<RawImage>();
                if(rawImage!=null)
                    rawImage.texture = Texture2D.blackTexture;
            }
            if (textureToShow == null)
            {
                textureToShow = new Texture2D(Constants.CAMERA_REQUESTED_WIDTH,
                                              Constants.CAMERA_REQUESTED_HEIGHT,
                                              TextureFormat.RGBA32,
                                              false);
                textureToShow.Apply();
            }
#if !UNITY_EDITOR
            PikkartARCore.CameraUpdateUnityTextureId_GL(textureToShow.GetNativeTexturePtr());
#endif

#if UNITY_EDITOR
            PikkartARCore.UpdateViewport((int)Constants.CAMERA_REQUESTED_WIDTH, (int)Constants.CAMERA_REQUESTED_HEIGHT, 0);
#endif

            DeviceOrientationChange.OnScreenDataChange += OnScreenDataChange;
            portraitScale = new Vector3(
                                         (float)Constants.CAMERA_REQUESTED_HEIGHT / (float)Constants.CAMERA_REQUESTED_WIDTH,
                                         (float)Constants.CAMERA_REQUESTED_HEIGHT / (float)Constants.CAMERA_REQUESTED_WIDTH,
                                         1.0f);

            data = null;

            imagePtr_send = IntPtr.Zero;
            imagePtr_receive = Marshal.AllocHGlobal(Constants.CAMERA_REQUESTED_WIDTH * Constants.CAMERA_REQUESTED_HEIGHT * 4);

            webcamTexture = GetManagedWebCamTexture(front_facing);

            if (rawImage != null) rawImage.texture = textureToShow;
            initialized_manager = true;
            
            OnScreenDataChange(DeviceOrientationChange.GetUpdatedDeviceOrientation());
        }

        void OnScreenDataChange(ScreenData screen) {

            int angle = 0;
            switch (DeviceOrientationChange.GetDeviceOrientation())
            {
                case DeviceOrientation.LandscapeLeft:
                    RotateLandscapeLeft();
                    angle = 0;
                    break;
                case DeviceOrientation.LandscapeRight:
                    RotateLandscapeRight();
                    angle = 180;
                    break;
                case DeviceOrientation.Portrait:
                    RotatePortrait();
                    angle = 90;
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    RotatePortaitUpsideDown();
                    angle = 270;
                    break;
            }
            PikkartARCore.UpdateViewport((int)screen.resolution.x, (int)screen.resolution.y, angle);
        }

        private void RotateLandscapeLeft() {
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            if(canvasScaler!=null) canvasScaler.matchWidthOrHeight = 0f;
        }

        private void RotateLandscapeRight() {
            transform.localRotation = Quaternion.identity;
            transform.Rotate(0f, 0f, 180f);
            transform.localScale = Vector3.one;
            canvasScaler.matchWidthOrHeight = 0f;
        }

        private void RotatePortrait() {
            transform.localRotation = Quaternion.identity;
            transform.Rotate(0f, 0f, -90f);
            transform.localScale = portraitScale;
            canvasScaler.matchWidthOrHeight = 1f;
        }

        private void RotatePortaitUpsideDown() {
            transform.localRotation = Quaternion.identity;
            transform.Rotate(0f, 0f, 90f);
            transform.localScale = portraitScale;
            canvasScaler.matchWidthOrHeight = 1f;
        }

        public int GetCameraWidth()
        {
            if (webcamTexture != null)
                return webcamTexture.width;
            return 0;
        }

        public int GetCameraHeight()
        {
            if (webcamTexture != null)
                return webcamTexture.height;
            return 0;
        }

        public IntPtr GetDataAsIntPtr()
        {
            if (webcamTexture != null && webcamTexture.isPlaying) 
            {
                if(data==null || data.Length!= webcamTexture.width* webcamTexture.height)
                {
                    data = new Color32[webcamTexture.width * webcamTexture.height];
                }
                data = webcamTexture.GetPixels32(data);
                if (data.Length >= webcamTexture.width * webcamTexture.height)
                {
                    if (imagePtr_send == IntPtr.Zero)
                    {
                        imagePtr_send = Marshal.AllocHGlobal(webcamTexture.width * webcamTexture.height * 4);
                    }
                    byte[] byteData = ColorUtils.Color32ArrayToByteArray(data);
                    Marshal.Copy(byteData, 0, imagePtr_send, webcamTexture.width * webcamTexture.height * 4);
                }
            }
            return imagePtr_send;
        }

        public Color32[] GetDataAsColor32()
        {
            if (webcamTexture != null && webcamTexture.isPlaying)
            {
                webcamTexture.GetPixels32(data);
            }
            return data;
        }

        public void UploadCameraDataAsColor32()
        {
            if (webcamTexture != null && webcamTexture.isPlaying)
            {
                webcamTexture.GetPixels32(data);
            }
            PikkartARCore.CopyNewFrame32(data, Constants.CAMERA_REQUESTED_WIDTH, Constants.CAMERA_REQUESTED_HEIGHT);
        }

        public void UploadCameraDataAsOpenGLTexture()
        {
            if (webcamTexture != null && webcamTexture.isPlaying)
            {
                IntPtr gl_id = webcamTexture.GetNativeTexturePtr();
                PikkartARCore.CopyNewFrameFromGL(gl_id, Constants.CAMERA_REQUESTED_WIDTH, Constants.CAMERA_REQUESTED_HEIGHT);
            }
        }

        public bool IsUpdated() {
            if(webcamTexture!=null)
                return webcamTexture.didUpdateThisFrame;
            return false;
        }

        public bool HasWebcam()
        {
            return WebCamTexture.devices.Length > 0;
        }

		public static WebCamTexture GetManagedWebCamTexture (bool front_facing) {
			if (webcamTexture == null) {
				webcamTexture = new WebCamTexture();
				WebCamDevice[] devices = WebCamTexture.devices;
				string deviceToUse = null;
				
				for( var i = 0 ; i < devices.Length ; i++ )
                {
                    Debug.Log("webcam " + i + " name " + devices[i].name + " kind " + devices[i].kind);
                    if (devices[i].isFrontFacing == front_facing && devices[i].kind != WebCamKind.Telephoto &&
                        devices[i].kind!=WebCamKind.WideAngle)
                    {
                        deviceToUse = devices[i].name;
                        break;
                    }
				}

                //Debug.Log("webcam selected = " + deviceToUse);
				
				if (deviceToUse == null && devices.Length > 0) {
					deviceToUse = devices[0].name;
				}

                Debug.Log("webcam final selected = " + deviceToUse);

                if (deviceToUse != null) {
					webcamTexture.deviceName = deviceToUse;
				}

                webcamTexture.requestedWidth = Constants.CAMERA_REQUESTED_WIDTH;
				webcamTexture.requestedHeight = Constants.CAMERA_REQUESTED_HEIGHT;
                webcamTexture.requestedFPS = 60.0f;
            }

            return webcamTexture;
		}
		
		public void StartWebcam (bool front_facing) {
            if (!initialized_manager)
            {
                InitWebcam(front_facing);
            }
            else if (webcamTexture == null)
            {
                webcamTexture = GetManagedWebCamTexture(front_facing);
            }

            if (webcamTexture != null)
            {
                webcamTexture.requestedWidth = Constants.CAMERA_REQUESTED_WIDTH;
                webcamTexture.requestedHeight = Constants.CAMERA_REQUESTED_HEIGHT;
                webcamTexture.requestedFPS = 60.0f;
                webcamTexture.Play();
            }
		}

        public void StopWebcam()
        {
            if (webcamTexture != null && webcamTexture.isPlaying)
            {
                webcamTexture.Pause();
                while (webcamTexture.isPlaying) { }
                webcamTexture.Stop();
            }

            /*if (webcamTexture != null)
            {
                Destroy(webcamTexture);
                webcamTexture = null;
            }*/
        }

        public bool Stopped()
        {
            return !webcamTexture.isPlaying;
        }

        public void UpdateVisibleFrame()
        {
#if UNITY_EDITOR
            PikkartARCore.UpdateCameraTexture(imagePtr_receive);
#endif
            if (textureToShow != null)
            {
                textureToShow.LoadRawTextureData(imagePtr_receive, Constants.CAMERA_REQUESTED_WIDTH * Constants.CAMERA_REQUESTED_HEIGHT * 4);
                textureToShow.Apply();
                if (rawImage != null) rawImage.texture = textureToShow;
            }
        }

		public Texture2D getTextureToShow()
		{
			return textureToShow;
		}
	}
}