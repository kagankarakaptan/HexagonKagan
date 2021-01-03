using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterMind : MonoBehaviour
{
    public GameObject mother; //the center point and a parent for the rotation of the selected group of 3 hexes

    public GameObject hexPrefab; //prefab of the hex itself
    public GameObject hiddenPrefab; //invisible hex prefab for the bottom of the gridMap

    public Color[] colors; //possible colors array for the hexes
    public float hexSize; //keeps the size of the hexes with spacing value
    public float spacing; //spacing between hexes

    private Vector2[] rootMap; //1D matrix of the vector2 that keeps the collision points of the hexes each-other 

    public int gridWidth = 8; //size of the gridMap's width
    public int gridHeight = 9; //size of the gridMap's height

    private Vector2 mapOffset; //position of the grid's bottom left corner

    public bool canSpin; //keeps the game state (can we play or should we wait the movements of the environment ?)
    public bool canFall; //keeps the game state (can we play or should we wait the movements of the environment ?)
    public bool canSelect; //keeps the game state (can we play or should we wait the movements of the environment ?)

    public int groundedCount;

    private void Awake()
    {
        rootMap = new Vector2[2 * (gridWidth - 1) * (gridHeight - 1)];
        hexSize = hexPrefab.GetComponent<SpriteRenderer>().bounds.size.x + spacing;
        mapOffset = transform.position;

    }

    private void Start()
    {
        //creating the hexGrid
        BuildMap();

        //re-building the hexMap
        ReBuild();

        //setting the initial values when everything is ready
        canSpin = true;
        canFall = false;
        canSelect = true;
        groundedCount = gridHeight * gridWidth;


    }




    private void Update()
    {
        //selection hex group
        if (Input.GetMouseButtonDown(0) && canSelect)
        {
            Collider2D[] hexes = Physics2D.OverlapCircleAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), hexSize / 4f);

            //Debug.Log(hexes.Length);

            if (hexes.Length == 3)
            {
                if (mother.transform.childCount == 3)
                    Release();

                Collect(hexes);
            }
            //else
            //    Release();
        }


        //Spinning the gruop
        if (Input.GetKeyDown(KeyCode.Space) && mother.transform.childCount == 3 && canSpin)
            Spin(true);


        ////testing the possible movements
        //if (Input.GetKeyDown(KeyCode.T))
        //    Debug.Log(PossibleMovements());

    }

    //setting the selecting as a mother with children
    public void Collect(Collider2D[] hexes)
    {
        Vector2 centerOfMass = (hexes[0].transform.position + hexes[1].transform.position + hexes[2].transform.position) / 3;

        mother.transform.position = centerOfMass;
        mother.GetComponent<SpriteRenderer>().enabled = true;

        foreach (Collider2D hex in hexes)
            hex.transform.SetParent(mother.transform);
    }

    //detaching children from currentSelection
    public void Release()
    {
        mother.transform.DetachChildren();
        mother.GetComponent<SpriteRenderer>().enabled = false;
        mother.transform.rotation = Quaternion.Euler(0, 0, 0);

    }

    //spinning the hex goup
    public void Spin(bool direction)
    {
        canFall = false;
        canSelect = false;

        //setting the game phase to movement of hexes
        if (direction)
        {
            //rotating clockwise
            mother.GetComponent<Animator>().Play("clockwise");
        }

        else
        {
            //rotating anticlockwise
            mother.GetComponent<Animator>().Play("anticlockwise");
        }

    }

    //testing for neighborhood
    public List<GameObject> NeighborhoodTest()
    {
        List<GameObject> neighbors = new List<GameObject>();

        foreach (Vector2 root in rootMap)
        {
            Collider2D[] hexes = Physics2D.OverlapCircleAll(root, hexSize / 2f);

            if (hexes[0].GetComponent<SpriteRenderer>().color == hexes[1].GetComponent<SpriteRenderer>().color && hexes[0].GetComponent<SpriteRenderer>().color == hexes[2].GetComponent<SpriteRenderer>().color)
            {
                if (!neighbors.Contains(hexes[0].gameObject)) neighbors.Add(hexes[0].gameObject);
                if (!neighbors.Contains(hexes[1].gameObject)) neighbors.Add(hexes[1].gameObject);
                if (!neighbors.Contains(hexes[2].gameObject)) neighbors.Add(hexes[2].gameObject);
            }

        }
        return neighbors;
    }

    //testing killing victims
    public void Action()
    {
        if (!canFall)
        {
            List<GameObject> victims = NeighborhoodTest();

            if (victims.Count != 0)
            {
                mother.GetComponent<Animator>().Play("Idle");

                foreach (GameObject victim in victims)
                {
                    victim.transform.position += new Vector3(0, 15f, 0);
                    victim.GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colors.Length)];

                }


                Release();
                canFall = true;
                //StartCoroutine(WaitAndAction());
            }


            victims.Clear();
        }


    }

    public int PossibleMovements()
    {
        int posibilities = 0;
        float radius = hexSize / 2f;

        foreach (Vector2 root in rootMap)
        {
            Collider2D[] hexes = Physics2D.OverlapCircleAll(root, radius);
            Collider2D[] others;

            if (hexes[0].GetComponent<SpriteRenderer>().color == hexes[1].GetComponent<SpriteRenderer>().color)
            {
                others = Physics2D.OverlapCircleAll(hexes[2].transform.position, radius);

                for (int i = 0; i < others.Length; i++)
                    if (others[i] != hexes[0] && others[i] != hexes[1] && others[i].GetComponent<SpriteRenderer>().color == hexes[0].GetComponent<SpriteRenderer>().color) posibilities++;
            }
            else if (hexes[0].GetComponent<SpriteRenderer>().color == hexes[2].GetComponent<SpriteRenderer>().color)
            {
                others = Physics2D.OverlapCircleAll(hexes[1].transform.position, radius);

                for (int i = 0; i < others.Length; i++)
                    if (others[i] != hexes[0] && others[i] != hexes[2] && others[i].GetComponent<SpriteRenderer>().color == hexes[0].GetComponent<SpriteRenderer>().color) posibilities++;
            }
            else if (hexes[1].GetComponent<SpriteRenderer>().color == hexes[2].GetComponent<SpriteRenderer>().color)
            {
                others = Physics2D.OverlapCircleAll(hexes[0].transform.position, radius);

                for (int i = 0; i < others.Length; i++)
                    if (others[i] != hexes[1] && others[i] != hexes[2] && others[i].GetComponent<SpriteRenderer>().color == hexes[1].GetComponent<SpriteRenderer>().color) posibilities++;
            }
        }

        return posibilities;
    }

    public void BuildMap()
    {
        int rootIndex = 0;
        //creating maps
        for (int row = 0; row < gridHeight; row++) //row===y
            for (int col = 0; col < gridWidth; col++) //col===x
            {

                //creating tiles
                float worldPosX = col * hexSize * 3f / 4f;
                float worldPosY = row * hexSize * Mathf.Sqrt(3) / 2f + hexSize * Mathf.Sqrt(3) / 4f * (col % 2);

                Vector2 worldPos = new Vector2(worldPosX, worldPosY) + mapOffset;

                if (row < gridHeight - 1)
                {
                    //left and right
                    if (col > 0 && col < gridWidth - 1)
                    {
                        //left
                        rootMap[rootIndex++] = worldPos + new Vector2(-hexSize / 4f, hexSize * Mathf.Sqrt(3) / 4f);
                        //right
                        rootMap[rootIndex++] = worldPos + new Vector2(+hexSize / 4f, hexSize * Mathf.Sqrt(3) / 4f);
                    }
                    else if (col == gridWidth - 1)
                        //left
                        rootMap[rootIndex++] = worldPos + new Vector2(-hexSize / 4f, hexSize * Mathf.Sqrt(3) / 4f);
                    else if (col == 0)
                        //right
                        rootMap[rootIndex++] = worldPos + new Vector2(+hexSize / 4f, hexSize * Mathf.Sqrt(3) / 4f);

                }

                //creating hidding path for ground
                if (row == 0)
                    Instantiate(hiddenPrefab, worldPos - new Vector2(0, hexSize * Mathf.Sqrt(3) / 2f), Quaternion.identity).name = "hiddenHex";


                //creating hexes
                Instantiate(hexPrefab, worldPos, Quaternion.identity).GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colors.Length)];

            }

    }

    public void ReBuild()
    {
        List<GameObject> neighbors = NeighborhoodTest();

        if (neighbors.Count != 0)
        {
            foreach (GameObject victim in neighbors)
                victim.GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colors.Length)];

            neighbors.Clear();
            ReBuild();
        }

        neighbors.Clear();





        //int similarity;
        //do
        //{
        //    similarity = 0;
        //    List<GameObject> neighbors = new List<GameObject>();

        //    foreach (Vector2 root in rootMap)
        //    {
        //        Collider2D[] hexes = Physics2D.OverlapCircleAll(root, hexSize / 2f);

        //        if (hexes[0].GetComponent<SpriteRenderer>().color == hexes[1].GetComponent<SpriteRenderer>().color && hexes[0].GetComponent<SpriteRenderer>().color == hexes[2].GetComponent<SpriteRenderer>().color)
        //        {
        //            similarity++;

        //            if (!neighbors.Contains(hexes[0].gameObject)) neighbors.Add(hexes[0].gameObject);
        //            if (!neighbors.Contains(hexes[1].gameObject)) neighbors.Add(hexes[1].gameObject);
        //            if (!neighbors.Contains(hexes[2].gameObject)) neighbors.Add(hexes[2].gameObject);
        //        }


        //    }

        //    foreach (GameObject victim in neighbors)
        //        victim.GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colors.Length)];

        //    neighbors.Clear();

        //} while (similarity > 0);
    }

    private IEnumerator WaitAndAction()
    {
        yield return null;
        //Debug.Log("falling started");
        //Debug.Break();


        while (groundedCount != gridHeight * gridWidth)
            yield return null;

        //Debug.Log("everything settled");
        canFall = false;
        Action();




    }


}
