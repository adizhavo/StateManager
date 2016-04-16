using UnityEngine;
using System.Collections;

public class StateRequestContainer : MonoBehaviour
{
    [SerializeField] protected StateRequest StateRequest;

    public StateRequest GetStateRequest()
    {
        return StateRequest;
    }
}

[System.Serializable]
public struct StateRequest
{
    [SerializeField] private States State;

    public StateRequest(States State)
    {
        this.State = State;
    }

    public States GetStateRequested()
    {
        return State;
    }
}
