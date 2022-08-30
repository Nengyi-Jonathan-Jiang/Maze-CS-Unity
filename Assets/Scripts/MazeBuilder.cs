using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeBuilder : MonoBehaviour {
    public int SIZE;

    public GameObject MazeNodePrefab;
    public GameObject MazeWallPrefab;
    public GameObject MazeKeyPrefab;
    public GameObject MazeDoorPrefab;
    public MazeData mazeData;

    // Start is called before the first frame update
    void Start() {
        mazeData = new MazeData(SIZE);
        Build();
        GenerateGameObjects();
    }

    public void Build() {
        var visited = new bool[SIZE, SIZE];
        var parent = new Vector2Int[SIZE, SIZE];
        var children = new List<Vector2Int>[SIZE, SIZE];

        var stk = new Stack<Vector2Int>();
        var dStk = new Stack<int>();
        
        var start = new Vector2Int(Random.Range(0, SIZE), Random.Range(0, SIZE));
        var end = start;
        var maxDepth = 0;
        
        stk.Push(start);
        dStk.Push(0);
        while (stk.Count > 0) {
            var p = stk.Peek();
            var depth = dStk.Peek();

            if (depth > maxDepth) {
                end = p;
                maxDepth = depth;
            }
            
            int row = p.x, col = p.y;
            visited[row, col] = true;
            //Get neighbors
            var neighbors = mazeData.GetNeighbors(row, col, pt => !visited[pt.x, pt.y]);
            if (neighbors.Count < 2) {
                stk.Pop();
                dStk.Pop();
            }
            if (neighbors.Count == 0) continue;
            var newPos = mazeData.RemoveNeighborWall(
                neighbors[Random.Range(0, neighbors.Count)],
                row, col
            );
            
            stk.Push(newPos);
            dStk.Push(depth + 1);

            children[p.x, p.y].Add(newPos);
            parent[newPos.x, newPos.y] = p;
        }

        GameObject.Find("Player").transform.position = new Vector2(start.x, start.y) * 8;
        GameObject.Find("Finish").transform.position = new Vector2(end.x, end.y) * 8;
        
        visited[start.x, start.y] = visited[end.x, end.y] = false;

        var solutionPath = new HashSet<Vector2Int>(new Vector2Int[]{end});
        while (end != start) {
            end = parent[end.x, end.y];
            solutionPath.Add(end);
        }



        Color[] COLORS = {Color.yellow, Color.red, Color.green, Color.blue};

        var hasDoor = new bool[SIZE, SIZE];
        var used = new bool[SIZE, SIZE];

        for (var i = 0; i < 4; i++) {   //We will place 3 keys
            var doorPos = new List<Vector2Int>(solutionPath)[Random.Range(0, solutionPath.Count)];
            AddMazeDoorAt(doorPos.x, doorPos.y, COLORS[i]);
            used[doorPos.x, doorPos.y] = hasDoor[doorPos.x, doorPos.y] = true;

            var doorPath = GetRoute(start, doorPos, parent);
            solutionPath.UnionWith(doorPath);

            var reachable = new HashSet<Vector2Int>();
            {
                var stk2 = new Stack<Vector2Int>(doorPath);
                while (stk2.Count > 0) {
                    Vector2Int pos = stk2.Pop();
                    var neighbors = mazeData.GetNeighbors(pos.x, pos.y, i => !hasDoor[i.x, i.y] && !reachable.Contains(i));
                    foreach (var n in neighbors) {
                        reachable.Add(mazeData.GetNeighbor(n, pos.x, pos.y));
                    }
                }
            }

            var possibleKeyPositions = new List<Vector2Int>(from item in reachable
                where !used[item.x, item.y]
                select item);


            /*
            
            var random = start;
            var tries = 0;
            while (!visited[random.x, random.y] || solutionPath.Contains(random)) {
                random = new Vector2Int(Random.Range(0, SIZE), Random.Range(0, SIZE));
                if(++tries > 100) goto done;
            }
            
            //Place key here
            AddMazeKeyAt(random.x, random.y, COLORS[i]);
            //Traverse back
            while (random != start) {
                visited[random.x, random.y] = false;
                random = tree[random.x, random.y];
            }

            tries = 0;
            while (!visited[random.x, random.y]) {
                random = solutionPath[Random.Range(0, solutionPath.Count)];
                if (++tries > 100) goto done;
            }

            //Place door here
            AddMazeDoorAt(random.x, random.y, COLORS[i]);
            visited[random.x, random.y] = false;

            */
        }

        // done:;
    }

    private static HashSet<Vector2Int> GetRoute(Vector2Int start, Vector2Int target, Vector2Int[,] parent) {
        var res = new HashSet<Vector2Int>();

        while (true) {
            target = parent[target.x, target.y];
            if (target == start) break;
            res.Add(target);
        }

        return res;
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

    private void AddMazeKeyAt(int x, int y, Color color) {
        GameObject res = Instantiate(MazeKeyPrefab);
        res.transform.position = new Vector2(x * 8, y * 8);
        res.transform.parent = gameObject.transform;
        res.name = "Key";
        res.GetComponent<SpriteRenderer>().color = color;
    }

    private void AddMazeDoorAt(int x, int y, Color color) {
        GameObject res = Instantiate(MazeDoorPrefab);
        res.transform.position = new Vector2(x * 8, y * 8);
        res.transform.parent = gameObject.transform;
        res.name = "Door";
        res.GetComponent<SpriteRenderer>().color = color;
    }
}
