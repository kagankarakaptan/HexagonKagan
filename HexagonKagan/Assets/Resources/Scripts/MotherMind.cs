using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotherMind : MonoBehaviour
{
    public MasterMind master;
    public bool canCall;

    private void Awake()
    {
        master = GameObject.Find("Master").GetComponent<MasterMind>();
    }

    public void CallAction() => master.Action();

    public void Ending()
    {
        if (master.groundedCount == master.gridWidth * master.gridHeight)
            master.canPlay = true;
    }

}
