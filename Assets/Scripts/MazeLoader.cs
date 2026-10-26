using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeLoader : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject wallPrefab;            
    public GameObject floorPrefab;    
    public string fileName = "maze";  

    [Header("Yazı Ayarı")]
    public Vector3 textLocalScale = new Vector3(1, 1, 1);

    public static MazeCell[,] grid;
    public static int gridWidth;
    public static int gridHeight;
    public static Vector2Int startPos;
    public static Vector2Int endPos;
    public static TextMesh infoText;
    private TextMesh warningText;

    private List<string> lines = new List<string>();

    void Start()
    {
        PathFinder.ResetScores();  

        grid = null;
        ParseFile();   
        GenerateMazeVisualsAndLogic(); 
        CreateWarningText();  
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) 
        {
            if (PathFinder.isSearching)
            {
                StartCoroutine(ShowWarningMessage());
            }
            else
            {
                GenerateRandomMaze();  
            }
        }
    }


    IEnumerator ShowWarningMessage()   
    {
        if (warningText != null)
        {
            warningText.text = "WAIT!";
            yield return new WaitForSeconds(2.0f); 
            warningText.text = "";
        }
    }

    void CreateWarningText()  
    {
        GameObject warnObj = new GameObject("WarningText");
        warnObj.transform.parent = this.transform;
        warnObj.transform.position = new Vector3(gridWidth + 0.2f, 2f, -gridHeight / 2f);  
        warnObj.transform.rotation = Quaternion.Euler(90, 0, 0);

        warningText = warnObj.AddComponent<TextMesh>();
        warningText.characterSize = 0.2f;
        warningText.fontSize = 60;
        warningText.anchor = TextAnchor.MiddleLeft;
        warningText.color = Color.red;
        warningText.text = "";   
    }

    void GenerateRandomMaze()
    {
        PathFinder.ResetScores();  

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);   
        }
         
        int sizeX = 8;
        int sizeY = 8;  
        gridWidth = sizeX;
        gridHeight = sizeY;

        grid = new MazeCell[sizeX, sizeY];     
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                grid[x, y] = new MazeCell();
                grid[x, y].x = x;
                grid[x, y].y = y;
                for (int w = 0; w < 4; w++) grid[x, y].walls[w] = true; 
            }
        }

        Stack<MazeCell> stack = new Stack<MazeCell>();   
        MazeCell startCell = grid[0, 0];
        startCell.visited = true;
        stack.Push(startCell);

        while (stack.Count > 0)  
        {
            MazeCell current = stack.Peek();
            List<MazeCell> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)  
            {
                MazeCell next = neighbors[Random.Range(0, neighbors.Count)];
                RemoveWalls(current, next);
                next.visited = true;
                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }

        int loopChance = 30;    
        for (int x = 1; x < gridWidth - 1; x++) 
        {
            for (int y = 1; y < gridHeight - 1; y++)
            {
                if (Random.Range(0, 100) < loopChance)
                {
                    int randomWall = Random.Range(0, 4);  
                    MazeCell current = grid[x, y];
                    MazeCell neighbor = null;

                    if (randomWall == 0) neighbor = grid[x, y - 1];  
                    else if (randomWall == 1) neighbor = grid[x - 1, y];   
                    else if (randomWall == 2) neighbor = grid[x, y + 1];  
                    else if (randomWall == 3) neighbor = grid[x + 1, y];  

                    if (neighbor != null) 
                    {
                        RemoveWalls(current, neighbor);
                    }
                }
            }
        }

        foreach (var cell in grid) cell.visited = false;  

        startPos = new Vector2Int(0, 0); 
        endPos = new Vector2Int(sizeX - 1, sizeY - 1);

        DrawRandomMazeWalls();  
        CreateFloors();
        CreateScoreBoard();
        CreateWarningText();   
    }

    List<MazeCell> GetUnvisitedNeighbors(MazeCell c)           
    {
        List<MazeCell> neighbors = new List<MazeCell>();
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = c.x + dx[i];
            int ny = c.y + dy[i];
            if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
            {
                if (!grid[nx, ny].visited) neighbors.Add(grid[nx, ny]);
            }
        }
        return neighbors;
    }

    void RemoveWalls(MazeCell a, MazeCell b) 
    {
        if (a.x == b.x)  
        {
            if (a.y > b.y) { a.walls[0] = false; b.walls[2] = false; } 
            else { a.walls[2] = false; b.walls[0] = false; }  
        }
        else   
        {
            if (a.x > b.x) { a.walls[1] = false; b.walls[3] = false; }  
            else { a.walls[3] = false; b.walls[1] = false; }   
        }
    }

    void DrawRandomMazeWalls()   
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                MazeCell c = grid[x, y];
                Vector3 pos;

                if (c.walls[0])        
                {
                    pos = new Vector3(x, 0.5f, -y + 0.5f);
                    GameObject w = Instantiate(wallPrefab, pos, Quaternion.identity);
                    w.transform.localScale = new Vector3(1.1f, 1f, 0.1f);
                    w.transform.parent = transform;
                }
                if (c.walls[1])  
                {
                    pos = new Vector3(x - 0.5f, 0.5f, -y);
                    GameObject w = Instantiate(wallPrefab, pos, Quaternion.identity);
                    w.transform.localScale = new Vector3(0.1f, 1f, 1.1f);
                    w.transform.parent = transform;
                }
                if (c.walls[2] && y == gridHeight - 1)   
                {
                    pos = new Vector3(x, 0.5f, -y - 0.5f);
                    GameObject w = Instantiate(wallPrefab, pos, Quaternion.identity);
                    w.transform.localScale = new Vector3(1.1f, 1f, 0.1f);
                    w.transform.parent = transform;
                }
                if (c.walls[3] && x == gridWidth - 1)     
                {
                    pos = new Vector3(x + 0.5f, 0.5f, -y);
                    GameObject w = Instantiate(wallPrefab, pos, Quaternion.identity);
                    w.transform.localScale = new Vector3(0.1f, 1f, 1.1f);
                    w.transform.parent = transform;
                }
            }
        }
    }

    void ParseFile()   
    {
        TextAsset txtData = Resources.Load<TextAsset>(fileName);
        if (txtData == null) { Debug.LogError("Dosya yok!"); return; }

        string[] rawLines = txtData.text.Split('\n');
        foreach (string line in rawLines)
        {
            if (!string.IsNullOrWhiteSpace(line)) lines.Add(line.Trim());
        }
    }

    void GenerateMazeVisualsAndLogic()   
    {
        int mapLineCount = lines.Count - 2;    
        gridHeight = mapLineCount / 2;
        string[] secondLineItems = lines[1].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        gridWidth = secondLineItems.Length - 1;

        grid = new MazeCell[gridWidth, gridHeight];   
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = new MazeCell();
                grid[x, y].x = x;
                grid[x, y].y = y;
                for (int w = 0; w < 4; w++) grid[x, y].walls[w] = false;
            }
        }

        int currentRowY = 0;
        for (int i = 0; i < mapLineCount; i++) 
        {
            string line = lines[i];
            string[] items = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            bool isVerticalRow = (i % 2 != 0);   

            for (int k = 0; k < items.Length; k++)
            {
                if (items[k] == "1")  
                {
                    Vector3 position;
                    Vector3 scale;

                    if (isVerticalRow) 
                    {
                        position = new Vector3(k - 0.5f, 0.5f, -currentRowY);
                        scale = new Vector3(0.1f, 1f, 1.1f);
                        if (k - 1 >= 0 && k - 1 < gridWidth) grid[k - 1, currentRowY].walls[3] = true;
                        if (k < gridWidth) grid[k, currentRowY].walls[1] = true;
                    }
                    else  
                    {
                        position = new Vector3(k, 0.5f, -currentRowY + 0.5f);
                        scale = new Vector3(1.1f, 1f, 0.1f);
                        if (currentRowY < gridHeight && k < gridWidth) grid[k, currentRowY].walls[0] = true;   
                        if (currentRowY - 1 >= 0 && k < gridWidth) grid[k, currentRowY - 1].walls[2] = true;
                    }

                    GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
                    wall.transform.localScale = scale;
                    wall.transform.parent = this.transform;
                }
            }
            if (isVerticalRow) currentRowY++;
        }

        string[] startLine = lines[lines.Count - 2].Split(',');  
        startPos = new Vector2Int(int.Parse(startLine[1]), int.Parse(startLine[0]));
        string[] endLine = lines[lines.Count - 1].Split(',');
        endPos = new Vector2Int(int.Parse(endLine[1]), int.Parse(endLine[0]));

        CreateFloors();
        CreateScoreBoard();
    }

    void CreateFloors()       
    {
        int counter = 1;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 pos = new Vector3(x, 0, -y);
                GameObject floor = Instantiate(floorPrefab, pos, Quaternion.identity);
                floor.name = $"Cell_{x}_{y}";
                floor.transform.parent = this.transform;

                GameObject textObj = new GameObject("Label");  
                textObj.transform.parent = floor.transform;
                textObj.transform.localPosition = new Vector3(0, 0.01f, 0);
                textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
                textObj.transform.localScale = textLocalScale;

                TextMesh tm = textObj.AddComponent<TextMesh>();
                tm.text = counter.ToString();
                tm.characterSize = 0.6f;
                tm.fontSize = 100;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.color = Color.black;

                grid[x, y].floorObject = floor; 
                grid[x, y].textMesh = tm;

                Renderer rend = floor.GetComponent<Renderer>(); 
                if (x == startPos.x && y == startPos.y) rend.material.color = Color.green;
                else if (x == endPos.x && y == endPos.y) rend.material.color = Color.red;
                else rend.material.color = Color.white;

                counter++;
            }
        }
    }

    void CreateScoreBoard() 
    {
        GameObject scoreObj = new GameObject("ScoreBoard");

        scoreObj.transform.parent = this.transform;
        scoreObj.transform.position = new Vector3(-1f, 1f, -gridHeight / 2f);
        scoreObj.transform.rotation = Quaternion.Euler(90, 0, 0);

        infoText = scoreObj.AddComponent<TextMesh>();
        infoText.characterSize = 0.06f;
        infoText.fontSize = 100;
        infoText.anchor = TextAnchor.MiddleRight;
        infoText.alignment = TextAlignment.Right;
        infoText.color = Color.white;

        UpdateScoreBoardText(0, 0, 0, 0);
    }

    public static void UpdateScoreBoardText(float bfsTime, int bfsLen, float dfsTime, int dfsLen) 
    {
        string bfsT = (bfsTime > 0) ? bfsTime.ToString("F2") + "s" : "-";
        string bfsL = (bfsLen > 0) ? bfsLen.ToString() : "-";
        string dfsT = (dfsTime > 0) ? dfsTime.ToString("F2") + "s" : "-";
        string dfsL = (dfsLen > 0) ? dfsLen.ToString() : "-";

        infoText.text = "COMPARISON\n\n" +
                        "Time:\n" +
                        $"BFS: {bfsT}\n" +
                        $"DFS: {dfsT}\n\n" +
                        "Length:\n" +
                        $"BFS: {bfsL}\n" +
                        $"DFS: {dfsL}";
    }
}