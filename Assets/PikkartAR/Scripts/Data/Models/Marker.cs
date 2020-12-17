using SQLite4Unity3d;
using System;

/*
 *  Oggetto Marker salvato nel database
 */

namespace PikkartAR
{
    /// <summary>
    /// Marker dto for database interactions.
    /// </summary>
	[Table("Markers")]
    public class Marker {
		
        [PrimaryKey]
        public string markerId { get; set; }
        public string markerDescriptor { get; set; }
        public string customData { get; set; }
        public DateTime updateDate { get; set; }
        public DateTime downloadDate { get; set; }
        public DateTime lastAccessDate { get; set; }
        public DateTime? publishedFrom { get; set; }
        public DateTime? publishedTo { get; set; }
        public bool cacheEnabled { get; set; }
        public string databaseId { get; set; }
        public bool arLogoEnabled { get; set; }
        [Ignore]
        public MarkerDatabase markerDatabase { get; set; }

        public Marker() {}

		public Marker(string markerId,
			string markerDescriptor,
			string customData,
			DateTime updateDate,
			DateTime downloadDate,
			DateTime lastAccessDate,
            DateTime? publishedFrom,
            DateTime? publishedTo,
            bool cacheEnabled,
            bool arLogoEnabled,
            string databaseId){

			this.markerId = markerId;
			this.markerDescriptor = markerDescriptor;
			this.customData = customData;
			this.updateDate = updateDate;
			this.downloadDate = downloadDate;
			this.lastAccessDate = lastAccessDate;
            this.publishedFrom = publishedFrom;
            this.publishedTo = publishedTo;
            this.cacheEnabled = cacheEnabled;
			this.databaseId = databaseId;
            this.arLogoEnabled = arLogoEnabled;
		}

        public Marker(string markerId,
            string markerDescriptor,
            string customData,
            DateTime updateDate,
            DateTime downloadDate,
            DateTime lastAccessDate,
            DateTime? publishedFrom,
            DateTime? publishedTo,
            bool cacheEnabled,
            MarkerDatabase markerDatabase,
            bool arLogoEnabled)
        {

            this.markerId = markerId;
            this.markerDescriptor = markerDescriptor;
            this.customData = customData;
            this.updateDate = updateDate;
            this.downloadDate = downloadDate;
            this.lastAccessDate = lastAccessDate;
            this.publishedFrom = publishedFrom;
            this.publishedTo = publishedTo;
            this.cacheEnabled = cacheEnabled;
            this.databaseId = markerDatabase.id;
            this.markerDatabase = markerDatabase;
            this.arLogoEnabled = arLogoEnabled;
        }

        public bool IsObsolete()
        {
            if (!markerDatabase.cloud) return false;

            DateTime timeNow = DateTime.Now.ToUniversalTime();
            if (cacheEnabled && timeNow >= publishedFrom && timeNow <= publishedTo)
                return (timeNow - downloadDate).Milliseconds > 1000 * 60 * 60 * 24; // un giorno
            else
                return true;
        }

        public bool IsPublished()
        {
            if (!markerDatabase.cloud) return true;

            DateTime timeNow = DateTime.Now.ToUniversalTime();
            return (timeNow >= publishedFrom && timeNow <= publishedTo);
        }

        public override string ToString()
        {
            return string.Format("[Marker: id={0}, customData={1}, updateDate={2}, downloadDate={3}, lastAccessDate={4}, publishedFrom={5}, " +
                        "publishedTo={6}, cacheEnabled={7}, databaseId={8}, arLogoEnabled={9}", markerId, customData, updateDate, downloadDate, lastAccessDate,
                        publishedFrom, publishedTo, cacheEnabled, databaseId, arLogoEnabled);
        }
    }
}
