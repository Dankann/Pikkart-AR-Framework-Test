using System.Collections;
using System.IO;
using UnityEngine;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PikkartAR
{
	/// <summary>
	/// Main script. Inizialize and start all components.
	/// Implements IRecognitionListener to communicate with a RecognitionManager object.
	/// </summary>
	public class PikkartMain : MonoBehaviour, IMarkerObjectListener, ICloudMarkerObjectListener
    {
		/// <summary>
		/// The marker list imported from the inspector
		/// </summary>
		public string[] _markerList = new string[1];
        public string[] _discoverList = new string[1];
	    /// <summary>
	    /// Dicitionary of markers in the scene
	    /// </summary>
	    protected MarkerObject[] _markerObjects;

        protected CloudMarkerObject[] _cloudMarkers;

	    public string[] GetMarkerList() {
	        return _markerList;
	    }

        public string[] GetDiscoverList()
        {
            return _discoverList;
        }

	    protected PikkartARCamera _camera;

        WebCamManager _webcam;

        private RecognitionManager _recognitionManager = null;
        private IRecognitionListener _recognitionListener;
		protected RecognitionOptions _recognitionOptions;
		[SerializeField]
		public RecognitionOptions.RecognitionStorage recognitionStorage;
		[SerializeField]
		public RecognitionOptions.RecognitionMode recognitionMode;
		[SerializeField]
		protected string[] databases = new string[]{};
        [SerializeField]
        public bool useWebcamOnEditor;
        [SerializeField]
        public bool isSeeThroughDevice = false;
        [SerializeField]
        public int seeThroughFramePrediction = 0;

        protected ICloudMarkerObjectListener _cloudMarkerObjectListener = null;

		/// <summary>
		/// Awake this instance.
		/// </summary>
		protected void Awake() {
#if UNITY_EDITOR && !(UNITY_ANDROID || UNITY_IOS) 
            if (EditorUtility.DisplayDialog("Platform error!",
            "Our SDK currently supports only Android and iOS platforms!\n" +
            "Please switch to a correct platform from File->Build settings.", "Ok"))
            {
                EditorApplication.isPlaying = false;
            }
#endif

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            useWebcamOnEditor = true;
#endif
	    }

        protected void Start()
        {
            _cloudMarkers = FindObjectsOfType<CloudMarkerObject>();
            _markerObjects = FindObjectsOfType<MarkerObject>();
            if (useWebcamOnEditor) {
                StartCoroutine(InitRecognition());
            }
            else
            {
                if (FindObjectOfType<ScanButton>() != null)
                    FindObjectOfType<ScanButton>().gameObject.SetActive(false);
                ActivateObjectsInEditor();
            }                
        }

        protected void ActivateObjectsInEditor()
        {
            foreach (MarkerObject markerObject in _markerObjects)
            {
                markerObject.OnMarkerLost();
                markerObject.OnMarkerFound();
            }

            foreach(CloudMarkerObject cloudMarker in _cloudMarkers)
            {
                cloudMarker.OnCloudMarkerLost();
                cloudMarker.OnCloudMarkerFound();
            }
        }

        public void OnPikkartARStartupComplete()
        {

        }

        public void SetRecognitionListener(IRecognitionListener recognitionListener)
        {
            _recognitionListener = recognitionListener;
        }

        /// <summary>
        /// Init this instance. Initialize and start RecongnitionManager, PikkartCamera and NatCamManager.
        /// </summary>
        protected IEnumerator InitRecognition()
	    {
            foreach (var device in WebCamTexture.devices)
            {
                Debug.Log("Name: " + device.name);
            }
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.Log("webcam found");
            }
            else
            {
#if UNITY_EDITOR
                if (EditorUtility.DisplayDialog("Camera error!",
                    "You cannot use our SDK without a camera!\n" +
                    "Please connect a camera and try again.", "Ok"))
                {
                    EditorApplication.isPlaying = false;
                }
#endif
                Debug.Log("webcam error no webcam");
                yield break;
            }

#if UNITY_EDITOR
            if (!useWebcamOnEditor)
            {
                yield break;
            }
#endif
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            foreach(CloudMarkerObject cloudMarker in _cloudMarkers) {
                cloudMarker.Init();
            }
	        foreach (MarkerObject marker in _markerObjects) {
	            marker.Init();
	        }

	        _camera = FindObjectOfType<PikkartARCamera>();

            _webcam = FindObjectOfType<WebCamManager>();

            if(_recognitionManager==null) {
				_recognitionManager = new RecognitionManager(this);
				_recognitionManager.LoadLocalMarkersOnStartup(true, _markerList, _discoverList);
			} else {
				PikkartARCore.SyncMarkersWithFolder();
			}

			_camera.SetRecognitionManager(_recognitionManager);

            _recognitionOptions = new RecognitionOptions(
                recognitionStorage,
                recognitionMode,
                new CloudRecognitionInfo(databases));
            
            if (_webcam.HasWebcam())
            {
                _camera.LaunchWebcam(_webcam);
            }
            else
            {
#if UNITY_EDITOR
                if (EditorUtility.DisplayDialog("Camera error!",
                    "You cannot use our SDK without a camera!\n" +
                    "Please connect a camera and try again.", "Ok"))
                {
                    EditorApplication.isPlaying = false;
                }
#endif
                Debug.Log("webcam error no webcam");
            }
#if UNITY_EDITOR
            if (_recognitionOptions.getMode() != RecognitionOptions.RecognitionMode.TAP_TO_SCAN)
            {
                if(FindObjectOfType<ScanButton>()!=null)
                    FindObjectOfType<ScanButton>().gameObject.SetActive(false);
            }
#else
            if(FindObjectOfType<ScanButton>()!=null)
                FindObjectOfType<ScanButton>().gameObject.SetActive(false);
#endif
            _camera.SetMarkers(ref _markerObjects, ref _cloudMarkers);

            _camera.SetSeeThroughDevice(isSeeThroughDevice, seeThroughFramePrediction);

            if (_recognitionListener!=null)
                _recognitionListener.OnPikkartARStartupComplete();
        }

        protected void StartRecognition(RecognitionOptions recognitionOptions, IRecognitionListener recognitionListener)
	    {
            SetRecognitionListener(recognitionListener);
            StartRecognition(recognitionOptions);
	    }

        protected void StartRecognition(RecognitionOptions recognitionOptions)
        {
#if UNITY_EDITOR
            if (!useWebcamOnEditor) return;
#endif
            _recognitionOptions = recognitionOptions;
            _recognitionManager.StartRecognition(recognitionOptions, _recognitionListener);
        }

        protected void StopRecognition()
	    {
	        PikkartARCore.StopRecognition();
	    }

	    protected void ChangeRecognitionOptions(RecognitionOptions recognitionOptions)
	    {
	        _recognitionManager.ChangeRecognitionOptions(recognitionOptions);
	    }

        protected void SetMarkerCacheSize(int markerCacheSize)
        {
            RecognitionManager.MarkerCacheSize = markerCacheSize;
        }

	    protected bool IsRecognitionActive()
	    {
	        return _recognitionManager.IsRecognitionRunning();
	    }

        protected void SyncLocalMarkers(bool isRecognitionManagerRunning, IRecognitionDataProviderListener recognitionDataProviderListener)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = jc.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject assetManager = context.Call<AndroidJavaObject>("getAssets");
#endif
            PikkartARCore.CheckUnityLicense(
#if UNITY_ANDROID && !UNITY_EDITOR
                    assetManager.GetRawObject()
#elif UNITY_EDITOR_WIN
                    new StringBuilder(Application.streamingAssetsPath),
    #if UNITY_5_6_OR_NEWER
                    new StringBuilder(PlayerSettings.applicationIdentifier)
    #else
                    new StringBuilder(PlayerSettings.bundleIdentifier)
    #endif
#endif
            );

            new RecognitionDataProvider(new CloudRecognitionInfo(databases), recognitionDataProviderListener,
                RecognitionManager.GetDeviceId()).SyncLocalMarkers(isRecognitionManagerRunning);
        }

        public void TapToScan()
        {
            if (!IsRecognitionActive() &&
                _recognitionOptions != null &&
                _recognitionOptions.getMode() == RecognitionOptions.RecognitionMode.TAP_TO_SCAN &&
                _recognitionListener != null)
            {
                StartRecognition(_recognitionOptions, _recognitionListener);
            }
        }

        protected bool isTracking()
	    {
	        return _camera.IsTracking();
	    }

	    protected MarkerInfo getCurrentMarker()
	    {
	        return _recognitionManager.GetCurrentMarker();
	    }

	    protected Matrix4x4 GetCurrentProjectionMatrix()
	    {
	        return _camera.GetProjectionMatrix();
	    }

	    protected Matrix4x4 GetCurrentModelViewMatrix()
	    {
	        return _camera.GetModelViewMatrix();
	    }

        public void SetCloudMarkerObjectListener(ICloudMarkerObjectListener cloudMarkerObjectListener)
        {
            _cloudMarkerObjectListener = cloudMarkerObjectListener;
        }

        private string previous_marker_id = "";
        private int previous_marker_pattern = -1;
        public void OnMarkerFound(MarkerInfo marker)
	    {
            Debug.Log("OnMarkerFound id: " + marker.getId());
            if (previous_marker_id != marker.getId() || previous_marker_pattern != marker.getARLogoCode())
            {
                foreach (MarkerObject m in _markerObjects)
                {
                    if (m.markerId == previous_marker_id && m.markerARLogoPattern == previous_marker_pattern)
                    {
                        m.OnMarkerLost();
                    }
                }
            }

            previous_marker_id = marker.getId();
            previous_marker_pattern = marker.getARLogoCode();

            foreach (MarkerObject m in _markerObjects)
            {
                if (m.markerId == marker.getId() && m.markerARLogoPattern == marker.getARLogoCode())
                {
                    m.OnMarkerFound();
                    return;
                }
            }

            previous_marker_pattern = -1;
            foreach (MarkerObject m in _markerObjects)
            {
                if (m.markerId == marker.getId() && m.markerARLogoPattern == -1)
                {
                    m.OnMarkerFound();
                    return;
                }
            }

            previous_marker_id = "";

            if (marker.getDatabase().isCloud() && _cloudMarkerObjectListener != null)
            {
                _cloudMarkerObjectListener.OnCloudMarkerFound(marker);
                return;
            }
        }

        public void OnARLogoFound(MarkerInfo marker, int payload)
        {
            Debug.Log("OnMarkerFound id: " + marker.getId() + " ARLogo:" + marker.getARLogoCode());
            if (previous_marker_id != marker.getId() || previous_marker_pattern != marker.getARLogoCode())
            {
                foreach (MarkerObject m in _markerObjects)
                {
                    if (m.markerId == previous_marker_id && m.markerARLogoPattern == previous_marker_pattern)
                    {
                        m.OnMarkerLost();
                    }
                }
            }

            previous_marker_id = marker.getId();
            previous_marker_pattern = marker.getARLogoCode();

            foreach (MarkerObject m in _markerObjects)
            {
                if (m.markerId == marker.getId() && m.markerARLogoPattern == marker.getARLogoCode())
                {
                    m.OnMarkerFound();
                    return;
                }
            }
            
            previous_marker_pattern = -1;
            foreach (MarkerObject m in _markerObjects)
            {
                if (m.markerId == marker.getId() && m.markerARLogoPattern == -1)
                {
                    m.OnMarkerFound();
                    return;
                }
            }

            previous_marker_id = "";

            if (marker.getDatabase().isCloud() && _cloudMarkerObjectListener != null)
            {
                _cloudMarkerObjectListener.OnCloudMarkerFound(marker);
                return;
            }
        }

        public void OnMarkerLost(MarkerInfo marker)
	    {
            previous_marker_id = "";
            previous_marker_pattern = -1;
            foreach (MarkerObject m in _markerObjects)
            {
                if (m.markerId == marker.getId() && m.markerARLogoPattern == marker.getARLogoCode())
                {
                    m.OnMarkerLost();
                    return;
                }
            }

            foreach (MarkerObject m in _markerObjects)
            {
                if (m.markerId == marker.getId() && m.markerARLogoPattern == -1)
                {
                    m.OnMarkerLost();
                    return;
                }
            }

            if (marker.getDatabase().isCloud() && _cloudMarkerObjectListener != null)
            {
                _cloudMarkerObjectListener.OnCloudMarkerLost(marker);
                return;
            }
        }

        // Implementazione interfaccia ICloudMarkerObjectListener
        public void OnCloudMarkerFound(MarkerInfo marker)
        {
            Debug.Log("OnCloudMarkerFound id: "+ marker.getId());
            if (_cloudMarkers.Length < 1)
                return;

            if (previous_marker_id != marker.getId() || previous_marker_pattern != marker.getARLogoCode())
            {
                foreach (CloudMarkerObject cloudMarker in _cloudMarkers)
                {
                    if (cloudMarker.cloudMarkerId == previous_marker_id && cloudMarker.cloudMarkerARLogoPattern == previous_marker_pattern)
                    {
                        cloudMarker.OnCloudMarkerLost();
                    }
                }
            }

            previous_marker_id = marker.getId();
            previous_marker_pattern = marker.getARLogoCode();

            foreach (CloudMarkerObject cloudMarker in _cloudMarkers)
            {
                if (cloudMarker.cloudMarkerId == marker.getId() && cloudMarker.cloudMarkerARLogoPattern == marker.getARLogoCode())
                {
                    cloudMarker.OnCloudMarkerFound();
                    return;
                }
            }

            previous_marker_pattern = -1;
            foreach (CloudMarkerObject cloudMarker in _cloudMarkers)
            {
                if (cloudMarker.cloudMarkerId == marker.getId() && cloudMarker.cloudMarkerARLogoPattern == -1)
                {
                    cloudMarker.OnCloudMarkerFound();
                    return;
                }
            }

            previous_marker_id = "";
        }

        public void OnCloudMarkerLost(MarkerInfo marker)
        {
            previous_marker_id = "";
            previous_marker_pattern = -1;

            if (_cloudMarkers.Length < 1)
                return;

            foreach (CloudMarkerObject cloudMarker in _cloudMarkers)
            {
                if (marker.getId() == cloudMarker.cloudMarkerId && marker.getARLogoCode() == cloudMarker.cloudMarkerARLogoPattern)
                {
                    cloudMarker.OnCloudMarkerLost();
                    return;
                }
            }

            foreach (CloudMarkerObject cloudMarker in _cloudMarkers)
            {
                if (cloudMarker.cloudMarkerId == marker.getId() && cloudMarker.cloudMarkerARLogoPattern == -1)
                {
                    cloudMarker.OnCloudMarkerLost();
                    return;
                }
            }
        }

        public void OnCloudARLogoFound(MarkerInfo marker, int payload)
        {
            Debug.Log("OnCloudARLogoFound id: " + marker.getId() + " ARLogo:" + marker.getARLogoCode());

            if (_cloudMarkers.Length < 1)
                return;
            
            if (previous_marker_id != marker.getId() || previous_marker_pattern != marker.getARLogoCode())
            {
                foreach (CloudMarkerObject cloudMarker in _cloudMarkers)
                {
                    if (cloudMarker.cloudMarkerId == previous_marker_id && cloudMarker.cloudMarkerARLogoPattern == previous_marker_pattern)
                    {
                        cloudMarker.OnCloudMarkerLost();
                    }
                }
            }

            previous_marker_id = marker.getId();
            previous_marker_pattern = marker.getARLogoCode();

            foreach (CloudMarkerObject cloudMarker in _cloudMarkers)
            {
                if (cloudMarker.cloudMarkerId == marker.getId() && cloudMarker.cloudMarkerARLogoPattern == marker.getARLogoCode())
                {
                    cloudMarker.OnCloudMarkerFound();
                    return;
                }
            }

            previous_marker_pattern = -1;
            foreach (CloudMarkerObject cloudMarker in _cloudMarkers)
            {
                if (cloudMarker.cloudMarkerId == marker.getId() && cloudMarker.cloudMarkerARLogoPattern == -1)
                {
                    cloudMarker.OnCloudMarkerFound();
                    return;
                }
            }

            previous_marker_id = "";
        }

        /// <summary>
        /// Marker import function.
        /// Look for local marker and add them to the list of markers. Called from Unity3D editor inspector.
        /// </summary>
        public void Check() {

            if (!Directory.Exists(Application.streamingAssetsPath) || !Directory.Exists(Application.streamingAssetsPath + "/markers"))
            {
                _markerList = new string[1];
                _markerList[0] = "";
                return;
            }                

            //calcolo il numero di marker sulla base dei file .dat presenti nella cartella
            int numberOfMarkers = 0;
	        System.IO.FileInfo[] dirInfo = new System.IO.DirectoryInfo(Application.streamingAssetsPath + "/markers").GetFiles();
	        foreach (System.IO.FileInfo filename in dirInfo)
	            if (filename.Name.Substring(filename.Name.Length - 4, 4).Equals(".dat"))
	                numberOfMarkers++;

	        //inizializzo la lista dei marker
	        _markerList = new string[numberOfMarkers+1];

	        //inserisco il placeholder
	        _markerList[0] = "";

	        //inserisco i marker
	        int i = 1;
			foreach (System.IO.FileInfo filename in dirInfo) {
				if (filename.Name.Substring(filename.Name.Length - 4, 4).Equals (".dat")) {
	                _markerList[i++] = filename.Name.Substring(0, filename.Name.Length - 4);
				}
			}

            //elimino eventuali file non più necessari
            System.IO.FileInfo[] xmldirInfo = new System.IO.DirectoryInfo(Application.streamingAssetsPath +
                "/xmls").GetFiles();
            foreach (System.IO.FileInfo filename in xmldirInfo)
                if (!filename.Name.Substring(filename.Name.Length - 5, 5).Equals(".meta"))
                    if (!filename.Name.Substring(filename.Name.Length - 5, 5).Equals("0.xml"))
                        if (!File.Exists(Application.streamingAssetsPath + "/markers/" +
                            filename.Name.Substring(0, filename.Name.Length - 4) + ".dat"))
                            File.Delete(filename.FullName);

            System.IO.FileInfo[] imagesdirInfo = new System.IO.DirectoryInfo(Application.dataPath +
                "/Editor/markerImages/").GetFiles();
            foreach (System.IO.FileInfo filename in imagesdirInfo)
                if (!filename.Name.Substring(filename.Name.Length - 5, 5).Equals(".meta"))
                    if (!File.Exists(Application.streamingAssetsPath + "/markers/" +
                        filename.Name.Substring(0, filename.Name.Length - 4) + ".dat"))
                        File.Delete(filename.FullName);

            if (!Directory.Exists(Application.streamingAssetsPath) || !Directory.Exists(Application.streamingAssetsPath + "/markers3D"))
            {
                _discoverList = new string[1];
                _discoverList[0] = "";
                return;
            }

            //calcolo il numero di marker sulla base dei file .dat presenti nella cartella
            int numberOfDiscoverModels = 0;
            dirInfo = new System.IO.DirectoryInfo(Application.streamingAssetsPath + "/markers3D").GetFiles();
            foreach (System.IO.FileInfo filename in dirInfo)
                if (filename.Name.Substring(filename.Name.Length - 4, 4).Equals(".dat"))
                    numberOfDiscoverModels++;

            //inizializzo la lista dei marker
            _discoverList = new string[numberOfDiscoverModels + 1];

            //inserisco il placeholder
            _discoverList[0] = "";

            //inserisco i marker
            i = 1;
            foreach (System.IO.FileInfo filename in dirInfo)
            {
                if (filename.Name.Substring(filename.Name.Length - 4, 4).Equals(".dat"))
                {
                    _discoverList[i++] = filename.Name.Substring(0, filename.Name.Length - 4);
                }
            }
        }
	}
}