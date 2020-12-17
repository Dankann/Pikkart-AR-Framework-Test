using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace PikkartAR {

	public class CryptUtilites {

        const int SIGNATURE_LENGTH = 69;
        static IntPtr signaturePtr = Marshal.AllocHGlobal(1024);

        /*
         *  CreateServerSignature
         */
        public static string AuthorizationCode (string method, string body, string date, string path)
		{
#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR_WIN)
            PikkartARCore.CreateUnityServerSignature(method, body, date, path, signaturePtr);
            string signature_str = Marshal.PtrToStringAuto (signaturePtr);
#else
            PikkartARCore.CreateUnityServerSignature(new StringBuilder(method), new StringBuilder(body),
                    new StringBuilder(date), new StringBuilder(path), signaturePtr);
            string signature_str = Marshal.PtrToStringAnsi(signaturePtr);
#endif
            if (signature_str != null && signature_str.Length>=SIGNATURE_LENGTH) {
				string signature = signature_str.ToString ().Substring (0, SIGNATURE_LENGTH);
				return signature;
			} else {
				Debug.Log ("signature=" + signature_str);
				return "";
			}
		}
	}
}