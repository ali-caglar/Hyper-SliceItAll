using System;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public static Action OnTap;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnTap?.Invoke();
        }
    }
}