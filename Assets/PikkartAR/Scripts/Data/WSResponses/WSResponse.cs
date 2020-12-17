/*
 *  Mappa il JSON della generica risposta
 */

namespace PikkartAR {

	/// <summary>
	/// Generic web service response dto.
	/// </summary>
	public class WSResponse {
		
		public class WSResult {
			public bool success;
			public int code;
			public string message;
		}
		
		public WSResult result;
	}
}