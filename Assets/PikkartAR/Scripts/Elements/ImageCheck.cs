using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 *  Il quadratino rosso/verde che c'era prima all'angolo 
 */
namespace PikkartAR {

	public class ImageCheck : MonoBehaviour {
		
		public Color[] colors;
		
		public void SetColor(bool set){
			if(set)
				GetComponent<Image> ().color = colors [1];
			else
				GetComponent<Image> ().color = colors [0];
		}
		
		public void SetActive (bool set) {
			GetComponent<Image> ().gameObject.SetActive (set);
		}
	}

}