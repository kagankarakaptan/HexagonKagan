using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexPhysics : MonoBehaviour
{
    public MasterMind master;

    public float gravity;
    public float verticalVelocity;
    public Vector2 aim;

    private bool key;

    private void Start()
    {
        master = GameObject.Find("Master").GetComponent<MasterMind>();
        key = false;
    }

    void Update()
    {
        if (master.canFall)
        {


            RaycastHit2D[] infos = Physics2D.RaycastAll(transform.position, Vector2.down);

            if (infos.Length > 1)
                aim = infos[1].collider.gameObject.transform.position + new Vector3(0, master.hexSize * Mathf.Sqrt(3) / 2f, 0);
            else
                aim = transform.position;

            Vector2 recentPos = transform.position; //keeps the position in the previous frame 
            //applying movement
            if (((Vector2)transform.position - aim).magnitude > master.hexSize / 4f)
            {
                transform.position -= new Vector3(0, -gravity * Time.deltaTime, 0);
            }
            else
            {
                //attaching to the aim
                transform.position = aim;
            }

            ////applying movement
            //if (transform.position.y > aim.y + master.hexSize)
            //{
            //    transform.position -= new Vector3(0, -gravity * Time.deltaTime, 0);
            //}
            //else
            //{
            //    //attaching to the aim
            //    transform.position = aim;
            //}

            //ground detection
            verticalVelocity = -((Vector2)transform.position - recentPos).magnitude;

            if (verticalVelocity == 0)
                Speak();
            else
                Steal();

        }

    }

    public void Speak()
    {
        if (key)
            master.groundedCount++;
        key = false;
    }

    public void Steal()
    {
        if (!key)
            master.groundedCount--;
        key = true;

    }

}
