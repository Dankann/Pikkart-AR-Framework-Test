using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Timers;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PikkartAR
{
    /// <summary>
    /// Recognition manager. Central management unit for recognition service.
    /// Inizialize start and stop recognition. Implements IDataProviderListener to communicate with the DataProvider.
    /// </summary>
    public class RecognitionManager : IRecognitionDataProviderListener
    {
        public static string GetAppDataPath() {
			string r = Application.persistentDataPath + "/";
            return r;
        }

        public static string GetDeviceId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }
        
        private const int _localMarkerNotFoundEventMinIntervalMillisecs = 1000;
        private const int MAIN_INTERVAL = 10;
        private const int SERVER_RESPONSE_INTERVAL = 15;

        private string _markerId = "";
        private IRecognitionListener _recognitionListener;
        private RecognitionDataProvider _dataProvider;
        private static bool _nativeWrapperInitialized = false;
        private static bool _licenceChecked = false;
        private RecognitionOptions _currentRecognitionOptions;
        //private bool _trackingOn;
        private string _deviceId;
        private bool _recognitionRunning;
        MarkerInfo _currentMarker;
        //IntPtr _nativeArray;
        private bool _active = false;

        public static int MarkerCacheSize = 25;

        private Timer _mainTimer = new Timer(MAIN_INTERVAL * 1000);
        private Timer _serverResponseTimer = new Timer(SERVER_RESPONSE_INTERVAL * 1000);
        private int _cloudMarkerNotFoundCount = 0;

        public static bool SavedLocalModels = false;

        private IMarkerObjectListener _markerObjectListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="PikkartAR.RecognitionManager"/> class.
        /// </summary>
        /// <param name="width">Frame Width.</param>
        /// <param name="height">Frame Height.</param>
        /// <param name="localRecognitionService">Object that implements the communication interface.</param>
        public RecognitionManager(IMarkerObjectListener markerObjectListener)
        {
            _markerObjectListener = markerObjectListener;
            _recognitionRunning = false;

#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = jc.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject assetManager = context.Call<AndroidJavaObject>("getAssets");
#endif
            _deviceId = RecognitionManager.GetDeviceId();

            // Native Recognition Service Initialization
            if (!_nativeWrapperInitialized)
            {
                 int res = PikkartARCore.InitRecognition(
#if UNITY_ANDROID && !UNITY_EDITOR
                    assetManager.GetRawObject(),
#endif
                    _localMarkerNotFoundEventMinIntervalMillisecs,
#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR_WIN)
                    GetAppDataPath(),
#else
                    new StringBuilder(GetAppDataPath()),
#endif
#if UNITY_EDITOR_WIN
                    new StringBuilder(Application.streamingAssetsPath),
    #if UNITY_5_6_OR_NEWER
                    new StringBuilder(PlayerSettings.applicationIdentifier),
    #else
                    new StringBuilder(PlayerSettings.bundleIdentifier),
    #endif

#elif UNITY_EDITOR_OSX
					Application.streamingAssetsPath,
    #if UNITY_5_6_OR_NEWER
					PlayerSettings.applicationIdentifier,
    #else
                    PlayerSettings.bundleIdentifier,
    #endif
#endif
                    Constants.PROCESS_WIDTH,
                    Constants.PROCESS_HEIGHT,
                    ScreenUtilities.GetScreenInches(),
                    Constants.CAMERA_REQUESTED_WIDTH,
                    Constants.CAMERA_REQUESTED_HEIGHT,
                    false);

                if (res > 0) _licenceChecked = true;
                else _licenceChecked = false;
                if (res==0)
                {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                    if(MonoBehaviour.FindObjectOfType<LicenseMessage>()!=null)
                        MonoBehaviour.FindObjectOfType<LicenseMessage>().gameObject.SetActive(true);
                    if(MonoBehaviour.FindObjectOfType<TrialMessage>()!=null)
                        MonoBehaviour.FindObjectOfType<TrialMessage>().gameObject.SetActive(false);
#elif UNITY_EDITOR
                    if (MonoBehaviour.FindObjectOfType<TrialMessage>() != null)
                        MonoBehaviour.FindObjectOfType<TrialMessage>().gameObject.SetActive(false);
                    if (MonoBehaviour.FindObjectOfType<LicenseMessage>() != null)
                        MonoBehaviour.FindObjectOfType<LicenseMessage>().gameObject.SetActive(false);
                    if (EditorUtility.DisplayDialog("Licence error!",
                    "You cannot use our sdk without a valid licence UNLESS you are in a trial!\n" +
                    "Please register to developer.pikkart.com to obtain your own licence or set your application identifier to 'com.pikkart.trial' to evaluate our SDK.", "Ok"))
                    {
                        EditorApplication.isPlaying = false;
                    }
#endif
                }
                else if(res==1)
                {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                    if(MonoBehaviour.FindObjectOfType<TrialMessage>()) MonoBehaviour.FindObjectOfType<TrialMessage>().gameObject.SetActive(true);
                    if(MonoBehaviour.FindObjectOfType<LicenseMessage>()) MonoBehaviour.FindObjectOfType<LicenseMessage>().gameObject.SetActive(false);
#elif UNITY_EDITOR
                    if (MonoBehaviour.FindObjectOfType<TrialMessage>()) MonoBehaviour.FindObjectOfType<TrialMessage>().gameObject.SetActive(false);
                    if(MonoBehaviour.FindObjectOfType<LicenseMessage>()) MonoBehaviour.FindObjectOfType<LicenseMessage>().gameObject.SetActive(false);
                    if (EditorUtility.DisplayDialog("No valid license",
                     "No valid license or running as com.pikkart.trial, press Ok to run in trial mode", "Ok"))
                     {
                         EditorApplication.isPlaying = true;
                     }
#endif
                }
                else
                {
                    if (MonoBehaviour.FindObjectOfType<TrialMessage>()) MonoBehaviour.FindObjectOfType<TrialMessage>().gameObject.SetActive(false);
                    if (MonoBehaviour.FindObjectOfType<LicenseMessage>()) MonoBehaviour.FindObjectOfType<LicenseMessage>().gameObject.SetActive(false);
                }

                PikkartARCore.SetProjectionMatrix(Constants.CAM_LARGE_FOV, Constants.CAM_SMALL_FOV);

                _nativeWrapperInitialized = true;
                //_nativeArray = Marshal.AllocHGlobal(Constants.BASE_RES_WIDTH * Constants.BASE_RES_HEIGHT * 3);
            }
            else
            {
                if (MonoBehaviour.FindObjectOfType<TrialMessage>()) MonoBehaviour.FindObjectOfType<TrialMessage>().gameObject.SetActive(false);
                if (MonoBehaviour.FindObjectOfType<LicenseMessage>()) MonoBehaviour.FindObjectOfType<LicenseMessage>().gameObject.SetActive(false);
            }


            _dataProvider = new RecognitionDataProvider(null, this, _deviceId);
        }

        public void scheduleTimers()
        {
            _mainTimer.Stop();
            _mainTimer.AutoReset = false;
            _mainTimer.Elapsed += (s, e) => {
                if (_currentRecognitionOptions.getStorage() == RecognitionOptions.RecognitionStorage.LOCAL ||
                _cloudMarkerNotFoundCount > 0)
                {
                    StopRecognition();
                    _recognitionListener.MarkerNotFound();
                }
            };
            _mainTimer.Start();

            _serverResponseTimer.Stop();
            _serverResponseTimer.AutoReset = false;
            _serverResponseTimer.Elapsed += (s, e) =>
            {
                StopRecognition();
                _recognitionListener.MarkerNotFound();
            };
            if (_currentRecognitionOptions.getStorage() != RecognitionOptions.RecognitionStorage.LOCAL)
                _serverResponseTimer.Start();
        }

        public void stopTimers()
        {
            _mainTimer.Stop();
            _serverResponseTimer.Stop();
            _cloudMarkerNotFoundCount = 0;
        }

        /// <summary>
        /// Starts the native service recognition.
        /// </summary>
        /// <param name="recognitionOptions">Recognition options.</param>
        /// <param name="recognitionListener">Recognition listener(MainScript).</param>
        public void StartRecognition(RecognitionOptions recognitionOptions, IRecognitionListener recognitionListener)
        {
            StopRecognition();

            SyncMarkersWithFolder();

            _recognitionRunning = true;
            _active = true;

            _currentRecognitionOptions = recognitionOptions;
            _recognitionListener = recognitionListener;
            _dataProvider = new RecognitionDataProvider(recognitionOptions.getCloudInfo(), this, GetDeviceId());

#if !UNITY_EDITOR
            if(!LoadSavedCameraParams())
                _dataProvider.ExecuteGetCameraParamFromServer(true);
#endif

            PikkartARCore.ChangeMode(_currentRecognitionOptions.getStorage() != RecognitionOptions.RecognitionStorage.LOCAL);
            PikkartARCore.StartRecognition(_currentRecognitionOptions.getStorage() != RecognitionOptions.RecognitionStorage.LOCAL, false);

            PikkartARCore.StartEffect();
            
            _markerId = "";

            if (_currentRecognitionOptions.getMode() == RecognitionOptions.RecognitionMode.TAP_TO_SCAN)
                scheduleTimers();

            
        }

        public void ForceMarkerSearch(string markerId)
        {
            ChangeRecognitionOptions(new RecognitionOptions(RecognitionOptions.RecognitionStorage.LOCAL, RecognitionOptions.RecognitionMode.CONTINUOUS_SCAN, null));
#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR_WIN)
            PikkartARCore.ForceMarkerSearch(markerId.ToString());
#else
            PikkartARCore.ForceMarkerSearch(new StringBuilder(markerId.ToString()));
#endif
        }

        public void ChangeRecognitionOptions(RecognitionOptions recognitionOptions)
        {
            if (_recognitionRunning)
            {
                if (_currentRecognitionOptions.getStorage() != recognitionOptions.getStorage())
                {
                    if (recognitionOptions.getStorage() != RecognitionOptions.RecognitionStorage.LOCAL)
                        PikkartARCore.StartEffect();
                    else
                        PikkartARCore.StopEffect();

                    PikkartARCore.ChangeMode(recognitionOptions.getStorage() != RecognitionOptions.RecognitionStorage.LOCAL);
                }

                if (_currentRecognitionOptions.getMode() != recognitionOptions.getMode())
                {
                    if (recognitionOptions.getMode() != RecognitionOptions.RecognitionMode.TAP_TO_SCAN)
                        stopTimers();
                    else
                        scheduleTimers();
                }
            }

            _currentRecognitionOptions = recognitionOptions;
        }

        /// <summary>
        /// Stops the recognition.
        /// </summary>
        public void StopRecognition()
        {
            stopTimers();
            PikkartARCore.StopEffect();
            PikkartARCore.StopRecognition();
            _recognitionRunning = false;
        }

        public bool IsRecognitionRunning()
        {
            return _recognitionRunning;
        }
        public bool IsActive()
        {
            return _active;
        }

        public MarkerInfo GetCurrentMarker()
        {
            return _currentMarker;
        }

		/// <summary>
		/// Locals the marker found. Called by PikkartARCamera instance.
		/// </summary>
		/// <param name="markerId">Marker identifier.</param>
        public void LocalMarkerFound(string markerId, int marker_internal_id)
        {
            stopTimers();

            if (_markerId == "" || _markerId != markerId)
            {
                _currentMarker = new MarkerInfo(markerId, marker_internal_id, (double)PikkartARCore.GetMarkerWidthInternalID(marker_internal_id),
                  (double)PikkartARCore.GetMarkerHeightInternalID(marker_internal_id));

                _dataProvider.ExecuteGetMarkerRequest(markerId, false, true);

                if (_currentRecognitionOptions.getMode() == RecognitionOptions.RecognitionMode.TAP_TO_SCAN)
                {
                    ForceMarkerSearch(markerId);
                    _recognitionRunning = false;
                }
            }
            else
            {
                _currentMarker.setARLogoCode(-1);
                _markerObjectListener.OnMarkerFound(_currentMarker);
                _recognitionListener.MarkerFound(_currentMarker);
            }
        }
        
        /// <summary>
		/// Called every _localMarkerNotFoundEventMinIntervalMillisecs if the marker is not found locally.
		/// </summary>
        public void LocalMarkerNotFound()
        {
            if (_currentRecognitionOptions.getStorage() != RecognitionOptions.RecognitionStorage.LOCAL && _recognitionRunning)
            {
                if (!IsConnectionAvailable())
                {
                    _recognitionListener.InternetConnectionNeeded();
                }
                else if (!_dataProvider.isFindMarkerRequestRunning())
                {
                    //int size = PikkartARCore.GetServerBuffer(_nativeArray);
                    //if (size > 0)
                    //{
                        //string imageDescriptor = Marshal.PtrToStringAnsi(_nativeArray);
                        _recognitionListener.ExecutingCloudSearch();
                        _dataProvider.ExecuteFindMarkerRequest();
                    //}
                }
            }
        }

        /// <summary>
        /// Markers the tracking lost.
        /// </summary>
        /// <param name="markerId">Marker identifier.</param>
        public void MarkerTrackingLost(string markerId)
		{
            _markerObjectListener.OnMarkerLost(_currentMarker);
            _recognitionListener.MarkerTrackingLost(markerId);
            if (_currentRecognitionOptions.getMode() == RecognitionOptions.RecognitionMode.TAP_TO_SCAN)
            {
                _recognitionRunning = false;
            }
        }
	
        public void ARLogoFound(int marker_internal_id, int logo_payload)
        {
            Debug.Log("ARLogoFound " + logo_payload);
            if (_currentMarker!=null && _currentMarker.getInternalId() == marker_internal_id && _currentMarker.getARLogoCode()!=logo_payload)
            {
                _currentMarker.setARLogoCode(logo_payload);
                _markerObjectListener.OnARLogoFound(_currentMarker, logo_payload);
                _recognitionListener.ARLogoFound(_currentMarker, logo_payload);
            }
        }

		/// <summary>
		/// FindMarker callback. Called by a DataProvider object after a cloud recognition service callback.
		/// </summary>
		/// <param name="marker">Marker.</param>
		public void FindMarkerCallback(Marker marker)
		{
            if (marker == null)
            {
                _cloudMarkerNotFoundCount++;
                _recognitionListener.CloudMarkerNotFound();
            }
            else
                 _dataProvider.CheckLocalMarkerCache(marker.markerId, true);
         }

        public void FindSimilarMarkerCallback(string[] markersId)
        {
            
        }

        public void GetMarkerCallback(Marker marker)
        {
            _currentMarker.setCustomData(marker.customData);
            _currentMarker.setDatabase(new MarkerDatabaseInfo(marker.markerDatabase.id, 
                marker.markerDatabase.customData, marker.markerDatabase.cloud));
            _currentMarker.setARLogoEnabled(marker.arLogoEnabled);

            _markerObjectListener.OnMarkerFound(_currentMarker);
            _recognitionListener.MarkerFound(_currentMarker);

            _markerId = marker.markerId;
        }

        public void LoadLocalMarkersOnStartup(bool isRecognitionManagerRunning, string[] markerIds, string[] discoverIds)
        {
           _dataProvider.LoadLocalMarkersOnStartup(isRecognitionManagerRunning, markerIds, discoverIds);
            SavedLocalModels = true;
        }

        public static void SyncMarkersWithFolder()
        {
            PikkartARCore.SyncMarkersWithFolder();
        }

        public bool IsConnectionAvailable()
        {
            return _recognitionListener.IsConnectionAvailable();
        }


        private PikkartARCamera _pikk_camera;
        public void SetPikkartCamera(PikkartARCamera camera)
        {
            _pikk_camera = camera;
        }

        public void GetCameraParamCallback(double[] cam_params, bool save)
        {
            if (cam_params[0] != -1.0)
            {
                if (save)
                {
                    SaveCameraParams(cam_params);
                }

                PikkartARCore.SetProjectionMatrix2((float)cam_params[0],
                    (float)cam_params[1],
                    (float)cam_params[2],
                    (float)cam_params[3]);

                _pikk_camera.SetCameraParameters(cam_params[0], cam_params[1], cam_params[2], cam_params[3]);
            }
        }

        public void SaveCameraParams(double[] cam_params)
        {
            PlayerPrefs.SetFloat("cam_param1", (float)cam_params[0]);
            PlayerPrefs.SetFloat("cam_param2", (float)cam_params[1]);
            PlayerPrefs.SetFloat("cam_param3", (float)cam_params[2]);
            PlayerPrefs.SetFloat("cam_param4", (float)cam_params[3]);
            PlayerPrefs.Save();
        }

        public bool LoadSavedCameraParams()
        {
            bool ret = PlayerPrefs.HasKey("cam_param1") &&
                PlayerPrefs.HasKey("cam_param2") &&
                PlayerPrefs.HasKey("cam_param3") &&
                PlayerPrefs.HasKey("cam_param4");
            if(ret)
            {
                float[] cam_params = new float[4];
                cam_params[0] = PlayerPrefs.GetFloat("cam_param1");
                cam_params[1] = PlayerPrefs.GetFloat("cam_param2");
                cam_params[2] = PlayerPrefs.GetFloat("cam_param3");
                cam_params[3] = PlayerPrefs.GetFloat("cam_param4");
                PikkartARCore.SetProjectionMatrix2(cam_params[0],
                    cam_params[1],
                    cam_params[2],
                    cam_params[3]);

                _pikk_camera.SetCameraParameters(cam_params[0], cam_params[1], cam_params[2], cam_params[3]);
            }
            return ret;
        }

        public static bool LoadDiscoverModel(string modelname)
        {
            Debug.Log("Loading discover model " + modelname);
#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR_WIN)
            String sbId = modelname.ToString();
#else
            StringBuilder sbId = new StringBuilder(modelname.ToString());
#endif
            return PikkartARCore.LoadDiscoverModel(sbId);
        }

        public static void StartDiscover()
        {
            PikkartARCore.EnableDiscover(true);
        }

        public static void StopDiscover()
        {
            PikkartARCore.EnableDiscover(false);
        }

        public static Vector2 GetPositionOfInterestPoint(int ip_internal_id)
        {
            HandledArray<float> position = new HandledArray<float>(2);
            PikkartARCore.GetPositionOfInterestPoint(ip_internal_id, position.GetArray());
            return new Vector2(position.GetArray()[0], position.GetArray()[1]);
        }

        public static String GetInterestPointPublicID(int ip_internal_id)
        {
            IntPtr ipFoundPtr = Marshal.AllocHGlobal(512);
            int size = PikkartARCore.GetInterestPointPublicID(ip_internal_id, ipFoundPtr);
            String ipFound = Marshal.PtrToStringAnsi(ipFoundPtr, size - 1);
            return ipFound;
        }
        public static int[] GetActiveInterestPoints()
        {
            HandledArray<int> ipIds = new HandledArray<int>(10);
            int sz = PikkartARCore.GetActiveInterestPoints(ipIds.GetArray());
            int[] ret = new int[sz];
            for (int i = 0; i < sz; ++i) ret[i] = ipIds.GetArray()[i];
            return ret;
        }
    }
}

