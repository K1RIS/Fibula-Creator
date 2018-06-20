using UnityEngine;
using System;

public class SavePrompt : MonoBehaviour
{
    public Action action;

    private void Awake()
    {
        CameraController.canMove = false;
    }

    public void confirm()
    {
        action();
        cancel();
    }

    public void cancel()
    {
        CameraController.canMove = true;
        Destroy(gameObject);        
    }
}
