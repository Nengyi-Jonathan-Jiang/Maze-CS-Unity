using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public class MazeBuilder : MonoBehaviour {
        public Color[] KeyColors = {Color.yellow, Color.red, Color.green, Color.blue};
        
        
        public int Size;

        public GameObject MazeNodePrefab;
        public GameObject MazeWallPrefab;
        public GameObject MazeKeyPrefab;
        public GameObject MazeDoorPrefab;

        // Start is called before the first frame update
        private void Start() {
            Build();
        }

        private void Build() {
            var mazeData = new MazeData(Size);
            var visited = new bool[Size, Size];
            var parent = new Vector2Int[Size, Size];
            var children = new List<Vector2Int>[Size, Size];
            for (var i = 0; i < Size; ++i) for (var j = 0; j < Size; ++j) children[i, j] = new List<Vector2Int>();

            var stk = new Stack<Vector2Int>();
            var dStk = new Stack<int>();

            var start = new Vector2Int(Random.Range(0, Size), Random.Range(0, Size));
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

            for (var i = 0; i <= Size; i++)
                for (var j = 0; j <= Size; j++)
                    AddMazeNodeAt(i, j);

            for (var i = 0; i < Size; i++) {
                AddMazeWallAt(i, -1, false);
                AddMazeWallAt(-1, i, true);
            }

            for (var i = 0; i < Size; i++) {
                for (var j = 0; j < Size; j++) {
                    if (mazeData[i, j].HasBottomWall())
                        AddMazeWallAt(i, j, true);
                    if (mazeData[i, j].HasRightWall())
                        AddMazeWallAt(i, j, false);
                }
            }

            visited[start.x, start.y] = visited[end.x, end.y] = false;


            var hasDoor = new bool[Size, Size];
            var used = new bool[Size, Size];
            used[start.x, start.y] = used[end.x, end.y] = true;

            var solutionPath = new HashSet<Vector2Int>(new[]{start, end});
            while (end != start) {
                end = parent[end.x, end.y];
                solutionPath.Add(end);
            }
            var keyPaths = new HashSet<Vector2Int>(solutionPath);

            for (int i = 0, color = 0; i < 100 && color < KeyColors.Count(); i++) {
                Vector2Int doorPos;
                try {
                    try {
                        doorPos = GetRandomElement((i == 0 ? solutionPath : keyPaths.Except(solutionPath)).Where(s => !used[s.x, s.y]));
                    }
                    catch {
                        doorPos = GetRandomElement(solutionPath.Where(s => !used[s.x, s.y]));
                    }
                }
                catch {break;}

                used[doorPos.x, doorPos.y] = hasDoor[doorPos.x, doorPos.y] = true;

                var doorPath = GetRoute(start, doorPos, parent);

                var reachable = new HashSet<Vector2Int>(doorPath);
                var stk2 = new Stack<Vector2Int>(doorPath);
                while (stk2.Count > 0) {
                    var pos = stk2.Pop();
                    var neighbors = mazeData.GetNeighbors(pos.x, pos.y, p =>
                        !hasDoor[p.x, p.y] && 
                        !reachable.Contains(p) 
                        && children[pos.x, pos.y].Contains(p)
                    );
                    foreach (var n in neighbors.Select(d => mazeData.GetNeighbor(d, pos.x, pos.y))) {
                        reachable.Add(n);
                        stk2.Push(n);
                    }
                }

                var possibleKeyPositions = new List<Vector2Int>(reachable);

                Vector2Int keyPos;
                try {
                    try {
                        keyPos = GetRandomElement(possibleKeyPositions.Where(s => 
                            !used[s.x, s.y] && 
                            !keyPaths.Contains(s) && 
                            children[s.x, s.y].Count == 0
                        ));
                    }
                    catch {
                        keyPos = GetRandomElement(possibleKeyPositions.Where(s => !used[s.x, s.y] && !keyPaths.Contains(s)));
                    }
                }
                catch {
                    continue;
                }
                
                var keyPath = GetRoute(start, keyPos, parent);
                keyPaths.UnionWith(keyPath);
                

                AddMazeDoorAt(doorPos.x, doorPos.y, KeyColors[color]);
                AddMazeKeyAt(keyPos.x, keyPos.y, KeyColors[color]);
                used[keyPos.x, keyPos.y] = true;
                color++;
            }
        }

        private static HashSet<Vector2Int> GetRoute(Vector2Int start, Vector2Int target, Vector2Int[,] parent) {
            var res = new HashSet<Vector2Int>();
            while (true) {
                target = parent[target.x, target.y];
                if (target == start)
                    break;
                res.Add(target);
            }
            return res;
        }

        private static T GetRandomElement<T>(IEnumerable<T> l) {
            var list = new List<T>(l);
            return list[Random.Range(0, list.Count)];
        }
        

        private void AddMazeNodeAt(int x, int y) {
            var res = Instantiate(MazeNodePrefab);
            res.transform.position = new Vector2(x * 8 - 4, y * 8 - 4);
            res.transform.parent = gameObject.transform;
            res.name = "Node";
        }

        private void AddMazeWallAt(int x, int y, bool rotation) {
            var res = Instantiate(MazeWallPrefab);
            res.transform.position = new Vector2(x * 8 + (rotation ? 4 : 0), y * 8 + (rotation ? 0 : 4));
            res.transform.parent = gameObject.transform;
            if (rotation)
                res.transform.rotation = Quaternion.Euler(0, 0, 90);
            res.name = "Wall";
        }

        private void AddMazeKeyAt(int x, int y, Color color) {
            var res = Instantiate(MazeKeyPrefab);
            res.transform.position = new Vector2(x * 8, y * 8);
            res.transform.parent = gameObject.transform;
            res.name = "Key";
            res.GetComponent<SpriteRenderer>().color = color;
        }

        private void AddMazeDoorAt(int x, int y, Color color) {
            var res = Instantiate(MazeDoorPrefab);
            res.transform.position = new Vector2(x * 8, y * 8);
            res.transform.parent = gameObject.transform;
            res.name = "Door";
            res.GetComponent<SpriteRenderer>().color = color;
        }
    }


}