using UnityEngine;
using PikkartAR;

/*
 *  Classe che mappa le immagini dei marker nell'editor e le disattiva quando si attiva la webcam dal pc
 */

public class ReferenceImage : MonoBehaviour {

	void Awake ()
    {
#if UNITY_EDITOR
        if (FindObjectOfType<PikkartMain>() != null && FindObjectOfType<PikkartMain>().useWebcamOnEditor)
            GetComponent<MeshRenderer>().enabled = false;
#endif
    }

#if !UNITY_EDITOR
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
#endif
}
