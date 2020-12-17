using UnityEngine;

namespace PikkartAR {

	public class ScreenData {

		private DeviceOrientation lastOrientation = DeviceOrientation.Unknown;
		public Vector2 resolution { get; set; }
		public DeviceOrientation orientation { get; set; }

		public ScreenData () {
			resolution = new Vector2 (Screen.width, Screen.height);
			orientation = Input.deviceOrientation;
		}

		public bool HasChanged () {
			if (orientation != Input.deviceOrientation) {
				//Debug.Log ("ScreenData HasChanged orientation");
				return true;
			}
			return false;
		}

		public void Update () {
			//Debug.Log ("ScreenData Update");
			lastOrientation = orientation;
			resolution = new Vector2 (Screen.width, Screen.height);
			orientation = Input.deviceOrientation;
		}

		public bool IsOrientationLansdcape () {
			if (orientation == DeviceOrientation.LandscapeLeft || orientation == DeviceOrientation.LandscapeRight) {
				return true;
			}
			return false;
		}

		public bool IsResolutionLandscape () {
			return ScreenUtilities.IsLandscapeResolution (resolution);
		}

		public override string ToString () {
			return resolution.ToString () + " " + orientation.ToString ();
		}

		public DeviceOrientation GetLastOrientation () {
			return lastOrientation;
		}
	}
}
