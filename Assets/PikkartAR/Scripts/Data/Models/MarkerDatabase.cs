using SQLite4Unity3d;
using System;

/*
 *  Oggetto MarkerDatabase salvato nel database
 */

namespace PikkartAR
{
	/// <summary>
	/// Marker database dto for database interactions.
	/// </summary>
	public class MarkerDatabase {

		[PrimaryKey]
		public string id { get; set; }
		public string code { get; set; }
		public string customData { get; set; }
        public bool cloud { get; set; }
		public DateTime updateDate { get; set; }
		public DateTime downloadDate { get; set; }
		public DateTime lastAccessDate { get; set; }
		
		public MarkerDatabase() { }
		
		public MarkerDatabase(string id, string code, string customData, bool cloud, DateTime updateDate, DateTime downloadDate) {
			this.id = id;
			this.code = code;
            this.cloud = cloud;
			this.customData = customData;
			this.updateDate = updateDate;
			this.downloadDate = downloadDate;
		}

		public override string ToString () {
			return string.Format ("[MarkerDatabase: id={0}, code={1}, customData={2}, cloud={3}, " +
                "updateDate={4}, downloadDate={5}, lastAccessDate={6}]", 
                id, code, customData, cloud, updateDate, downloadDate, lastAccessDate);
		}
	}
}