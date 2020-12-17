using UnityEngine;
using UnityEngine.Events;

public class TouchInputReceiver : MonoBehaviour
{
    bool touching = false;
    bool longTouchTriggered = false;
    float touchTime = 0f;
    //float lastTouchDownTime = 0f;

    public UnityEvent OnClickEvents;

    //public TouchMonoBehaviour controller;
    //public string mName;

    void Start() {}

    void OnTouchDown() {
        //Debug.Log("OnTouchDown");
        if (touching)
            touchTime = 0;
        //else 
        //    if ((Time.time - lastTouchDownTime) <= 0.25)
        //        controller.DoubleTap(mName);
        touching = true;
        //lastTouchDownTime = Time.time;
    }

    void OnTouchUp() {
        //Debug.Log("OnTouchUp");
        if (touchTime <= 0.2)
        {
            //Debug.Log("OnTouchUpInvoke");
            OnClickEvents.Invoke();
            //controller.Tap(mName);
        }
        touching = false;
        touchTime = 0;
        longTouchTriggered = false;
    }

    void OnTouchStay()
    {
        if (touching)
            touchTime += Time.deltaTime;

        if (!longTouchTriggered && touchTime >= 1)
        {
            //controller.LongTouch(mName);
            longTouchTriggered = true;
        }
    }

    void OnTouchExit() {

    }
}
