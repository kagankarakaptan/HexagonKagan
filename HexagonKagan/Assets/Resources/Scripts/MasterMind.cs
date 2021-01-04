using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MasterMind : MonoBehaviour
{
    public int score; //current score variable
    public Text scoreText;
    public int scoreBorder; //for the bomb spawn

    public GameObject mother; //the center point and a parent for the rotation of the selected group of 3 hexes

    public GameObject hexPrefab; //prefab of the hex itself
    public GameObject hiddenPrefab; //invisible hex prefab for the bottom of the gridMap

    public Sprite hexSprite; //sprite of the hexagon itself
    public Sprite bombSprite; //sprite of the bomb itself

    public Color[] colors; //possible colors array for the hexes
    [HideInInspector] public float hexSize; //keeps the size of the hexes with spacing value
    public float spacing; //spacing between hexes

    private Vector2[] rootMap; //1D matrix of the vector2 that keeps the collision points of the hexes each-other 

    public int gridWidth; //size of the gridMap's width
    public int gridHeight; //size of the gridMap's height

    private Vector2 mapOffset; //position of the grid's bottom left corner

    //public bool canSpin; //keeps the game state (can we play or should we wait the movements of the environment ?)
    //public bool canFall; //keeps the game state (can we play or should we wait the movements of the environment ?)
    public bool canPlay; //keeps the game state (can we play or should we wait the movements of the environment ?)

    public int groundedCount;

    private void Awake()
    {
        rootMap = new Vector2[2 * (gridWidth - 1) * (gridHeight - 1)];
        hexSize = hexPrefab.GetComponent<SpriteRenderer>().bounds.size.x + spacing;
        mapOffset = transform.position;
        score = 0;
        scoreBorder = 1000;

        groundedCount = gridHeight * gridWidth;

    }

    private void Start()
    {
        //creating the hexGrid
        BuildMap();

        //re-building the hexMap
        ReBuild();

        //fixing the hexMap
        if (PossibleMovements() == 0) ReBuildAll();

        //setting the initial values when everything is ready
        //canSpin = true;
        //canFall = false;
        canPlay = true;


    }




    private void Update()
    {

        //selection hex group
        if (Input.GetMouseButtonDown(0) && canPlay)
        {
            Collect(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            ////mobile testing...
            //Spin(true);
        }

        //Spinning the gruop
        if (Input.GetKeyDown(KeyCode.Space) && mother.transform.childCount == 3)
            Spin(true);


    }

    public void Collect(Vector2 position)
    {
        Collider2D[] hexes = Physics2D.OverlapCircleAll(position, hexSize / 4f);

        if (hexes.Length == 3)
        {
            if (mother.transform.childCount == 3)
                Release();

            Vector2 centerOfMass = (hexes[0].transform.position + hexes[1].transform.position + hexes[2].transform.position) / 3;
            mother.transform.position = centerOfMass;
            mother.GetComponent<SpriteRenderer>().enabled = true;


            foreach (Collider2D hex in hexes)
                hex.gameObject.transform.SetParent(mother.transform);
        }




    }

    //detaching children from currentSelection
    public void Release()
    {
        for (int i = 0; i < mother.transform.childCount; i++)
            mother.transform.GetChild(i).rotation = Quaternion.identity;

        //if (mother.transform.GetChild(0) != null) mother.transform.GetChild(0).rotation = Quaternion.identity;
        //if (mother.transform.GetChild(1) != null) mother.transform.GetChild(1).rotation = Quaternion.identity;
        //if (mother.transform.GetChild(2) != null) mother.transform.GetChild(2).rotation = Quaternion.identity;

        //Transform[] children = mother.transform.GetComponentsInChildren<Transform>();
        //foreach (Transform child in children)
        //    child.transform.rotation = Quaternion.identity;

        mother.transform.DetachChildren();
        mother.GetComponent<SpriteRenderer>().enabled = false;

        mother.GetComponent<Animator>().Play("Idle");
        //mother.transform.rotation = Quaternion.Euler(0, 0, 0);

    }

    //spinning the hex goup
    public void Spin(bool direction)
    {
        //canFall = false;
        canPlay = false;

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
        List<GameObject> victims = NeighborhoodTest();

        if (victims.Count != 0)
        {
            //Debug.Log(mother.transform.GetChild(0).position);
            //GameObject temp = mother.transform.GetChild(0).gameObject;
            Release();
            //Debug.Log(temp.transform.position);


            foreach (GameObject victim in victims)
            {
                Kill(victim);
            }

            if (score >= scoreBorder)
                BombSpawn(victims[Random.Range(0, victims.Count)]);


            //canFall = true;
            StartCoroutine(WaitAndAction());
        }
        else if (mother.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            CountDown();

            Collect(mother.transform.position);

            canPlay = true;

            //Debug.Log(PossibleMovements());
            if (PossibleMovements() == 0)
                GameOver();

        }


        victims.Clear();



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

    public void ReBuildAll()
    {
        GameObject[] hexes = GameObject.FindGameObjectsWithTag("hex");

        foreach (GameObject hex in hexes)
            hex.GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colors.Length)];

        if (PossibleMovements() == 0)
            ReBuildAll();

    }

    private IEnumerator WaitAndAction()
    {
        //yield return null;
        //Debug.Log("falling started");
        //Debug.Break();

        do yield return null;
        while (groundedCount != gridHeight * gridWidth);
        Action();

        //Debug.Log("everything settled");
        //canFall = false;




    }

    public void Kill(GameObject victim)
    {


        victim.transform.position += new Vector3(0, (gridHeight + 5) * hexSize * Mathf.Sqrt(3) / 2f, 0); //move up
        victim.GetComponent<SpriteRenderer>().color = colors[Random.Range(0, colors.Length)]; //change color

        if (victim.transform.CompareTag("bomb"))
        {
            victim.transform.tag = "hex";
            victim.GetComponent<HexPhysics>().clock = 999;
            victim.GetComponent<SpriteRenderer>().sprite = hexSprite;
            victim.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        }

        GetScore();
    }

    public void GetScore()
    {
        score += 5;
        scoreText.GetComponent<Text>().text = score.ToString();
        //effects
    }

    public void BombSpawn(GameObject victim)
    {
        victim.transform.tag = "bomb";
        //victim.GetComponent<SpriteRenderer>().sprite = bombSprite;
        //victim.GetComponent<SpriteRenderer>().color = Color.black; //for visual testing
        victim.GetComponent<HexPhysics>().clock = Random.Range(6, 11);
        victim.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        scoreBorder += 1000;

    }

    public void CountDown()
    {
        GameObject[] bombs = GameObject.FindGameObjectsWithTag("bomb");

        foreach (GameObject bomb in bombs)
        {
            bomb.GetComponent<HexPhysics>().clock--;
            bomb.transform.GetChild(0).GetComponent<TextMesh>().text = bomb.GetComponent<HexPhysics>().clock.ToString();
            if (bomb.GetComponent<HexPhysics>().clock == 0)
                GameOver();
        }

    }

    public void GameOver()
    {
        Debug.Log("game over!");
    }
}
