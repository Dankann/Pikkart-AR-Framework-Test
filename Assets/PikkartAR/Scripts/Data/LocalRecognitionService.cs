using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

/*
 *  Gestisce interazione col database
 */

namespace PikkartAR
{
	/// <summary>
	/// Local recognition service.
	/// Handles local database interactions.
	/// Implements ILocalRecognitionService.
	/// </summary>
	public class LocalRecognitionService {

		DBUtilities dbu;

		/// <summary>
		/// Initializes a new instance of the <see cref="PikkartAR.LocalRecognitionService"/> class.
		/// </summary>
		public LocalRecognitionService() {
			dbu = new DBUtilities (Constants.DB_NAME);
		}

		#region MARKER
		/// <summary>
		/// Gets the marker from the db.
		/// </summary>
		/// <returns>The marker.</returns>
		/// <param name="markerId">Marker identifier.</param>
		public Marker GetMarker(string markerId)
		{
            List<Marker> markers = dbu.MarkerQuery("SELECT * FROM " +
               "Markers WHERE markerId='" + markerId + "'");
            /*Marker marker = dbu.GetDB()
                .Table<Marker>()
                .Where(x => x.markerId == markerId)
                .FirstOrDefault();*/

            if (/*marker!=null*/markers.Count!=0 && markers[0] != null)
            {
                //marker.markerDatabase = GetMarkerDatabase(marker.databaseId);
				markers[0].markerDatabase = GetMarkerDatabase(markers[0].databaseId);
            }

			return /*marker*/(markers.Count!=0 && markers[0] != null)?markers[0]:null;
		}

		/// <summary>
		/// Saves the marker.
		/// </summary>
		/// <param name="marker">Marker.</param>
		/// <param name="replace">If set to <c>true</c> replace.</param>
		public void SaveMarker(Marker marker, bool replace)
		{
			if (marker == null)
				return;
			
            //Debug.Log("saveMarker: " + marker.ToString());

			if (replace)
				dbu.InsertOrReplace<Marker>(marker);
			else
				dbu.Insert<Marker>(marker);

			dbu.UpdateLastAccessDate(marker);

			SaveMarkerDatabase(marker.markerDatabase, true);
		}

        public void UpdateAccessDate(Marker marker)
        {
            if (marker == null)
                return;

            dbu.UpdateLastAccessDate(marker);

            if (marker.markerDatabase == null)
                return;

            dbu.UpdateLastAccessDate(marker.markerDatabase);
        }

		/// <summary>
		/// Deletes the marker.
		/// </summary>
		/// <param name="markerId">Marker identifier.</param>
		public void DeleteMarker(string markerId)
		{
#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR_WIN)
            PikkartARCore.DeleteLocalMarker(markerId,true);
#else
            PikkartARCore.DeleteLocalMarker(new StringBuilder(markerId),true);
#endif
            dbu.Delete<Marker>(markerId);
		}

		/// <summary>
		/// Deletes the marker.
		/// </summary>
		/// <param name="marker">Marker object.</param>
		public void DeleteMarker(Marker marker) 
		{
#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR_WIN)
            PikkartARCore.DeleteLocalMarker(marker.markerId,true);
#else
            PikkartARCore.DeleteLocalMarker(new StringBuilder(marker.markerId),true);
#endif
            dbu.Delete<Marker>(marker);
		}
#endregion

