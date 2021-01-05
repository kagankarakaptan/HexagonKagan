using UnityEngine;

public class MotherMind : MonoBehaviour
{
    public MasterMind master;
    public bool canCall;

    private void Awake()
    {
        master = GameObject.Find("Master").GetComponent<MasterMind>();
    }

    //Action calling that can call inside the both spining animations
    public void CallAction() => master.Action();

    //ending function that can call at the last frame of the both spinning animations
    public void Ending()
    {
        if (master.groundedCount == master.gridWidth * master.gridHeight)
            master.canPlay = true;
    }

}
