using UnityEngine;
using PikkartAR;

/*
 *  Classe che mappa la camera dell'editor, ovvero la vista sulla scena Unity
 *  e la disattiva quando si attiva la webcam dal pc.
 */

public class UICamera : MonoBehaviour
{
    void Awake()
    {
        if (FindObjectOfType<PikkartMain>() != null && !FindObjectOfType<PikkartMain>().useWebcamOnEditor)
            GetComponent<Camera>().enabled = false;
    }
}
