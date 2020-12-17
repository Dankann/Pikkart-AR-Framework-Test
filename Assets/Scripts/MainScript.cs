using UnityEngine;
using PikkartAR;

/*
 *  Estensione del PikkartMain per fornire all'utente un esempio pronto all'uso 
 *
 *  Chiama internamente lo Start() del PikkartMain per inizializzare la camera, etc
 *  e lo StartRecognition (PikkartMain non lo fa)
 *  Implementa anche i metodi dell'IRecognitionListener
 */

public class MainScript : PikkartMain, IRecognitionListener
{
    private new void Start()
    {
        base.Start();
        SetCloudMarkerObjectListener(this);
        SetRecognitionListener(this);
        
    }

    public new void OnPikkartARStartupComplete()
    {
        StartRecognition(_recognitionOptions, this);
    }

    /*void Update()
    {
        for (int i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Ended)
            {
                TapToScan();
            }
        }
    }*/

    /*
     * Metodi Listener
     */
    public void ExecutingCloudSearch() {

    }
    public void CloudMarkerNotFound() {

    }

    public bool IsConnectionAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public void InternetConnectionNeeded() {

    }


    public void MarkerFound(MarkerInfo marker)
    {
        //Debug.Log("Marker " + marker.getId() + " found");
    }

    public void MarkerTrackingLost(string markerId)
    {
        Debug.Log("Marker " + markerId + " lost");
    }

    public void MarkerNotFound()
    {
        Debug.Log("No marker found");
    }

    public void ARLogoFound(MarkerInfo marker, int payload)
    {
        Debug.Log("ARLogo found " + payload);
    }
}