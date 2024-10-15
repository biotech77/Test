using System.Collections;
using System.Collections.Generic;
using RSG;
using UnityEngine;

public class GuiManager : MonoBehaviour
{
    public IPromise Initialize()
    {
        return Promise.Resolved();
    }

    public IPromise Reset()
    {
        return Promise.Resolved();
    }
}
