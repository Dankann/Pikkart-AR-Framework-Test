using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PikkartAR;

public class TouchInput : MonoBehaviour {

	public LayerMask touchInputMask;
	private List<GameObject> touchList = new List<GameObject> ();
	private GameObject[] touchesOld;
	private RaycastHit hit;
	//private Vector3 oldHitPosition;
	private static readonly float rayCastDistance = 500f;
	//private bool controlsActive = true;
	//private static Vector3 defaultPosition = new Vector3 (-1, -1, -1);
	
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {		
		
		#if UNITY_EDITOR
		
		if( Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0) ) {
			
			touchesOld = new GameObject[touchList.Count];
			touchList.CopyTo(touchesOld);
			touchList.Clear();
			
			Vector3 position = Input.mousePosition;
			
			Ray ray = Camera.main.ScreenPointToRay(position);
			
			if(Physics.Raycast(ray, out hit, rayCastDistance, touchInputMask)) {
				
				Debug.DrawLine (ray.origin, hit.point);
				
				GameObject recipient = hit.transform.gameObject;
				touchList.Add(recipient);
				
				if(Input.GetMouseButtonDown(0)) {
					recipient.SendMessage("OnTouchDown", hit.point, SendMessageOptions.DontRequireReceiver);
				}
				if(Input.GetMouseButtonUp(0)) {
					recipient.SendMessage("OnTouchUp", hit.point, SendMessageOptions.DontRequireReceiver);
				}
				if(Input.GetMouseButton(0)) {
					recipient.SendMessage("OnTouchStay", hit.point, SendMessageOptions.DontRequireReceiver);
				}
			}
			
//			if(Input.GetMouseButtonDown(0)) {
//				oldHitPosition = position;
//			}
//			if(Input.GetMouseButtonUp(0)) {
//				oldHitPosition = defaultPosition;
//			}
			
			foreach(GameObject g in touchesOld) {
				if(g != null)
				if(!touchList.Contains(g)) {
					g.SendMessage("OnTouchExit", hit.point, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		
		#endif
		
		if (Input.touchCount > 0) {

            //FindObjectOfType<PikkartMain>().TapToScan();

            touchesOld = new GameObject[touchList.Count];
			touchList.CopyTo (touchesOld);
			touchList.Clear ();
			
			foreach (Touch touch in Input.touches) {
				Ray ray = Camera.main.ScreenPointToRay (touch.position);

                //Debug.DrawLine (ray.origin, hit.point);
                //Debug.Log("Touch Input");
				
				if (Physics.Raycast (ray, out hit, rayCastDistance, touchInputMask)) {
					
					GameObject recipient = hit.transform.gameObject;
					touchList.Add (recipient);
										
					if (touch.phase == TouchPhase.Began) {
						// SendMessageOptions.DontRequireReceiver se il recipient non esiste non succede nulla
						recipient.SendMessage ("OnTouchDown", hit.point, SendMessageOptions.DontRequireReceiver);
					}
					if (touch.phase == TouchPhase.Ended) {
						recipient.SendMessage ("OnTouchUp", hit.point, SendMessageOptions.DontRequireReceiver);
					}
					if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved) {
						recipient.SendMessage ("OnTouchStay", hit.point, SendMessageOptions.DontRequireReceiver);
					}
					if (touch.phase == TouchPhase.Canceled) {
						// succede, ad esempio, quando ci sono troppi touch
						recipient.SendMessage ("OnTouchExit", hit.point, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
			
//			if (Input.touchCount == 1) {
//				Touch touchZero = Input.GetTouch (0);
//				if (touchZero.phase == TouchPhase.Moved) {
//					
//					yDiff -= touchZero.deltaPosition.y * yRotationSpeed;
//					xDiff += touchZero.deltaPosition.x * xRotationSpeed;
//					yDiff = Mathf.Clamp (yDiff, 0, 80);
//					Quaternion rotation = Quaternion.Euler (yDiff, xDiff, 0);
//					Vector3 newPosition;
//					if (mainObject == null) {
//						newPosition = rotation * new Vector3 (0f, 0f, -cameraDistance) + new Vector3 (0f, 0f, 0f);
//					} else {
//						newPosition = rotation * new Vector3 (0f, 0f, -cameraDistance) + cameraFocus; // mainObject.position fissa il centro in cui guarda la camera
//					}
//					transform.rotation = rotation;
//					transform.position = newPosition;
//				}
//				
//				cameraFocusTemp = cameraFocus;
//			}
			
			foreach (GameObject g in touchesOld) {
				if (!touchList.Contains (g)) {
					g.SendMessage ("OnTouchExit", hit.point, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		
	}

}
