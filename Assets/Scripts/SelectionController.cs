using UnityEngine;
using UnityEngine.Events;
using Lean.Touch;
using System;

[Serializable]
public class LeanSelectedEvent : UnityEvent<LeanFinger> { }
[Serializable]
public class LeanDeselectedEvent : UnityEvent { }

public class SelectionController : MonoBehaviour
{
    public LeanSelectedEvent leanSelectedEvent { get; private set; }
    public LeanDeselectedEvent leanDeselectedEvent { get; private set; }

    void Awake()
    {
        leanSelectedEvent = new LeanSelectedEvent();
        leanDeselectedEvent = new LeanDeselectedEvent();
    }

    public void OnSelect(LeanFinger finger)
    {
        leanSelectedEvent.Invoke(finger);
    }

    public void OnDeselect()
    {
        leanDeselectedEvent.Invoke();
    }
}