        #region STATIC_DELETE_OBSOLETE
		/// <summary>
		/// Deletes obsolete local data.
		/// </summary>
		/// <param name="cacheMilliseconds">Cache milliseconds.</param>
		/// <param name="isRecognitionManagerRunning">is recognition manager running.</param>
		public void DeleteObsoleteLocalData(bool deleteFilesNotInSqlite, bool isRecognitionManagerRunning)
		{
			List<string> markersFileNames = new List<string>();
			List<Marker> dbMarkers = GetMarkersList();
			string markersPath = RecognitionManager.GetAppDataPath() + "markers";

            if (!Directory.Exists(markersPath))
                Directory.CreateDirectory(markersPath);
            
			if (dbMarkers.Count > 0) {
				foreach (Marker marker in dbMarkers) {
					string markerId = marker.markerId;
					string markerFileName = markerId + ".dat";
					DateTime? publishedTo = marker.publishedTo;

					markersFileNames.Add(markerFileName);
                    
                    if (publishedTo == null) continue;

                    if (publishedTo.Value.CompareTo(DateTime.Now.ToUniversalTime()) < 0) {
						if (isRecognitionManagerRunning) {
#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR_WIN)
                            PikkartARCore.DeleteLocalMarker(markerId,true);
#else
                            PikkartARCore.DeleteLocalMarker(new StringBuilder(markerId),true);
#endif
                        }
                        else {							
							string markerFilePath = markersPath + "/" + markerFileName;
							if (!Directory.Exists(markersPath))
								Directory.CreateDirectory(markersPath);
							if (File.Exists(markerFilePath))
								File.Delete(markerFilePath);
						}
					}
				}
			}

			if (deleteFilesNotInSqlite) {
				var info = new DirectoryInfo(markersPath);
				FileInfo[] filesInfo = info.GetFiles();

				foreach (FileInfo fileInfo in filesInfo) {
					if (fileInfo.Name.Contains(".dat") && !markersFileNames.Contains(fileInfo.Name)) {
						fileInfo.Delete();
					}
				}
			}

			dbu.DeleteObsoleteMarkers();
		}
        #endregion

        #region MARKER_DATABASE

		public MarkerDatabase GetMarkerDatabase(string id)
		{
            List<MarkerDatabase> markerDatabases = dbu.MarkerDBQuery("SELECT * FROM " + "MarkerDatabase WHERE id='" + id + "'");


            //MarkerDatabase markerDatabase = dbu.GetDB()
            //        .Table<MarkerDatabase>()
            //        .Where(x => x.id == id)
            //        .FirstOrDefault();

            if (markerDatabases.Count != 0 && markerDatabases[0] != null/*markerDatabase != null*/)
            {          
				dbu.UpdateLastAccessDate(markerDatabases[0]);
	            //System.Threading.Thread thread = new System.Threading.Thread(() => dbu.UpdateLastAccessDate(markerDatabase));
				//System.Threading.Thread thread = new System.Threading.Thread(() => dbu.UpdateLastAccessDate(markerDatabases[0]));
	            //thread.Start();
            }

            return /*markerDatabase*/ markerDatabases.Count != 0? markerDatabases[0] : null;
		}

		public void SaveMarkerDatabase(MarkerDatabase markerDatabase, bool replace)
		{
    		if (markerDatabase == null)
    			return;

			//Debug.Log("SaveMarkerDatabase: " + markerDatabase.ToString());
			
			if (replace)
				dbu.InsertOrReplace<MarkerDatabase>(markerDatabase);
			else
				dbu.Insert<MarkerDatabase>(markerDatabase);
			
			dbu.UpdateLastAccessDate(markerDatabase);
		}

		public void DeleteMarkerDatabase(string id){
			dbu.Delete<MarkerDatabase>(id);
		}

        #endregion

        public void DeleteOldestMarkers(int markerToDeleteCount, String markerToSaveId,
                                    bool isRecognitionRunning)
        {
            List<Marker> markers = dbu.MarkerQuery("SELECT markerId FROM " +
                "Markers WHERE markerId<>'" + markerToSaveId + "' AND " + 
                "databaseId IN (SELECT id FROM MarkerDatabase " +
                " WHERE cloud=1) ORDER BY lastAccessDate DESC LIMIT " + markerToDeleteCount);

            foreach(Marker marker in markers)
            {
                dbu.Delete<Marker>(marker.markerId);
                if (isRecognitionRunning)
                {
#if UNITY_EDITOR_OSX || (UNITY_IOS && !UNITY_EDITOR_WIN)

                    PikkartARCore.DeleteLocalMarker(marker.markerId,true);
#else
                    PikkartARCore.DeleteLocalMarker(new StringBuilder(marker.markerId),true);
#endif
                }
                else
                    File.Delete(StorageUtils.GetApplicationPersistentDataPath() + "markers/" + marker.markerId + ".dat");
            }
        }

        public int GetAllDbMarkersCount()
        {
            return dbu.GetMarkers().Count;
        }

        public List<Marker> GetMarkersList()
        {
            return dbu.GetMarkers();
        }
        
        public List<Marker> GetCloudMarkersList()
        {
            return dbu.MarkerQuery("SELECT markerId, updateDate, lastAccessDate from Markers" +
                " WHERE databaseId IN (SELECT id FROM MarkerDatabase" +
                " WHERE cloud=1)");
        }
    }
}