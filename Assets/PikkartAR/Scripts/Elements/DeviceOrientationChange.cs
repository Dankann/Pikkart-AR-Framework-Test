using UnityEngine;
using System;
using System.Collections;

namespace PikkartAR {

	public class DeviceOrientationChange : MonoBehaviour {
		
		public static event Action<ScreenData> OnScreenDataChange;
		
		static bool isAlive = true;	// Keeps this script running
		static ScreenData screen;
		static bool isReady = false;
		
        public static bool forcedOrientationActive = false;
        public static ScreenOrientation forcedScreenOrientation = ScreenOrientation.Unknown;
		
		void Awake () {
			//Screen.autorotateToLandscapeLeft = false;
			//Screen.autorotateToLandscapeRight = false;
			//Screen.autorotateToPortrait = false;
			//Screen.autorotateToPortraitUpsideDown = false;

			//Screen.orientation = ScreenOrientation.AutoRotation;

			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToLandscapeRight = true;
			Screen.autorotateToPortrait = true;
			Screen.autorotateToPortraitUpsideDown = true;
			screen = new ScreenData ();
		}
		
		void Start () {			
			StartCoroutine(CheckForChange());
		}
		
		IEnumerator CheckForChange () {
			
			yield return new WaitForSeconds(Constants.SCREEN_ORIENTATION_CHECK_DELAY_TIME);
			
			while (isAlive) {
				
				if (screen.HasChanged ()) {
                    //Debug.Log ("Screen has changed");
                    UpdateScreenOrientation();
				}
				yield return new WaitForSeconds(Constants.SCREEN_ORIENTATION_CHECK_DELAY_TIME);
			}
		}

        private void UpdateScreenOrientation()
        {
            isReady = false;
            screen.Update();
            if (OnScreenDataChange != null) OnScreenDataChange(screen);
            isReady = true;
        }

		public static ScreenData GetUpdatedDeviceOrientation() {
			ScreenData device_orientation = new ScreenData ();
			device_orientation.Update ();

			return device_orientation;
		}
		
		public static DeviceOrientation GetDeviceOrientation () {
			
            if (forcedOrientationActive)
            {
                switch (forcedScreenOrientation)
                {
                    case ScreenOrientation.LandscapeLeft:
                        return DeviceOrientation.LandscapeLeft;
                    case ScreenOrientation.LandscapeRight:
                        return DeviceOrientation.LandscapeRight;
                    case ScreenOrientation.PortraitUpsideDown:
                        return DeviceOrientation.PortraitUpsideDown;
                    default:
                        return DeviceOrientation.Portrait;
                }
            } else
            {
                switch (screen.orientation)
                {
			case DeviceOrientation.LandscapeLeft:
			case DeviceOrientation.LandscapeRight:
			case DeviceOrientation.Portrait:
			case DeviceOrientation.PortraitUpsideDown:
				return screen.orientation;
			default:
                        if (screen.IsResolutionLandscape())
                        {
                            if (screen.GetLastOrientation() == DeviceOrientation.LandscapeRight)
                            {
						return DeviceOrientation.LandscapeRight;
                            }
                            else
                            {
						return DeviceOrientation.LandscapeLeft;
					}
                        }
                        else
                        {
                            if (screen.GetLastOrientation() == DeviceOrientation.PortraitUpsideDown)
                            {
						return DeviceOrientation.PortraitUpsideDown;
                            }
                            else
                            {
						return DeviceOrientation.Portrait;
					}
				}
			}
		}
		}
		
		public static bool IsReady () {
			
			return isReady;
		}
		
		void OnDestroy () {
			
			isAlive = false;
		}

        public static void SetForcedOrientation(ScreenOrientation orientation)
        {
            forcedOrientationActive = true;
            forcedScreenOrientation = orientation;
            if (FindObjectOfType<DeviceOrientationChange>() != null)
                FindObjectOfType<DeviceOrientationChange>().UpdateScreenOrientation();
        }

        public static void DisableForcedOrientation()
        {
            forcedOrientationActive = false;
            forcedScreenOrientation = ScreenOrientation.Unknown;
            if (FindObjectOfType<DeviceOrientationChange>() != null)
                FindObjectOfType<DeviceOrientationChange>().UpdateScreenOrientation();
        }
		
	}
}