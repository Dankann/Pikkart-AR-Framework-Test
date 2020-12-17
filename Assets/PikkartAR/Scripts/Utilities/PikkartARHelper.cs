using UnityEngine;
using System.Collections;

namespace PikkartAR {

	public class PikkartARHelper : MonoBehaviour {

		static PikkartARHelper _instance;

		public static PikkartARHelper Instance
		{
			get
			{
				if ( _instance == null) {
					_instance = (new GameObject("PikkartARHelperContainer")).AddComponent<PikkartARHelper>();
				}
				return _instance;
			}
		}

		public void LaunchCoroutine (IEnumerator routine)
		{
			if(routine!=null) StartCoroutine (routine);
		}

		public void PrintStatus ()
		{
			Debug.Log ("PikkartARHelper Status Check");
		}
	}
}
