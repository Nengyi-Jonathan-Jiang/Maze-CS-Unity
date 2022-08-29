using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeBuilder : MonoBehaviour {
    public int SIZE;

    public GameObject MazeNodePrefab;
    public GameObject MazeWallPrefab;
    public MazeData mazeData;

    // Start is called before the first frame update
    void Start() {
        mazeData = new MazeData(SIZE);
        Build();
        GenerateGameObjects();
    }

    public void Build() {
        var visited = new bool[SIZE, SIZE];
        var tree = new Vector2Int[SIZE, SIZE];
        Stack<Vector2Int> stk = new Stack<Vector2Int>();
        var start = new Vector2Int(Random.Range(0, SIZE), Random.Range(0, SIZE));
        stk.Push(start);
        while (stk.Count > 0) {
            var p = stk.Peek();
            int row = p.x, col = p.y;
            visited[row, col] = true;
            //Get neighbors
            var neighbors = mazeData.GetNeighbors(row, col, pt => !visited[pt.x, pt.y]);
            if (neighbors.Count < 2) stk.Pop();
            if (neighbors.Count == 0) continue;
            var newPos = mazeData.RemoveNeighborWall(
                neighbors[Random.Range(0, neighbors.Count)],
                row, col
            );
            stk.Push(newPos);
            tree[newPos.x, newPos.y] = p;
        }

        GameObject.Find("Player").transform.position = new Vector2(start.x, start.y) * 8;

        Debug.Log(tree);
    }

    private void GenerateGameObjects() {
        for (var i = 0; i <= SIZE; i++)
            for(var j = 0; j <= SIZE; j++)
                AddMazeNodeAt(i, j);

        for (var i = 0; i < SIZE; i++) {
            AddMazeWallAt(i, -1, false);
            AddMazeWallAt(-1, i, true);
        }
        
        for (var i = 0; i < SIZE; i++) {
            for (var j = 0; j < SIZE; j++) {
                if (mazeData[i, j].HasBottomWall()) {
                    AddMazeWallAt(i, j, true);
                }
                if (mazeData[i, j].HasRightWall()) {
                    AddMazeWallAt(i, j, false);
                }
            }
        }
    }

    private void AddMazeNodeAt(int x, int y) {
        GameObject res = Instantiate(MazeNodePrefab);
        res.transform.position = new Vector2(x * 8 - 4, y * 8 - 4);
        res.transform.parent = gameObject.transform;
        res.name = "Node";
    }

    private void AddMazeWallAt(int x, int y, bool rotation) {
        GameObject res = Instantiate(MazeWallPrefab);
        res.transform.position = new Vector2(x * 8 + (rotation ? 4 : 0), y * 8 + (rotation ? 0 : 4));
        res.transform.parent = gameObject.transform;
        if (rotation) res.transform.rotation = Quaternion.Euler(0, 0, 90);
        res.name = "Wall";
    }
}
