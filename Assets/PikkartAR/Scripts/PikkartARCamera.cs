using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace PikkartAR {


	/// <summary>
	/// Pikkart AR camera.
	/// Handles camera update events and camera settings.
	/// </summary>
	public class PikkartARCamera : MonoBehaviour {

        private MarkerObject[] _markerObjects;
        private CloudMarkerObject[] _cloudMarkers;

        public void SetMarkers(ref MarkerObject[] markerObjects, ref CloudMarkerObject[] cloudMarkers)
        {
            _markerObjects = markerObjects;
            _cloudMarkers = cloudMarkers;
        }

        // fps
        float deltaTime = 0.0f;
        
        /* Webcam manager
        */
        public WebCamManager _webcam = null;

        /* Camera view matrix
        */
        private Matrix4x4 viewMatrix;
        
        /* Unity camera
        */
        private Camera cam =  null;
        private GameObject camera_game_object = null;

        private bool IsSeethroughDevice = false;
        private int PredictionFrames = 1;
        public void SetSeeThroughDevice(bool is_seethrough, int prediction_frames)
        {
            if(prediction_frames==0)
            {
                IsSeethroughDevice = false;
                PredictionFrames = 0;
            }
            else
            {
                IsSeethroughDevice = is_seethrough;
                PredictionFrames = prediction_frames;
            }
        }

        /* recognition manager
        */
        private RecognitionManager recognitionManager;

		/* enable show fps 
        */
		public bool showFps;

        private String previousMarkerFound = "";

        private object m_Handle = new object();
        int m_elapsedTime = 0;
        public int elapsedTime
        {
            get
            {
                int tmp;
                lock (m_Handle)
                {
                    tmp = m_elapsedTime;
                }
                return tmp;
            }
            set
            {
                lock (m_Handle)
                {
                    m_elapsedTime = value;
                }
            }
        }

        private bool camera_coroutine_running = true;
        private bool camera_loop_running = false;

        public Matrix4x4 GetProjectionMatrix()
        {
            if (cam != null)
                return cam.projectionMatrix;
            else return Matrix4x4.identity;
        }

        public Matrix4x4 GetModelViewMatrix()
        {
            if (cam != null)
                return cam.worldToCameraMatrix;
            else return transform.worldToLocalMatrix;
        }

        public Vector3 GetDownVector()
        {
            if(cam!=null)
                return new Vector3(-cam.worldToCameraMatrix.m10, -cam.worldToCameraMatrix.m11, -cam.worldToCameraMatrix.m12);
            else
                return new Vector3(-transform.worldToLocalMatrix.m10, -transform.worldToLocalMatrix.m11, -transform.worldToLocalMatrix.m12);
        }

        public bool IsTracking()
        {
            return previousMarkerFound.Length == 0;
        }

        void Awake()
		{
			Application.targetFrameRate = Constants.APPLICATION_TARGET_FRAMERATE;
            //DeviceOrientationChange.OnScreenDataChange += OnScreenDataChange;
        }

        void OnEnable()
        {
            OnApplicationPause(false);
        }
        void OnDisable()
        {
            OnApplicationPause(true);
        }

        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                camera_coroutine_running = false;
                if (_webcam != null)
                {
                    _webcam.StopWebcam();
                    while(!_webcam.Stopped())
                    {
                        Debug.Log("Waiting camera to stop");
                    }
                    StopAllCoroutines();
                }
            }
            else
            {
                if (_webcam != null)
                {
                    camera_coroutine_running = false;
                    StopAllCoroutines();
                    _webcam.StartWebcam(false);
                    camera_coroutine_running = true;
                    StartCoroutine(CameraLoop());
                    //StartCoroutine(MarkerUpdateLoop());
                }
            }
        }

        /*! \brief Initialization code
         *
         *  Initialization code
         */
        void Start()
        {
            Debug.Log("[START] PikkartARCamera");
            //setup camera
            cam = GetComponent<Camera>();
            if (cam != null) {
                cam.fieldOfView = Constants.UNITY_SMALL_FOV;
                cam.nearClipPlane = Constants.NEAR_CLIP_PLANE;
                cam.farClipPlane = Constants.FAR_CLIP_PLANE;
            }

            camera_game_object = GameObject.Find("CameraGameObject");
            if (camera_game_object != null)
            {
                if(cam!=null)
                    camera_game_object.transform.SetParent(cam.transform);
                else
                    camera_game_object.transform.SetParent(transform);
            }

            viewMatrix = Matrix4x4.identity;
        }

        public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
        {
            // Trap the case where the matrix passed in has an invalid rotation submatrix.
            if (m.GetColumn(2) == Vector4.zero)
            {
                return Quaternion.identity;
            }
            return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
        }

        public static Vector3 PositionFromMatrix(Matrix4x4 m)
        {
            return new Vector3(m.m03, m.m13, m.m23);
        }

        public static Matrix4x4 LHMatrixFromRHMatrix(Matrix4x4 rhm)
        {
            Matrix4x4 lhm_t = rhm.inverse;

            Matrix4x4 lhm = new Matrix4x4(); ;

            // Column 0.
            lhm[0, 0] = lhm_t[0, 0];
            lhm[1, 0] = lhm_t[1, 0];
            lhm[2, 0] = -lhm_t[2, 0];
            lhm[3, 0] = lhm_t[3, 0];

            // Column 1.
            lhm[0, 1] = lhm_t[0, 1];
            lhm[1, 1] = lhm_t[1, 1];
            lhm[2, 1] = lhm_t[2, 1];
            lhm[3, 1] = lhm_t[3, 1];

            // Column 2.
            lhm[0, 2] = -lhm_t[0, 2];
            lhm[1, 2] = -lhm_t[1, 2];
            lhm[2, 2] = -lhm_t[2, 2];
            lhm[3, 2] = -lhm_t[3, 2];

            // Column 3.
            lhm[0, 3] = lhm_t[0, 3];
            lhm[1, 3] = lhm_t[1, 3];
            lhm[2, 3] = lhm_t[2, 3];
            lhm[3, 3] = lhm_t[3, 3];

            return lhm;
        }

        private IntPtr markerFoundPtr;
        private HandledArray<int> trackedMarkersData;
        private HandledArray<float> viewMatrixData;
        public void LaunchWebcam(WebCamManager webcam)
        {
            markerFoundPtr = Marshal.AllocHGlobal(512);
            viewMatrixData = new HandledArray<float>(Constants.MATRIX_4X4_ARRAY_SIZE);
            trackedMarkersData = new HandledArray<int>(30);
            _webcam = webcam;
            _webcam.StartWebcam(false);
            StartCoroutine(CameraLoop());
        }

        //private IEnumerator MarkerUpdateLoop()
        private void MarkerUpdate()
        {
            //Debug.Log("camera_coroutine_running?=" + camera_coroutine_running);
            //while (camera_coroutine_running)
            //{
                if (viewMatrix != null && !viewMatrix.isIdentity && _markerObjects!=null && _cloudMarkers!=null)
                {
                    Matrix4x4 transformed_viewMatrix = Matrix4x4.identity;

                    if (cam != null && cam.worldToCameraMatrix != null)
                    {
                        transformed_viewMatrix = cam.worldToCameraMatrix.inverse * viewMatrix;
                    }
                    else
                    {
                        Matrix4x4 correction = Matrix4x4.identity;
                        correction.m22 = -1;
                        transformed_viewMatrix = transform.worldToLocalMatrix.inverse * correction * viewMatrix;
                    }

                    foreach (MarkerObject localMarker in _markerObjects)
                    {
                        if (localMarker != null && localMarker.enabled)
                        {
                            //MatrixUtils.SetTransformFromMatrixAverage(localMarker.transform, ref transformed_viewMatrix);
                            MatrixUtils.SetTransformFromMatrix(localMarker.transform, ref transformed_viewMatrix);
                        }
                    }
                    foreach (CloudMarkerObject cloudMarker in _cloudMarkers)
                    {
                        if (cloudMarker != null && cloudMarker.enabled)
                        {
                            //MatrixUtils.SetTransformFromMatrixAverage(cloudMarker.transform, ref transformed_viewMatrix);
                            MatrixUtils.SetTransformFromMatrix(cloudMarker.transform, ref transformed_viewMatrix);
                        }
                    }

                    if (camera_game_object != null)
                    {
                        Matrix4x4 temp = Matrix4x4.identity;
                        if (cam != null && cam.worldToCameraMatrix != null)
                            temp = LHMatrixFromRHMatrix(cam.worldToCameraMatrix);
                        else
                            temp = LHMatrixFromRHMatrix(transform.worldToLocalMatrix);

                        camera_game_object.transform.position = PositionFromMatrix(temp);
                        camera_game_object.transform.rotation = QuaternionFromMatrix(temp);
                    }
                }
             //   yield return new WaitForEndOfFrame();
            //}
        }

        private IEnumerator CameraLoop()
        {
            camera_coroutine_running = true;
            while (camera_coroutine_running)
            {
                camera_loop_running = true;
                if (_webcam!=null && _webcam.IsUpdated())
                {
                    //Debug.Log("webcam Pikkart Camera Loop camera updated");
                    PikkartARCore.SetCameraOriginalSize(Constants.CAMERA_REQUESTED_WIDTH, Constants.CAMERA_REQUESTED_HEIGHT);

                    PikkartARCore.CopyNewFrame(_webcam.GetDataAsIntPtr(), _webcam.GetCameraWidth(), _webcam.GetCameraHeight());
                    //PikkartARCore.CopyNewFrame32(_webcam.GetDataAsColor32(), Constants.CAMERA_REQUESTED_WIDTH, Constants.CAMERA_REQUESTED_HEIGHT);
                    //_webcam.UploadCameraDataAsColor32();

                    bool found_something = PikkartARCore.ImageProcessing();

                    if (Screen.currentResolution.width > Screen.currentResolution.height)
                    {
                        if(cam!=null)
                            cam.projectionMatrix = MatrixUtils.GetAlternateLandscapeProjectionMatrix();
                    }
                    else
                    {
                        if(cam!=null)
                            cam.projectionMatrix = MatrixUtils.GetAlternatePortraitProjectionMatrix();
                    }

                    String markerFound = "";
                    int logo_payload = -1;

                    int numTrackedMarkers = 0;
                    if(found_something) numTrackedMarkers = PikkartARCore.GetTrackedInternalIDs(trackedMarkersData.GetArray());
                    //Debug.Log("markerFound=numTrackedMarkers " + numTrackedMarkers);
                    if (numTrackedMarkers > 0)
                    {
                        int size = PikkartARCore.GetMarkerNameFromInternalIDs(trackedMarkersData.GetArray()[0], markerFoundPtr);
                        markerFound = Marshal.PtrToStringAnsi(markerFoundPtr, size-1);
                        if (IsSeethroughDevice)
                        {
                            size = PikkartARCore.GetMarkerPoseInternalID_WithPrediction(trackedMarkersData.GetArray()[0], PredictionFrames, viewMatrixData.GetArray());
                        }
                        else
                        {
                            size = PikkartARCore.GetMarkerPoseInternalID(trackedMarkersData.GetArray()[0], viewMatrixData.GetArray());
                        }
                        logo_payload = PikkartARCore.MarkerHasLogoInternalId(trackedMarkersData.GetArray()[0]);
                        if (logo_payload > 0)
                        {
                            recognitionManager.ARLogoFound(trackedMarkersData.GetArray()[0], logo_payload);
                        }
                        else logo_payload = -1;
                    }
                    else
                    {
                        markerFound = "";
                    }

                    if (markerFound == "")
                    {
                        if (recognitionManager != null)
                        {
                            if (recognitionManager.IsRecognitionRunning())
                            {
                                recognitionManager.LocalMarkerNotFound();
                            }
                        }
                        viewMatrix = Matrix4x4.identity;
                    }
                    else if (markerFound != "")
                    {
						if (IsSeethroughDevice) {
							if (previousMarkerFound != "" && markerFound != previousMarkerFound)
								recognitionManager.MarkerTrackingLost (previousMarkerFound);
						}
                        if (recognitionManager != null && previousMarkerFound == "")
                        {
                            recognitionManager.LocalMarkerFound(markerFound, trackedMarkersData.GetArray()[0]);
                        }
                        viewMatrix = MatrixUtils.GetViewMatrixFromArray(viewMatrixData.GetArray());

                        viewMatrix = MatrixUtils.GetRotationMatrixForOrientation() * viewMatrix;   
                    }

					if (!IsSeethroughDevice) {
						if (previousMarkerFound != "" && markerFound == "")
							recognitionManager.MarkerTrackingLost (previousMarkerFound);
					}
					previousMarkerFound = markerFound;

                    MarkerUpdate();

#if UNITY_EDITOR
                    _webcam.UpdateVisibleFrame();
#else
                    if (!IsSeethroughDevice) {
                        GL.IssuePluginEvent(PikkartARCore.GetRenderEventFunc(), 666);
                    }
#endif

                    //Debug.Log("markerFound=" + markerFound);

                }
                //yield return new WaitUntil(() => _webcam!=null && _webcam.IsUpdated() == true);
                //yield return null;
                yield return new WaitForEndOfFrame();
            }
            camera_loop_running = false;
        }

        void Update()
        {
            // per il calcolo degli fps
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        }

		/// <summary>
		/// Sets the RecognitionManager reference.
		/// </summary>
		/// <param name="recognitionManager">Recognition manager ref</param>
        public void SetRecognitionManager(RecognitionManager recognitionManager)
        {
            this.recognitionManager = recognitionManager;
            this.recognitionManager.SetPikkartCamera(this);
        }

        public void SetCameraParameters(double fx, double fy, double cx, double cy)
        {
            /*cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.fieldOfView = Constants.UNITY_SMALL_FOV;
                cam.nearClipPlane = Constants.NEAR_CLIP_PLANE;
                cam.farClipPlane = Constants.FAR_CLIP_PLANE;
            }*/
            Debug.Log("Pikkart Camera Params: fx=" + fx + " fy=" + fy + " cx=" + cx + " cy=" + cy);
        }

		/// <summary>
		/// Raises the screen data change event.
		/// </summary>
		/// <param name="screen">Screen.</param>
        /*public void OnScreenDataChange(ScreenData screen)

        {
            if (screen != null)
            {
                if (screen.IsResolutionLandscape())
                {
                    cam.projectionMatrix = MatrixUtils.GetAlternateLandscapeProjectionMatrix();
                }
                else {
                    cam.projectionMatrix = MatrixUtils.GetAlternatePortraitProjectionMatrix();
                }
            }
        }*/
        
        void OnDestroy()
        {
            camera_coroutine_running = false;
            StopAllCoroutines();
            //_webcam.StopWebcam();
            //viewMatrixData.Free();
            //trackedMarkersData.Free();
        }

#region SHOW_FPS
        void OnGUI()
        {
			if (!showFps)
				return;
			
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(1.0f, 0.0f, 0.5f, 1.0f);
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);

            Rect rect2 = new Rect(0, 40, w, h * 2 / 100);
            float fps2 = 1000.0f / elapsedTime;
            string text2 = string.Format("Processing: {0:0.0} ms ({1:0.} fps)", elapsedTime, fps2);
            GUI.Label(rect2, text2, style);

            Rect rect3 = new Rect(0, 60, w, h * 2 / 100);
            string text3 = "Marker found: " + previousMarkerFound;
            GUI.Label(rect3, text3, style);


        }
#endregion

	}
}