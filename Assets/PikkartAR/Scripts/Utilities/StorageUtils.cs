using UnityEngine;
using System.IO;
using System.Text;

namespace PikkartAR {

	public class StorageUtils {

		public const string MARKERS_FOLDER_NAME = "markers";

		private static string libraryMarkersPath = ""; 
		public static string GetLibraryMarkersPath ()
		{
			if (string.IsNullOrEmpty (libraryMarkersPath)) {
				libraryMarkersPath = Application.persistentDataPath + "/" + MARKERS_FOLDER_NAME + "/";
			}
			return libraryMarkersPath;
		}

		private static string sourceMarkerPath = "";
		public static string GetSourceMarkerPath ()
		{
			if (string.IsNullOrEmpty (sourceMarkerPath)) {
				sourceMarkerPath = Application.streamingAssetsPath + "/" + MARKERS_FOLDER_NAME + "/";
			}
			return sourceMarkerPath;
		}

		private static string appPersistentDataPath = "";
		private static StringBuilder appPersistendDataPathSb = null;
		public static string GetApplicationPersistentDataPath ()
		{
			if (string.IsNullOrEmpty (appPersistentDataPath)) {
				appPersistentDataPath = Application.persistentDataPath + "/";
			}
			return appPersistentDataPath;
		}
		public static StringBuilder GetApplicationPersistentDataPathStringBuilder ()
		{
			if (appPersistendDataPathSb == null) {
				appPersistendDataPathSb = new StringBuilder (GetApplicationPersistentDataPath ());
			}
			return appPersistendDataPathSb;
		}

		public static void CopyMarkerToLocalStorage (string markerFileName)
		{
			string markerLocalPath = GetLibraryMarkersPath () + markerFileName;
			string markerSourcePath = GetSourceMarkerPath () + markerFileName;

			if (!File.Exists (markerLocalPath)) {
				if (!Directory.Exists (GetLibraryMarkersPath ())) {
					try {
						Directory.CreateDirectory(GetLibraryMarkersPath ());
					} catch (System.IO.IOException) {}
				}

#if UNITY_ANDROID || UNITY_WP_8_1
				WWW reader = new WWW (markerSourcePath);
				while (!reader.isDone) {}
				File.WriteAllBytes (markerLocalPath, reader.bytes);
#else
				File.Copy (markerSourcePath, markerLocalPath);
				while (!File.Exists (markerLocalPath)) {}
#endif
			}
		}

		public static void DeleteMarkerFromLocalStorage (string markerName)
		{
			markerName = markerName.Replace (".dat", "");
			int markerId = int.Parse (markerName);
			DeleteMarkerFromLocalStorage (markerId);
		}

		public static void DeleteMarkerFromLocalStorage (int markerId)
		{
			string markerLocalPath = GetLibraryMarkersPath () + markerId + ".dat";

			if (File.Exists (markerLocalPath)) {
				File.Delete (markerLocalPath);
			}
		}
	}
}
