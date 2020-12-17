using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using PikkartAR;

public class CloudMarkerObject : MonoBehaviour {

    [SerializeField]
    public string cloudMarkerId;
    [SerializeField]
    public int cloudMarkerARLogoPattern = -1;
    [SerializeField]
    public UnityEvent OnCloudMarkerFoundEvents;
    [SerializeField]
    public UnityEvent OnCloudMarkerLostEvents;
    [SerializeField]
    public UnityEvent OnCloudMarkerPatternCodeFoundEvents;

    public virtual void Init()
    {
        transform.position = Vector3.zero;
        EnableWithChildren(false);
    }

    public virtual void OnCloudMarkerFound()
    {
        Debug.Log("CloudMarkerObject.OnCloudMarkerFound() id: "+ cloudMarkerId);
        EnableWithChildren(true);
        OnCloudMarkerFoundEvents.Invoke();
    }

    public virtual void OnCloudMarkerLost()
    {
        EnableWithChildren(false);
        OnCloudMarkerLostEvents.Invoke();
    }

    public void ARLogoFound(int payload)
    {
        //Debug.Log("ARLogoFound");
        OnCloudMarkerPatternCodeFoundEvents.Invoke();
    }

    protected void EnableWithChildren(bool setting)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(setting);
        }
            
    }
}
