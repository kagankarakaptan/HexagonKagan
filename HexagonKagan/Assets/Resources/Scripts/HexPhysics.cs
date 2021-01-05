using UnityEngine;

public class HexPhysics : MonoBehaviour
{
    public MasterMind master; //master object of the game
    public GameObject mother; //holder of the selected group of hexes

    public float gravity; //vertical movement value to be added to the all hexes
    public float verticalVelocity; //current vertical movement speed of this object
    public Vector2 aim; //target position according the one below

    private bool key; //the switch parameter to the controll if this object can send the message to the master anytime

    public int clock; //the counter value of the bomb hexagon

    private void Start()
    {
        master = GameObject.Find("Master").GetComponent<MasterMind>();
        mother = GameObject.Find("Mother");

        key = false;
    }

    void Update()
    {
        if (mother.transform.childCount == 0)
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
                //transform.position = Vector3.Lerp(transform.position, aim, 0.05f);
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

    //Basic communication system between master for the ground checking problem

    //if isGrounded
    public void Speak()
    {
        if (key)
            master.groundedCount++;
        key = false;
    }

    //if isntGrounded
    public void Steal()
    {
        if (!key)
            master.groundedCount--;
        key = true;

    }

}
