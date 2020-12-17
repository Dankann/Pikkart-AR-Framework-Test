using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Marker object.
/// Handles activation/deactivation of a specific marker 3d scenes.
/// </summary>
public class MarkerObject : MonoBehaviour {

    [SerializeField]
    public UnityEvent OnMarkerFoundEvents;
    [SerializeField]
    public UnityEvent OnMarkerLostEvents;
    [SerializeField]
    public UnityEvent OnMarkerPatternCodeFoundEvents;

    public int markerARLogoPattern = -1;
    public string markerId = "";
	public int markerIndex = -1;
    public float x = 0, y = 0;

	/// <summary>
	/// Shows the marker scene. Called when a specific marker is found.
	/// If the marker id matches the id of this object all children gameobject are activated.
	/// </summary>
	/// <param name="foundMarkerId">Found marker identifier.</param>
	public virtual void OnMarkerFound()
    {
		EnableWithChildren(true);
        OnMarkerFoundEvents.Invoke();
    }

	/// <summary>
	/// Hides the marker scene. Used to hide all augmented objects children of this marker.
	/// </summary>
	/// <param name="lostMarkerId">Lost marker identifier.</param>
	public virtual void OnMarkerLost()
    {
        EnableWithChildren(false);
        OnMarkerLostEvents.Invoke();
	}

    public void ARLogoFound(int payload)
    {
        //TODO
        //Debug.Log("ARLogoFound");
        OnMarkerPatternCodeFoundEvents.Invoke();
        gameObject.BroadcastMessage("MarkerPayloadFound", payload, SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Init this instance.
    /// </summary>
    public virtual void Init() {
        transform.position = Vector3.zero;
        /*MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            if (!meshRenderer.CompareTag("MarkerImage"))
            {
                meshRenderer.transform.position = Vector3.zero;
            //else
                //meshRenderer.transform.localPosition = new Vector3(x, 0, y);
            }
        }*/
        EnableWithChildren(false);
	}

    /// <summary>
    /// Enables/disables (activates game object) the childrens.
    /// </summary>
    /// <param name="setting">Activation setting.</param>
    protected void EnableWithChildren(bool setting) {
        for (int i = 0; i < transform.childCount; i++)
			transform.GetChild(i).gameObject.SetActive(setting);
    }
}
