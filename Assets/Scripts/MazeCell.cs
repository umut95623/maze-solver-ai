using UnityEngine; 

public class MazeCell
{
    public int x, y;
    public bool[] walls = new bool[4];
    public bool visited = false;
    public MazeCell parent;
    public GameObject floorObject; 
    public TextMesh textMesh; 
}