using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexPhysics : MonoBehaviour
{
    public float gravity;
    public float verticalVelocity;

    void Update()
    {
        RaycastHit2D info = Physics2D.Raycast(-transform.position, Vector2.down, 0.375f);

        Debug.DrawRay(transform.position, Vector2.down * 0.375f, Color.red);

        if (info)
            verticalVelocity = 0f;
        else
            verticalVelocity += gravity * Time.deltaTime * Time.deltaTime * 0.5f;

        transform.position += new Vector3(0, verticalVelocity, 0);
    }
}
