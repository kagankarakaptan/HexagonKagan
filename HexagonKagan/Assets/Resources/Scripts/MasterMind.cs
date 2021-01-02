using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterMind : MonoBehaviour
{
    public GameObject mother;
    public GameObject tester;

    public GameObject hexPrefab;
    public Color[] colors;
    private float hexSize;
    public float spacing;

    private Vector2[] rootMap;

    public int gridWidth = 8;
    public int gridHeight = 9;

    private Vector2 mapOffset;


    private void Awake()
    {
        rootMap = new Vector2[2 * (gridWidth - 1) * (gridHeight - 1)];
        hexSize = hexPrefab.GetComponent<SpriteRenderer>().bounds.size.x + spacing;
        mapOffset = transform.position;

    }

    private void Start()
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

                //creating hexes
                Instantiate(hexPrefab, worldPos, Quaternion.identity).GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colors.Length)];

            }

        //visualization of roots
        //foreach (Vector2 position in rootMap)
        //    Instantiate(tester, position, Quaternion.identity);


        //re-building the hexMap
        int similarity;
        do
        {
            similarity = 0;
            List<GameObject> neighbors = new List<GameObject>();

            foreach (Vector2 root in rootMap)
            {
                Collider2D[] hexes = Physics2D.OverlapCircleAll(root, hexSize / 2f);

                if (hexes[0].GetComponent<SpriteRenderer>().color == hexes[1].GetComponent<SpriteRenderer>().color && hexes[0].GetComponent<SpriteRenderer>().color == hexes[2].GetComponent<SpriteRenderer>().color)
                {
                    similarity++;

                    if (!neighbors.Contains(hexes[0].gameObject)) neighbors.Add(hexes[0].gameObject);
                    if (!neighbors.Contains(hexes[1].gameObject)) neighbors.Add(hexes[1].gameObject);
                    if (!neighbors.Contains(hexes[2].gameObject)) neighbors.Add(hexes[2].gameObject);
                }


            }

            foreach (GameObject victim in neighbors)
                victim.GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colors.Length)];

            neighbors.Clear();

        } while (similarity > 0);

    }




    private void Update()
    {
        //selection hex group
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D[] hexes = Physics2D.OverlapCircleAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.25f);

            //Debug.Log(hexes.Length);

            if (hexes.Length == 3)
            {
                if (mother.transform.childCount == 3)
                    Release();

                Collect(hexes);
            }

        }

        //spinning the selected group
        //if (Input.GetKeyDown(KeyCode.Space))
        //    Spin(true);

        //testing the gamestate
        if (Input.GetKeyDown(KeyCode.Space) && mother.transform.childCount == 3)
            Spin(true);

        if (Input.GetKeyDown(KeyCode.T))
            Debug.Log(PossibleMovements());
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
    }

    public void Spin(bool clockwise)
    {
        if (clockwise)
        {
            //rotating clockwise
            for (int i = 0; i < 3; i++)
            {

            }

            //action
            Action();
        }

        else
        {
            //rotating anticlockwise

            //action
            Action();
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

    //destroying victims
    public void Action()
    {

        List<GameObject> victims = NeighborhoodTest();

        if (victims != null)
        {
            foreach (GameObject victim in victims)
                Destroy(victim);
        }

        victims.Clear();
    }

    public int PossibleMovements()
    {
        int posibilities = 0;

        foreach (Vector2 root in rootMap)
        {
            Collider2D[] hexes = Physics2D.OverlapCircleAll(root, hexSize / 2f);
            Collider2D[] others;

            if (hexes[0].GetComponent<SpriteRenderer>().color == hexes[1].GetComponent<SpriteRenderer>().color)
            {
                others = Physics2D.OverlapCircleAll(hexes[2].transform.position, hexSize * 1.25f);

                for (int i = 0; i < others.Length; i++)
                    if (others[i] != hexes[0] && others[i] != hexes[1] && others[i].GetComponent<SpriteRenderer>().color == hexes[0].GetComponent<SpriteRenderer>().color) posibilities++;
            }
            else if (hexes[0].GetComponent<SpriteRenderer>().color == hexes[2].GetComponent<SpriteRenderer>().color)
            {
                others = Physics2D.OverlapCircleAll(hexes[1].transform.position, hexSize * 1.25f);

                for (int i = 0; i < others.Length; i++)
                    if (others[i] != hexes[0] && others[i] != hexes[2] && others[i].GetComponent<SpriteRenderer>().color == hexes[0].GetComponent<SpriteRenderer>().color) posibilities++;
            }
            else if (hexes[1].GetComponent<SpriteRenderer>().color == hexes[2].GetComponent<SpriteRenderer>().color)
            {
                others = Physics2D.OverlapCircleAll(hexes[0].transform.position, hexSize * 1.25f);

                for (int i = 0; i < others.Length; i++)
                    if (others[i] != hexes[1] && others[i] != hexes[2] && others[i].GetComponent<SpriteRenderer>().color == hexes[1].GetComponent<SpriteRenderer>().color) posibilities++;
            }
        }

        return posibilities;
    }

}
