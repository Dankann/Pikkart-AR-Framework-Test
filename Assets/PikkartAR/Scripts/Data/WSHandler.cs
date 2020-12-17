using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;

/*
 *  Classe template per gestire una generica richiesta web
 */

namespace PikkartAR {

	/// <summary>
	/// Web Services Handler.
	/// Handles generic web requests and timeouts.
	/// </summary>
	public class WSHandler {

        private WWW m_www = null;
		private const string POST = "POST";
		private const string GET = "GET";

		public delegate void ErrorCallback (string error);
		
		public delegate void SuccessCallbackWithResponse<T> ( T response = default(T) );

		/// <summary>
		/// Timeout coroutine
		/// </summary>
		/// <param name="timeout">Timeout length.</param>
		public IEnumerator StartTimeout(float timeout = Constants.DEFAULT_NET_TIMEOUT_SEC)
		{
//			Debug.Log ("StartingTimeout");
			yield return new WaitForSeconds(timeout);
//			Debug.Log ("Timeout!");
			ClearWWW();
		}

		/// <summary>
		/// Web service request coroutine.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="requestBody">Request body.</param>
		/// <param name="callbackWithResponse">Callback with response.</param>
		/// <param name="errorCallback">Error callback.</param>
		/// <typeparam name="T">Response type.</typeparam>
		public IEnumerator CallWS<T> (
			string url, 
			Dictionary<string, string> requestBody = null,
			SuccessCallbackWithResponse <T> callbackWithResponse = null,
			ErrorCallback errorCallback = null)
		{
			m_www = null;
			yield return 0;
			
			byte[] requestBodyBytes = null;
			byte[] escapedRequestBodyBytes = null;
			if (requestBody != null) {
				requestBodyBytes = NetUtilites.GetRequestBody(requestBody);
				escapedRequestBodyBytes = NetUtilites.GetEscapedRequestBody(requestBody);
			}
			yield return 0;
			
			Dictionary<string, string> headers = NetUtilites.GetHeaders (
				requestBodyBytes == null ? GET : POST,
				new Uri(url).PathAndQuery,
				requestBodyBytes);

			yield return 0;

			m_www = new WWW(url, escapedRequestBodyBytes, headers);

			PikkartARHelper.Instance.LaunchCoroutine (StartTimeout());
			yield return 0;
#if UNITY_IPHONE
			while (m_www != null && !m_www.isDone) { yield return null; }
#else
			yield return m_www; // Cannot be interrupted by m_www.Dispose() on iOS
#endif
			
			if (m_www == null) {
				Debug.LogWarning ("WWW timed out!");
				if (errorCallback != null) errorCallback("timeout");
			} else {
				if (m_www.error == null || m_www.error.Length==0)
				{
					//Debug.Log ("WWW Ok: " + m_www.text);
					try {
						WSResponse response = JsonUtilities.ToObject<WSResponse> (m_www.text);

						//Debug.Log ("CallWS deserialized response to object");
						if (response.result.code == 200) {
							//Debug.Log ("Response result code is 200");
							T specificResponse = JsonUtilities.ToObject<T> (m_www.text);
							//Debug.Log ("CallWS deserialized response to specific object");
							if (callbackWithResponse != null) callbackWithResponse (specificResponse);
						} else {
							Debug.LogWarning (response.result.message);
							if (errorCallback != null) errorCallback(response.result.message);
						}
					} catch (Exception e) {
						Debug.LogError ("CallWS exception catched: " + e.Message);
						if (errorCallback != null) errorCallback(e.Message);
					}
				} else {
					Debug.LogWarning ("WWW Error: " + m_www.error);
					if (errorCallback != null) errorCallback(m_www.error);
				}
			}
			
			yield return 0;
			ClearWWW ();
		}

        /// <summary>
        /// Invalidates or clears a request.
        /// </summary>
        public void ClearWWW()
		{
			if (m_www != null) {
				m_www.Dispose();		// android
				m_www = null;			// ios
				System.GC.Collect();
			}
		}
    }
}
