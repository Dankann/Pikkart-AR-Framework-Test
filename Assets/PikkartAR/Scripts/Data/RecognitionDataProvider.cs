using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace PikkartAR
{
    /// <summary>
    /// Handles access to data between local and cloud storage.
    /// Also handles cloud requests management. Works as a bridge between the recognition manager and local/cloud recognition services.
    /// </summary>
    public class RecognitionDataProvider {

        private IRecognitionDataProviderListener _dataProviderListener;
        private LocalRecognitionService _localRecognitionService;
        private CloudRecognitionService _cloudRecognitionService;
        private Marker _cachedMarker;

        private bool _findMarkerRequestRunning = false;
        private bool _getMarkerRequestRunning = false;
        private bool _getcameraparamsRunning = false;

        private IntPtr _nativeArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="PikkartAR.DataProvider"/> class.
        /// </summary>
        /// <param name="cloudRecognitionServiceListener">Cloud recognition service listener.</param>
        /// <param name="localRecognitionService">Local recognition service.</param>
        /// <param name="deviceId">Device identifier.</param>
        public RecognitionDataProvider(CloudRecognitionInfo authInfo,
            IRecognitionDataProviderListener recognitionDataProviderListener, string deviceId)
        {
            _nativeArray = Marshal.AllocHGlobal(Constants.CAMERA_REQUESTED_WIDTH * Constants.CAMERA_REQUESTED_HEIGHT * 3);
            _dataProviderListener = recognitionDataProviderListener;
            _localRecognitionService = new LocalRecognitionService();
            _cloudRecognitionService = new CloudRecognitionService(authInfo, deviceId);
            _cachedMarker = null;
        }

        

        /// <summary>
        /// Find marker request running control function.
        /// </summary>
        /// <returns><c>true</c>, if find marker request is running, <c>false</c> otherwise.</returns>
        public bool isFindMarkerRequestRunning() {
            return _findMarkerRequestRunning;
        }

        /// <summary>
        /// Instructs the cloud recognition service to execute a FindMarker request.
        /// </summary>
        /// <param name="imageBase64">Image base64.</param>
        public void ExecuteFindMarkerRequest()
        {
            if (_findMarkerRequestRunning)
                return;

            _findMarkerRequestRunning = true;

            int size = PikkartARCore.GetUpdatedServerBuffer(_nativeArray);            
            if (size == 0)
            {
                _findMarkerRequestRunning = false;
                return;
            }
            string imageDescriptor = Marshal.PtrToStringAnsi(_nativeArray);            
            PikkartARHelper.Instance.LaunchCoroutine(_cloudRecognitionService.FindMarker(imageDescriptor, FindMarkerCallbackOK, FindMarkerCallbackKO));
        }
      
        /// <summary>
        /// Successful FindMarker request callback function.
        /// </summary>
        /// <param name="response">FindMarker response.</param>
        public void FindMarkerCallbackOK(WSMarkerResponse response) {
        
            DateTime now = DateTime.Now.ToUniversalTime();
            Marker marker = new Marker(response.data.markerId,
                response.data.markerDescriptor,
                response.data.markerCustomData,
                DateTime.Parse(response.data.markerUpdateDate),
                now, now,
                DateTime.Parse(response.data.publishedFrom),
                DateTime.Parse(response.data.publishedTo),
                response.data.cacheEnabled,
                new MarkerDatabase(response.data.markerDatabase.id,
                response.data.markerDatabase.code,
                response.data.markerDatabase.customData,
                response.data.markerDatabase.cloud,
                now, now),
                response.data.arLogoEnabled);

            string appdatapath = RecognitionManager.GetAppDataPath();
            Thread thread = new Thread(() => SaveLocalMarkerThread(marker, true, true, appdatapath, true));
            thread.Start();
            //SaveLocalMarker(marker, true, true);

            _findMarkerRequestRunning = false;
            Thread thread2 = new Thread(() => _dataProviderListener.FindMarkerCallback(marker));
            thread2.Start();
            //_dataProviderListener.FindMarkerCallback(marker);
        }

        /// <summary>
        /// Unsuccessful FindMarker request callback function.
        /// </summary>
        /// <param name="error">Error data.</param>
        public void FindMarkerCallbackKO(string error) {
            Debug.Log("not found");
            if (error != null && error != "Marker not found")
                Debug.LogWarning(error);

            _findMarkerRequestRunning = false;
            _dataProviderListener.FindMarkerCallback(null);
        }

        /// <summary>
		/// Get marker request running control function.
		/// </summary>
		/// <returns><c>true</c>, if get marker request is running, <c>false</c> otherwise.</returns>
		public bool isGetMarkerRequestRunning()
        {
            return _getMarkerRequestRunning;
        }

        /// <summary>
        /// Instructs the cloud recognition service to execute a GetMarker request.
        /// </summary>
        /// <param name="markerId">marker Id.</param>
        /// <param name="getDescriptor">.</param>
        public void ExecuteGetMarkerRequest(string markerId, bool getDescriptor, bool oneAtATime)
        {
            if (_getMarkerRequestRunning && oneAtATime)
                return;

            _getMarkerRequestRunning = true;

            if (_cachedMarker == null || _cachedMarker.markerId != markerId)
            {
                _cachedMarker = _localRecognitionService.GetMarker(markerId);

                if (_cachedMarker != null && !_cachedMarker.markerDatabase.cloud)
                {
                    System.Threading.Thread thread = new System.Threading.Thread(() => _localRecognitionService.UpdateAccessDate(_cachedMarker));
                    thread.Start();                  
                }
                else
                {
                    if (_dataProviderListener.IsConnectionAvailable() && (_cachedMarker == null || !_cachedMarker.IsPublished() || _cachedMarker.IsObsolete()))
                    {
                        PikkartARHelper.Instance.LaunchCoroutine(_cloudRecognitionService.GetMarker(markerId, getDescriptor, GetMarkerCallbackOK, GetMarkerCallbackKO));
                        return;
                    }
                }
            }
            _dataProviderListener.GetMarkerCallback(_cachedMarker);
            _getMarkerRequestRunning = false;
        }

        public void ExecuteGetMarkerRequestThread(string markerId, bool getDescriptor, bool oneAtATime,bool isConnectionAvailable)
        {
            if (_getMarkerRequestRunning && oneAtATime)
                return;

            _getMarkerRequestRunning = true;

            if (_cachedMarker == null || _cachedMarker.markerId != markerId)
            {
                _cachedMarker = _localRecognitionService.GetMarker(markerId);

                if (_cachedMarker != null && !_cachedMarker.markerDatabase.cloud)
                {
                    _localRecognitionService.UpdateAccessDate(_cachedMarker);
                }
                else
                {
                    if (isConnectionAvailable && (_cachedMarker == null || !_cachedMarker.IsPublished() || _cachedMarker.IsObsolete()))
                    {
                        PikkartARHelper.Instance.LaunchCoroutine(_cloudRecognitionService.GetMarker(markerId, getDescriptor, GetMarkerCallbackOK, GetMarkerCallbackKO));
                        return;
                    }
                }
            }

            _dataProviderListener.GetMarkerCallback(_cachedMarker);
            _getMarkerRequestRunning = false;
        }

        /// <summary>
        /// Successful GetMarker request callback function.
        /// </summary>
        /// <param name="response">GetMarker response.</param>
        public void GetMarkerCallbackOK(WSMarkerResponse response)
        {
            DateTime now = DateTime.Now.ToUniversalTime();
            Marker cloudMarker = new Marker(response.data.markerId,
                response.data.markerDescriptor,
                response.data.markerCustomData,
                DateTime.Parse(response.data.markerUpdateDate),
                now, now,
                DateTime.Parse(response.data.publishedFrom),
                DateTime.Parse(response.data.publishedTo),
                response.data.cacheEnabled,
                new MarkerDatabase(response.data.markerDatabase.id,
                response.data.markerDatabase.code,
                response.data.markerDatabase.customData,
                response.data.markerDatabase.cloud,
                now, now),
                response.data.arLogoEnabled);

            if (cloudMarker.IsPublished())
            {
                if (cloudMarker.markerDescriptor != null)
                    SaveLocalMarker(cloudMarker, true, _cachedMarker != null, true);
                else
                    _localRecognitionService.SaveMarker(cloudMarker, _cachedMarker != null);
                    
                _cachedMarker = cloudMarker;
                _dataProviderListener.GetMarkerCallback(_cachedMarker);
            }
            else
            {
                if (_cachedMarker != null)
                    _localRecognitionService.DeleteMarker(_cachedMarker.markerId);
                _cachedMarker = null;
            }
                
            _getMarkerRequestRunning = false;
        }

        /// <summary>
        /// Unsuccessful GetMarker request callback function.
        /// </summary>
        /// <param name="error">Error data.</param>
        public void GetMarkerCallbackKO(string error)
        {
            /*if (_cachedMarker != null)
                _localRecognitionService.DeleteMarker(_cachedMarker.markerId);
            _cachedMarker = null;*/

            _getMarkerRequestRunning = false;
        }

        public void LoadLocalDiscoverModelsOnStartUp(string[] discoverIds)
        {
            //save all local discover models
            string srcDiscoverPath = Application.streamingAssetsPath + "/markers3D/";
#if UNITY_EDITOR || UNITY_IOS
            if (!Directory.Exists(Application.streamingAssetsPath) || !Directory.Exists(srcDiscoverPath))
                return;
#endif
            string dstDiscoverPath = RecognitionManager.GetAppDataPath() + "markers3D/";

            if (!Directory.Exists(dstDiscoverPath))
            {
                Directory.CreateDirectory(dstDiscoverPath);
            }
            else
            {
                string[] discover_files = Directory.GetFiles(dstDiscoverPath);
                foreach (string ff in discover_files)
                {
                    File.Delete(ff);
                }
            }
#if UNITY_EDITOR || UNITY_IOS
            foreach (string dicoverId in discoverIds)
            {
                
                if (dicoverId == "") continue;
                string filename = Path.GetFileName(dicoverId + ".dat");
                string discoverSrcFile = srcDiscoverPath + filename;
                string discoverDstFile = dstDiscoverPath + filename;
                Debug.Log("Saving discover model " + discoverDstFile);
                File.WriteAllBytes(discoverDstFile, File.ReadAllBytes(discoverSrcFile));
            }
#else
            foreach (string dicoverId in discoverIds)
            {
                if (dicoverId == "") continue;
                string filename = Path.GetFileName(dicoverId + ".dat");
                WWW reader = new WWW(srcDiscoverPath + filename);
			    while (!reader.isDone) { }
			    if (!String.IsNullOrEmpty(reader.error)) {
				    continue;
			    } 
                string discoverDstFile = dstDiscoverPath + filename;
                Debug.Log("Saving discover model " + discoverDstFile);
                File.WriteAllBytes(discoverDstFile, reader.bytes);
            }
#endif
        }

        public void LoadLocalMarkersOnStartup(bool isRecognitionManagerRunning, string[] markerIds, string[] discoverIds)
        {
            LoadLocalDiscoverModelsOnStartUp(discoverIds);

            if (markerIds.Length < 1)
                return;

            string srcMarkerPath = Application.streamingAssetsPath + "/markers/";
#if UNITY_EDITOR || UNITY_IOS
            if (!Directory.Exists(Application.streamingAssetsPath) || !Directory.Exists(srcMarkerPath))
                return;
#endif
            string dstMarkerPath = RecognitionManager.GetAppDataPath() + "markers/";

            if (!Directory.Exists(dstMarkerPath))
            {
                Directory.CreateDirectory(dstMarkerPath);
            }
            else
            {
                string[] files = Directory.GetFiles(dstMarkerPath);
                foreach (string ff in files)
                {
                    File.Delete(ff);
                }
            }

            foreach (string markerId in markerIds)
            {
                Debug.Log("Loading marker " + markerId);
                if (markerId == "") continue;

                string markerString = null;

#if UNITY_EDITOR || UNITY_IOS
                markerString = File.ReadAllText(srcMarkerPath + markerId + ".dat");
#else
				WWW reader = new WWW(srcMarkerPath + markerId + ".dat");
				while (!reader.isDone) { }
				if (!String.IsNullOrEmpty(reader.error)) {
					continue;
				}
				markerString = reader.text;                
#endif
                if (markerString.Length > 0)
                {
                    Debug.Log("Loading marker save " + markerId);
                    WSMarkerResponse.WSMarker markerData = JsonUtilities.ToObject<WSMarkerResponse.WSMarker>(markerString);
                    DateTime now = DateTime.Now.ToUniversalTime();
                    Marker marker = new Marker(markerData.markerId,
                        markerData.markerDescriptor, markerData.markerCustomData,
                        DateTime.Parse(markerData.markerUpdateDate), now, now,
                        markerData.publishedFrom != null ? (DateTime?)DateTime.Parse(markerData.publishedFrom) : null,
                        markerData.publishedTo != null ? (DateTime?)DateTime.Parse(markerData.publishedTo) : null,
                        markerData.cacheEnabled,
                        new MarkerDatabase(markerData.markerDatabase.id, markerData.markerDatabase.code,
                        markerData.markerDatabase.customData, markerData.markerDatabase.cloud, now, now),
                        markerData.arLogoEnabled);

                    SaveLocalMarker(marker, isRecognitionManagerRunning, true, false);
                }
            }
            PikkartARCore.SyncMarkersWithFolder();
        }

        public static void SaveLocalMarkerFile(string markerId, byte[] markerDescriptor)
        {
            string markerFileName = markerId + ".dat";
            string markersFolder = RecognitionManager.GetAppDataPath() + "markers";

            string markerFilePath = markersFolder + "/" + markerFileName;

            if (!Directory.Exists(markersFolder))
                Directory.CreateDirectory(markersFolder);

            if (!File.Exists(markerFilePath))
                File.WriteAllBytes(markerFilePath, markerDescriptor);
        }

        /// <summary>
        /// Instruct the local recognition service to save a marker.
        /// </summary>
        /// <param name="marker">Marker.</param>
        public void SaveLocalMarker(Marker marker, bool isRecognitionManagerRunning, bool replace, bool sync_after_save)
        {
            if (replace || !File.Exists(RecognitionManager.GetAppDataPath() +"markers/" + marker.markerId + ".dat"))
            {
				string dstMarkerPath = RecognitionManager.GetAppDataPath() + "markers/";
				if (!Directory.Exists(dstMarkerPath))
				{
					Directory.CreateDirectory(dstMarkerPath);
				}
				
                if (isRecognitionManagerRunning)
                {
#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR_WIN)
                    String sbId = marker.markerId.ToString();
                    String sbDescriptor = marker.markerDescriptor;
#else
                    StringBuilder sbId = new StringBuilder(marker.markerId.ToString());
                    StringBuilder sbDescriptor = new StringBuilder(marker.markerDescriptor);
#endif
                    Debug.Log("Loading marker PikkartARCore.SaveLocalMarker " + sbId);
                    PikkartARCore.SaveLocalMarker(sbId, sbDescriptor);
                }
                else
                    SaveLocalMarkerFile(marker.markerId, Convert.FromBase64String(marker.markerDescriptor));

                if (sync_after_save)
                    PikkartARCore.SyncMarkersWithFolder();
            }

            marker.markerDescriptor = null;
            _localRecognitionService.SaveMarker(marker, _localRecognitionService.GetMarker(marker.markerId) != null);
        }

        public void SaveLocalMarkerThread(Marker marker, bool isRecognitionManagerRunning, bool replace, string appdatapath, bool sync_after_save)
        {
            if (replace || !File.Exists(appdatapath + "markers/" + marker.markerId + ".dat"))
            {
                string dstMarkerPath = appdatapath + "markers/";
                if (!Directory.Exists(dstMarkerPath))
                {
                    Directory.CreateDirectory(dstMarkerPath);
                }

                if (isRecognitionManagerRunning)
                {
#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR_WIN)
                    String sbId = marker.markerId.ToString();
                    String sbDescriptor = marker.markerDescriptor;
#else
                    StringBuilder sbId = new StringBuilder(marker.markerId.ToString());
                    StringBuilder sbDescriptor = new StringBuilder(marker.markerDescriptor);
#endif
                    PikkartARCore.SaveLocalMarker(sbId, sbDescriptor);
                }
                else
                    SaveLocalMarkerFile(marker.markerId, Convert.FromBase64String(marker.markerDescriptor));

                if (sync_after_save)
                    PikkartARCore.SyncMarkersWithFolder();
            }

            marker.markerDescriptor = null;
            _localRecognitionService.SaveMarker(marker, true/*_localRecognitionService.GetMarker(marker.markerId) != null*/);
        }

        public void SyncLocalMarkers(bool isRecognitionManagerRunning)
        {
            _localRecognitionService.DeleteObsoleteLocalData(true, isRecognitionManagerRunning);

            if (!_dataProviderListener.IsConnectionAvailable())
                return;

            List<Marker> dbCloudMarkers = _localRecognitionService.GetCloudMarkersList();

            int markerToSyncCount = Math.Max(RecognitionManager.MarkerCacheSize - (_localRecognitionService.GetAllDbMarkersCount() - dbCloudMarkers.Count), 0);

            PikkartARHelper.Instance.LaunchCoroutine(_cloudRecognitionService.GetMarkersToSync(
                dbCloudMarkers, markerToSyncCount, SyncLocalMarkersCallbackOK, SyncLocalMarkersCallbackKO));
        }

        public void SyncLocalMarkersCallbackOK(WSMarkersSyncResponse response) {

			// delete
			if (response.data.toDelete != null && response.data.toDelete.Count > 0) {
				foreach (string markerId in response.data.toDelete) {
					_localRecognitionService.DeleteMarker(markerId);
				}
			}

			// update
			if (response.data.toUpdate != null && response.data.toUpdate.Count > 0) {
				foreach (string markerId in response.data.toUpdate) {
					ExecuteGetMarkerRequest(markerId, true, false);
				}
			}

			// download
			if (response.data.toDownload != null && response.data.toDownload.Count > 0) {
				foreach (string markerId in response.data.toDownload) {
                    ExecuteGetMarkerRequest(markerId, true, false);
				}
			}

		}

		public void SyncLocalMarkersCallbackKO(string error) {
			if (error != null)
				Debug.LogWarning(error);
		}

        public void CheckLocalMarkerCache(String markerToKeepId, bool isRecognitionRunning)
        {
            _localRecognitionService.DeleteOldestMarkers(
                    Math.Max(_localRecognitionService.GetAllDbMarkersCount() - RecognitionManager.MarkerCacheSize, 0),
                    markerToKeepId, isRecognitionRunning);
        }

        public void GetCameraParamsCallbackOK(WSCameraParamsResponse response)
        {
            double[] cam_params = new double[4];
            byte[] byte_array;
            byte_array = Convert.FromBase64String(response.data);
            float[] paramsf = new float[4];

            IntPtr byte_send;
            byte_send = Marshal.AllocHGlobal(20);
            Marshal.Copy(byte_array, 0, byte_send, 20);
            PikkartARCore.DecryptCalibParams(byte_send, paramsf);
            cam_params[0] = paramsf[0];
            cam_params[1] = paramsf[1];
            cam_params[2] = paramsf[2];
            cam_params[3] = paramsf[3];

            _dataProviderListener.GetCameraParamCallback(cam_params, true);
            _getMarkerRequestRunning = false;
        }

        public void GetCameraParamsCallbackKO(string error)
        {
            
            _getMarkerRequestRunning = false;
        }

        public void ExecuteGetCameraParamFromServer(bool oneAtATime)
        {
            if (_getcameraparamsRunning && oneAtATime)
                return;

            _getcameraparamsRunning = true;

            double[] cam_params = new double[4];
            if (_dataProviderListener.IsConnectionAvailable())
            {
                PikkartARHelper.Instance.LaunchCoroutine(_cloudRecognitionService.GetCameraParams(GetCameraParamsCallbackOK, GetCameraParamsCallbackKO));
                return;
            }
        }
    }
}