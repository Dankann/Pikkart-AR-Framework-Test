using UnityEngine;
using System.Collections;

namespace PikkartAR {

	public class ScreenUtilities {

		public static float GetScreenInches () {
#if UNITY_ANDROID && !UNITY_EDITOR
            float x = Mathf.Pow (DisplayMetricsAndroid.WidthPixels/DisplayMetricsAndroid.XDPI, 2);
			float y = Mathf.Pow (DisplayMetricsAndroid.HeightPixels/DisplayMetricsAndroid.YDPI, 2);
			float inches = Mathf.Sqrt (x+y);
			return inches;
#else
			return Mathf.Sqrt((Screen.width * Screen.width) + (Screen.height * Screen.height))/Screen.dpi;
#endif
		}

		public static int GetLongSide () {
			if (Screen.width >= Screen.height) {
				return Screen.width;
			}
			return Screen.height;
		}

		public static int GetShortSide () {
			if (Screen.width > Screen.height) {
				return Screen.height;
			}
			return Screen.width;
		}

		public static float GetPortraitAspectRatio () {
			return (float)GetShortSide ()/(float)GetLongSide ();
		}

		public static float GetLandscapeAspectRatio () {
			return (float)GetLongSide ()/(float)GetShortSide ();
		}

		public static bool IsLandscapeResolution () {
			if (Screen.width > Screen.height) {
				return true;
			}
			return false;
		}

		public static bool IsLandscapeResolution (Vector2 resolution) {
			if (resolution.x > resolution.y) {
				return true;
			}
			return false;
		}
	}
}