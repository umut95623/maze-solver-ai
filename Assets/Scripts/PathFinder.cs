using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics; 

public class PathFinder : MonoBehaviour
{
    public enum AlgorithmType { BFS, DFS }

    [Header("Ayarlar")]
    public AlgorithmType selectedAlgorithm = AlgorithmType.BFS;  
    public float stepDelay = 0.1f;      

    [Header("Renkler")]
    public Color visitedColor = Color.yellow;   
    public Color pathColor = Color.blue;         

    public static bool isSearching = false; 
    private static float bfsTime = 0;  
    private static int bfsLen = 0;
    private static float dfsTime = 0;
    private static int dfsLen = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isSearching)
        {
            StartCoroutine(RunAlgorithm()); 
        }
    }

    IEnumerator RunAlgorithm()   
    {
        isSearching = true; 
        ResetMazeState();   
        Stopwatch stopwatch = new Stopwatch(); 
        stopwatch.Start();

        if (MazeLoader.grid == null) { isSearching = false; yield break; }
        Queue<MazeCell> queue = new Queue<MazeCell>();  
        Stack<MazeCell> stack = new Stack<MazeCell>(); 
        MazeCell startCell = MazeLoader.grid[MazeLoader.startPos.x, MazeLoader.startPos.y];
        startCell.visited = true;

        if (selectedAlgorithm == AlgorithmType.BFS) queue.Enqueue(startCell);  
        else stack.Push(startCell);

        bool targetFound = false;
        MazeCell endCell = null;

        int[] dX = { 0, -1, 0, 1 };
        int[] dY = { -1, 0, 1, 0 };
        int[] priorityOrder = { 2, 1, 3, 0 };

        while (queue.Count > 0 || stack.Count > 0) 
        {
            MazeCell current;

            if (selectedAlgorithm == AlgorithmType.BFS) current = queue.Dequeue(); 
            else current = stack.Pop();

            if (current.x == MazeLoader.endPos.x && current.y == MazeLoader.endPos.y)  
            {
                targetFound = true;
                endCell = current;
                stopwatch.Stop();  
                float elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f; 
                int routeLength = GetPathLength(current);

                if (selectedAlgorithm == AlgorithmType.BFS)
                {
                    bfsTime = elapsedSeconds;   
                    bfsLen = routeLength;
                }
                else
                {
                    dfsTime = elapsedSeconds;
                    dfsLen = routeLength;
                }

                if (MazeLoader.infoText != null)    
                    MazeLoader.UpdateScoreBoardText(bfsTime, bfsLen, dfsTime, dfsLen);

                current.textMesh.text = "Visited";  
                current.textMesh.characterSize = 0.3f;
                current.textMesh.color = Color.black;
                break;   
            } 

            if (current != startCell) 
            {
                current.floorObject.GetComponent<Renderer>().material.color = visitedColor;   
                if (current != endCell)
                {
                    current.textMesh.text = "Visited";   
                    current.textMesh.characterSize = 0.3f;
                    current.textMesh.color = Color.black;
                }
            }

            yield return new WaitForSeconds(stepDelay);  

            if (selectedAlgorithm == AlgorithmType.DFS)   
            {
                for (int i = priorityOrder.Length - 1; i >= 0; i--) 
                {
                    int dirIndex = priorityOrder[i];
                    CheckNeighbor(current, dX[dirIndex], dY[dirIndex], queue, stack);
                }
            }
            else 
            {
                for (int i = 0; i < 4; i++)    
                    CheckNeighbor(current, dX[i], dY[i], queue, stack);
            }
        }

        if (targetFound)
        {
            DrawPath(endCell);  
        }

        isSearching = false;
    }

    void ResetMazeState()    
    {
        int counter = 1;

        for (int x = 0; x < MazeLoader.gridWidth; x++)
        {
            for (int y = 0; y < MazeLoader.gridHeight; y++)
            {
                MazeCell cell = MazeLoader.grid[x, y];

                cell.visited = false;
                cell.parent = null;

                Renderer rend = cell.floorObject.GetComponent<Renderer>();  
                if (x == MazeLoader.startPos.x && y == MazeLoader.startPos.y)
                    rend.material.color = Color.green;
                else if (x == MazeLoader.endPos.x && y == MazeLoader.endPos.y)
                    rend.material.color = Color.red;
                else
                    rend.material.color = Color.white;

                cell.textMesh.text = counter.ToString();  
                cell.textMesh.characterSize = 0.6f; 
                cell.textMesh.color = Color.black;

                counter++;
            }
        }
    }

    public static void ResetScores() 
    {
        bfsTime = 0;
        bfsLen = 0;
        dfsTime = 0;
        dfsLen = 0;
    }

    void CheckNeighbor(MazeCell current, int dx, int dy, Queue<MazeCell> q, Stack<MazeCell> s)
    {
        int neighborX = current.x + dx;
        int neighborY = current.y + dy;

        if (neighborX >= 0 && neighborX < MazeLoader.gridWidth &&  
            neighborY >= 0 && neighborY < MazeLoader.gridHeight)
        {
            MazeCell neighbor = MazeLoader.grid[neighborX, neighborY];

            int wallIndex = -1;     
            if (dx == 0 && dy == -1) wallIndex = 0;    
            else if (dx == -1 && dy == 0) wallIndex = 1;
            else if (dx == 0 && dy == 1) wallIndex = 2;
            else if (dx == 1 && dy == 0) wallIndex = 3;

            if (wallIndex != -1 && current.walls[wallIndex] == false && !neighbor.visited)  
            {
                neighbor.visited = true;
                neighbor.parent = current;
                if (selectedAlgorithm == AlgorithmType.BFS) q.Enqueue(neighbor);
                else s.Push(neighbor);
            }
        }
    }

    void DrawPath(MazeCell endCell)  
    {
        MazeCell current = endCell;
        while (current != null)
        {
            if ((current.x != MazeLoader.startPos.x || current.y != MazeLoader.startPos.y) &&
                (current.x != MazeLoader.endPos.x || current.y != MazeLoader.endPos.y))
            {
                current.floorObject.GetComponent<Renderer>().material.color = pathColor;
            }
            current = current.parent;
        }
    }

    int GetPathLength(MazeCell endCell) 
    {
        int count = 0;
        MazeCell current = endCell;
        while (current != null) { count++; current = current.parent; }
        return count;
    }
}