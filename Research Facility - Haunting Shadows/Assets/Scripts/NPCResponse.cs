using UnityEngine;
using System;

public class NPCResponse : MonoBehaviour
{
    public Action<string> OnNPCResponse;

    public void RespondToPlayer()
    {
        string response = "Hi.";
        Debug.Log(response);
        OnNPCResponse?.Invoke(response);
    }
    public void RespondToPlayer1()
    {
        string response = "My name is Jake.";
        Debug.Log(response);
        OnNPCResponse?.Invoke(response);
    }
    public void RespondToPlayer2()
    {
        string response = "You should look for some keys, I am sure they are here somewhere. Once you collect them all you can escape through the door.";
        Debug.Log(response);
        OnNPCResponse?.Invoke(response);
    }
    public void RespondToPlayer3()
    {
        string response = "I cannot do much for you, try to lock him inside one of the garages.";
        Debug.Log(response);
        OnNPCResponse?.Invoke(response);
    }
    public void RespondToPlayer4()
    {
        string response = "This is my only key, I was looking to escape too but you can take it, I am stuck here forever anyways.";
        Debug.Log(response);
        OnNPCResponse?.Invoke(response);
    }
    public void RespondToPlayer5()
    {
        string response = "This was once a research facility, they were trying to create supernatural beings. Once the entity escaped everyone fled, and now it inhabits the forest looking for prey. Rumours are they are using this entire area to experiment on it.";
        Debug.Log(response);
        OnNPCResponse?.Invoke(response);
    }
    public void RespondToPlayer6()
    {
        string response = "Thanks, you too that skincare routine has been paying off.";
        Debug.Log(response);
        OnNPCResponse?.Invoke(response);
    }
}
