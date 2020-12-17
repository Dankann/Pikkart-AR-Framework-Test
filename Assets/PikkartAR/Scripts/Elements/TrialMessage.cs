using UnityEngine;
using System.Collections;

public class TrialMessage : MonoBehaviour {

    void Awake()
    {}

    public void OnClickExit()
    {
        MonoBehaviour.FindObjectOfType<TrialMessage>().gameObject.SetActive(false);
    }

}