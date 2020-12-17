using LitJson;

namespace PikkartAR {
	
	public class JsonUtilities {

		public static T ToObject<T> (string obj) {
			return JsonMapper.ToObject<T> (obj);
		}
	}
}
