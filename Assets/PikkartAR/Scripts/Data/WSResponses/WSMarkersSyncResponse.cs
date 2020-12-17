using System.Collections.Generic;

/*
 *  Mappa il JSON della risposta a una SyncLocalMarkers
 */

namespace PikkartAR {
	/// <summary>
	/// Web service Markers Sync response dto.
	/// </summary>
	public class WSMarkersSyncResponse : WSResponse {

		public class WSMarkersSync
		{
			public List<string> toDelete { get; set; }
			public List<string> toDownload { get; set; }
			public List<string> toUpdate { get; set; }
		}

		public WSMarkersSync data;
	}
}