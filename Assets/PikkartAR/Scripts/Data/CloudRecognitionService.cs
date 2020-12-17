using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
/*
*  Gestisce le richieste al server
*  Ogni metodo prende per parametro due metodi per gestire la risposta 
*  in caso di richiesta andata a buon fine o meno.
*/

namespace PikkartAR {

	/// <summary>
	/// Cloud recognition service.
	/// Handles cloud service coroutine requests and callbacks.
	/// </summary>
	public class CloudRecognitionService {

        private CloudRecognitionInfo _cloudInfo;
        private string _deviceId;
        

        /// <summary>
        /// Initializes a new instance of the <see cref="PikkartAR.CloudRecognitionService"/> class.
        /// </summary>
        /// <param name="dataProvider">Data provider.</param>
        /// <param name="deviceId">Device identifier.</param>
        public CloudRecognitionService (CloudRecognitionInfo cloudInfo, string deviceId)
		{
            _cloudInfo = cloudInfo;
            _deviceId = deviceId;
            
        }

		/// <summary>
		/// Coroutine to call the FindMarker cloud service.
		/// </summary>
		/// <returns>The marker.</returns>
		/// <param name="descriptor">Descriptor.</param>
		/// <param name="sceneId">Scene identifier.</param>
		/// <param name="callbackOK">Successful callback.</param>
		/// <param name="callbackKO">Error callback.</param>
		public IEnumerator FindMarker(string imageDescriptor, 
            WSHandler.SuccessCallbackWithResponse<WSMarkerResponse> callbackOK,
            WSHandler.ErrorCallback callbackKO)
		{           
            Dictionary<string, string> postData = new Dictionary<string, string>();
			postData.Add("contextType", NetUtilites.UNITY_CONTEXT_REQUEST);
			postData.Add("dbCode", String.Join(",", _cloudInfo.getDatabaseNames()));
			postData.Add("descriptor", imageDescriptor);
			postData.Add("deviceId", _deviceId);
			
			string url = _cloudInfo.GetWebApiUrl() + "/ar/reco/markers/?find";

            return new WSHandler().CallWS<WSMarkerResponse> (url, postData, callbackOK, callbackKO);
		}
		
		/// <summary>
		/// Coroutine to call the GetMarker cloud service.
		/// </summary>
		/// <returns>The marker.</returns>
		/// <param name="markerId">Marker identifier.</param>
		/// <param name="getDescriptor">If set to <c>true</c> get descriptor.</param>
		/// <param name="callbackOK">Successful callback.</param>
		/// <param name="callbackKO">Unsuccessful callback.</param>
		public IEnumerator GetMarker (string markerId, bool getDescriptor,
			WSHandler.SuccessCallbackWithResponse<WSMarkerResponse> callbackOK,
			WSHandler.ErrorCallback callbackKO) {
			
			Dictionary<string, string> body = new Dictionary<string, string> ();
			body.Add("contextType", NetUtilites.UNITY_CONTEXT_REQUEST);
			body.Add("deviceId", _deviceId);

            string query = getDescriptor ? "deviceCompleteInfo" : "deviceInfo";
            string url = _cloudInfo.GetWebApiUrl() + "/ar/reco/markers/" + markerId + "/?" + query;

            return new WSHandler().CallWS<WSMarkerResponse> (url, body, callbackOK, callbackKO);
		}

		public IEnumerator GetMarkersToSync(
			List<Marker> localMarkers,
			int maxMarkersCapacity,
			WSHandler.SuccessCallbackWithResponse<WSMarkersSyncResponse> callbackOK,
			WSHandler.ErrorCallback callbackKO){

			Dictionary<string, string> postData = new Dictionary<string, string>();
			string localMarkersId = null;
			string localMarkersUpdateDates = null;
			string localMarkersAccessDates = null;

			if (localMarkers != null && localMarkers.Count > 0) {
				string dateFormat = "dd/MM/yyyy HH:mm:ss";

				foreach (Marker marker in localMarkers) {
                    //TODO implementa controlli sulle date
                    localMarkersId += marker.markerId + ",";
					localMarkersUpdateDates += marker.updateDate.ToString(dateFormat) + ",";
					localMarkersAccessDates += marker.lastAccessDate.ToString(dateFormat) + ",";
				}

				//tolgo le ',' in coda alle stringhe
				localMarkersId = localMarkersId.Substring(0, localMarkersId.Length - 1);
				localMarkersUpdateDates = localMarkersUpdateDates.Substring(0, localMarkersUpdateDates.Length - 1);
				localMarkersAccessDates = localMarkersAccessDates.Substring(0, localMarkersAccessDates.Length - 1);

				//Debug.Log("localMarkersId " + localMarkersId);
				//Debug.Log("localMarkersUpdateDates " + localMarkersUpdateDates);
				//Debug.Log("localMarkersAccessDates " + localMarkersAccessDates);

				postData.Add("ids", localMarkersId);
				postData.Add("updateDates", localMarkersUpdateDates);
				postData.Add("lastAccessDates", localMarkersAccessDates);
			}

			postData.Add("maxCapacity", maxMarkersCapacity.ToString());
			postData.Add("contextType", NetUtilites.UNITY_CONTEXT_REQUEST);
			postData.Add("deviceId", _deviceId);

			string url = _cloudInfo.GetWebApiUrl() + "/ar/reco/markers/?sync";

			return new WSHandler().CallWS<WSMarkersSyncResponse>(url, postData, callbackOK, callbackKO);
		}

        public IEnumerator GetCameraParams(WSHandler.SuccessCallbackWithResponse<WSCameraParamsResponse> callbackOK,
            WSHandler.ErrorCallback callbackKO)
        {
            string deviceModel = SystemInfo.deviceModel;
            Debug.Log("PikkartARDeviceInfo: deviceModel " + SystemInfo.deviceModel);
            Debug.Log("PikkartARDeviceInfo: deviceName " + SystemInfo.deviceName);
            Debug.Log("PikkartARDeviceInfo: deviceType " + SystemInfo.deviceType);
            Debug.Log("PikkartARDeviceInfo: deviceUniqueIdentifier " + SystemInfo.deviceUniqueIdentifier);
#if UNITY_EDITOR
            string cleanedDeviceModel = deviceModel.Replace(" ", "");
#else
            int charLocation = deviceModel.IndexOf(" ", StringComparison.Ordinal);
            string cleanedDeviceModel;
            if(charLocation<deviceModel.Length && charLocation>=0) {
                string deviceActualModel = deviceModel.Substring(charLocation, deviceModel.Length-charLocation);
                cleanedDeviceModel = deviceActualModel.Replace(" ", "");
            }
            else {
                cleanedDeviceModel = deviceModel.Replace(" ", "");
            }
#endif
            Debug.Log("PikkartARDeviceInfo: cleanedDeviceModel " + cleanedDeviceModel);

            string url = _cloudInfo.GetWebApiUrl() + "/ar/camera/" + cleanedDeviceModel;
            return new WSHandler().CallWS<WSCameraParamsResponse>(url, null, callbackOK, callbackKO);
        }
    }
}
