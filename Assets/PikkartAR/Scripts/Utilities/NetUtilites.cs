using UnityEngine;
using System.Collections.Generic;
using System;

namespace PikkartAR {

	public class NetUtilites {

		public const string UNITY_CONTEXT_REQUEST = "1";

    	public const string AUTH_HEADER_KEY = "Authorization";
		public const string DATE_HEADER_KEY = "PkDate";
		public const string UAGN_HEADER_KEY = "User-Agent";
		public const string UAGN_HEADER_VAL = "PkRs 1.0";

		public static byte[] GetRequestBody (Dictionary<string, string> bodyDict) {
			string requestBodyString = GetStringParamsFromDictionary (bodyDict);
			return System.Text.Encoding.UTF8.GetBytes (requestBodyString);
		}

		public static byte[] GetEscapedRequestBody (Dictionary<string, string> bodyDict) {
			string requestBodyString = GetStringParamsFromDictionary (bodyDict);
			requestBodyString = WWW.EscapeURL (requestBodyString);
			return System.Text.Encoding.UTF8.GetBytes (requestBodyString);
		}

		public static string GetStringParamsFromDictionary (Dictionary<string, string> dictionary) {
			if (dictionary == null) return "";
			string requestBody = "";
			foreach (KeyValuePair<string, string> param in dictionary)
				requestBody += param.Key + "=" + param.Value + "&";
			requestBody = requestBody.Substring(0, requestBody.Length - 1);

			return requestBody;
		}

		public static Dictionary<string, string> GetHeaders (string method, string path, string body) {
			Dictionary<string, string> headers = GetEmptyHeaders ();
			string date = GetDate ();
            
            headers[AUTH_HEADER_KEY] = CryptUtilites.AuthorizationCode(method, body, date, path);
			headers[DATE_HEADER_KEY] = date;
			headers[UAGN_HEADER_KEY] = UAGN_HEADER_VAL;

			return headers;
		}

		public static Dictionary<string, string> GetHeaders (string method, string path, byte[] body = null) {
			string bodyString = "";
			if (body != null)
                bodyString = System.Text.Encoding.UTF8.GetString (body);

			return GetHeaders (method, path, bodyString);
		}

		public static Dictionary<string, string> GetEmptyHeaders () {
            return new WWWForm().headers;
        }

		public static string GetDate () {
			return DateTime.Now.ToUniversalTime().ToString (Constants.DATE_FORMAT);
		}

		private static void WriteLogFile(string authorizationCode, string method, string body, string date, string path){
			#if !UNITY_WSA_10_0 && !UNITY_WP_8_1
			System.IO.StreamWriter sw = System.IO.File.CreateText (Application.persistentDataPath + "/logNetUtils.txt");
			sw.WriteLine (method);
			sw.WriteLine (body);
			sw.WriteLine (date);
			sw.WriteLine (path);
			sw.WriteLine (authorizationCode);
			sw.Close ();
			#endif
		}
	}
}
