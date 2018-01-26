using Assets.Wulfram3.Scripts.HUD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This will only allow key commands to be registered if ChatManager.isChatOpen is false.
/// </summary>
public class InputEx 
{
    public static float GetAxisRaw(string axisRaw)
    {
        if(!ChatManager.isChatOpen)
        {
            return UnityEngine.Input.GetAxisRaw(axisRaw);
        }

        return 0f;
    }

    public static float GetAxis(string axisRaw)
    {
        if (!ChatManager.isChatOpen)
        {
            return UnityEngine.Input.GetAxis(axisRaw);
        }

        return 0f;
    }

    public static bool GetButton(string buttonName)
    {
        if (!ChatManager.isChatOpen)
        {
            return UnityEngine.Input.GetButton(buttonName);
        }

        return false;
    }

    public static bool GetButtonDown(string buttonName)
    {
        if (!ChatManager.isChatOpen)
        {
            return UnityEngine.Input.GetButtonDown(buttonName);
        }

        return false;
    }

    public static bool GetButtonUp(string buttonName)
    {
        if (!ChatManager.isChatOpen)
        {
            return UnityEngine.Input.GetButtonUp(buttonName);
        }

        return false;
    }

    public static bool GetKey(string name)
    {
        if (!ChatManager.isChatOpen)
        {
            return UnityEngine.Input.GetKey(name);
        }

        return false;
    }

    public static bool GetKeyDown(string name)
    {
        if (!ChatManager.isChatOpen)
        {
            return UnityEngine.Input.GetKeyDown(name);
        }

        return false;
    }

    public static bool GetKeyUp(string name)
    {
        if (!ChatManager.isChatOpen)
        {
            return UnityEngine.Input.GetKeyUp(name);
        }

        return false;
    }

    public static bool GetMouseButton(int button)
    {
        if (!ChatManager.isChatOpen)
        {
            return UnityEngine.Input.GetMouseButton(button);
        }

        return false;
    }

    public static bool GetMouseButtonDown(int button)
    {
        if (!ChatManager.isChatOpen)
        {
            return UnityEngine.Input.GetMouseButtonDown(button);
        }

        return false;
    }

    public static bool GetMouseButtonUp(int button)
    {
        if (!ChatManager.isChatOpen)
        {
            return UnityEngine.Input.GetMouseButtonUp(button);
        }

        return false;
    }
}
