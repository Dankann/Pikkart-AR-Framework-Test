/*
 *  Mappa il JSON della risposta a una FindMarker o una GetMarker
 */

namespace PikkartAR {
	/// <summary>
	/// Web service Marker response dto.
	/// </summary>
	public class WSMarkerResponse : WSResponse {

		public class WSMarkerDatabase
		{
			public string customData { get; set; }
			public string id { get; set; }
			public string code { get; set; }
            public bool cloud { get; set; }
		}
		
		public class WSMarker
		{
			public bool cacheEnabled { get; set; }
			public string markerUpdateDate { get; set; }
			public string publishedTo { get; set; }
			public string markerId { get; set; }
			public string publishedFrom { get; set; }
			public WSMarkerDatabase markerDatabase { get; set; }
			public string markerDescriptor { get; set; }
			public string markerCustomData { get; set; }
            public bool arLogoEnabled { get; set; }
		}
		
		public WSMarker data;
	}
}