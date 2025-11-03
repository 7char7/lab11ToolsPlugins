using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Pathfinding : MonoBehaviour
{
    [Header("Grid variables")]
    public int gridWidth;
    public int gridHeight;
    public float obstacleProbability;

    [Header("Start and end points")]
    public Vector2Int start;
    public Vector2Int goal;

    [Header("Manual obstacle placement")]
    public Vector2Int obstaclePosition;


    public bool done = false;

    private List<Vector2Int> path = new List<Vector2Int>();
    private Vector2Int next;
    private Vector2Int current;

    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    private int[,] grid;

    private void Start()
    {
        GenerateRandomGrid(gridWidth, gridHeight, obstacleProbability);
        FindPath(start, goal);
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        float cellSize = 1f;
        //draw grid cells
        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);
                Gizmos.color = grid[y, x] == 1 ? Color.black : Color.white;
                Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
            }
        }

        // Draw path
        foreach (var step in path)
        {
            Vector3 cellPosition = new Vector3(step.x * cellSize, 0, step.y * cellSize);
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
        }

        // Draw start and goal
        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(start.x * cellSize, 0, start.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));

        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(goal.x * cellSize, 0, goal.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));
    }

    private bool IsInBounds(Vector2Int point)
    {
        return point.x >= 0 && point.x < grid.GetLength(1) &&
               point.y >= 0 && point.y < grid.GetLength(0);
    }

    private void FindPath(Vector2Int start, Vector2Int goal)
    {
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(start);

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            current = frontier.Dequeue();

            if (current == goal)
            {
                break;
            }

            foreach (Vector2Int direction in directions)
            {
                next = current + direction;

                if (IsInBounds(next) && grid[next.y, next.x] == 0 && !cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            Debug.Log("Path not found.");
            return;
        }

        // Trace path from goal to start
        Vector2Int step = goal;
        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Add(start);
        path.Reverse();

        Debug.Log($"Path found with {path.Count} steps.");
    }

    // Create random grid
    void GenerateRandomGrid(int width, int height, float obstacleProbability)
    {
        grid = new int[height, width]; 

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[y, x] = Random.value < obstacleProbability ? 1 : 0;
            }
        }

        //prevent start and goal from being obstacles
        grid[start.y, start.x] = 0;
        grid[goal.y, goal.x] = 0;
    }

    //adds the obsticles
    public void AddObstacle(Vector2Int position)
    {
        if (grid == null)
        {
            Debug.LogWarning("Grid not generated yet!");
            return;
        }

        if (position.x >= 0 && position.x < grid.GetLength(1) &&
            position.y >= 0 && position.y < grid.GetLength(0))
        {
            grid[position.y, position.x] = 1;
        }
        else
        {
            Debug.LogWarning("Position out of grid bounds!");
        }
    }

    // added the button to the 3 dots thing in corner, made better under but keeping here
    [ContextMenu("Add Obstacle At Coordinates")]
    public void AddObstacleFromInspector()
    {
        AddObstacle(obstaclePosition);
        Debug.Log($"Added obstacle manually at {obstaclePosition}");

        // Optional: update path visualization
        path.Clear();
        FindPath(start, goal);
    }

}


//makes button to add obsticles 
#if UNITY_EDITOR

[CustomEditor(typeof(Pathfinding))]
public class PathfindingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Pathfinding script = (Pathfinding)target;
        if (GUILayout.Button("Add Obstacle At Coordinates"))
        {
            script.AddObstacleFromInspector();
        }
    }
}
#endif
