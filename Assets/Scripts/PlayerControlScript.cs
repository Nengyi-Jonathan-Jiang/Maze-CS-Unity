using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts {
    public class PlayerControlScript : MonoBehaviour {
        private int x, y;
        public float movementSpeed;

        SwipeDetector swipeDetector = new SwipeDetector();
        
        Action onFinishAnimate = null;
        
        public Vector2Int Pos {
            set {
                x = value.x;
                y = value.y;
                transform.position = new Vector3(x, y, 0) * 8;
                GetComponent<TrailRenderer>().Clear();
            }
        }

        public HashSet<string> Keys;

        public GameObject MarkerPrefab;
        public MazeBuilder maze;

        // Start is called before the first frame update
        private void Start() {
            Keys = new HashSet<string>();
        }

        // Update is called once per frame
        private void Update() {

            //var movement = new Vector2();
            /*
            if (Input.GetKey(KeyCode.W))
                movement += new Vector2(0, 1);
            if (Input.GetKey(KeyCode.A))
                movement += new Vector2(-1, 0);
            if (Input.GetKey(KeyCode.S))
                movement += new Vector2(0, -1);
            if (Input.GetKey(KeyCode.D))
                movement += new Vector2(1, 0);
            */

            var swipe = swipeDetector.check();
            if ((new Vector3(x, y, 0) * 8 - transform.position).magnitude > 1) { }
            else {
                if (swipe != Vector2.zero) {
                    var angle = Mathf.Atan2(swipe.y, swipe.x);
                    var a = Mathf.RoundToInt(2 * angle / Mathf.PI);
                    switch (a) {
                        case -2:
                        case 2:
                            Move(-1, 0, MazeData.Neighbor.Top);
                            break;
                        case -1:
                            Move(0, -1, MazeData.Neighbor.Left);
                            break;
                        case 0:
                            Move(1, 0, MazeData.Neighbor.Bottom);
                            break;
                        case 1:
                            Move(0, 1, MazeData.Neighbor.Right);
                            break;
                    }
                }
                else{
                    if (Input.GetKeyDown(KeyCode.W)) {
                        Move(0, 1, MazeData.Neighbor.Right);
                    }
                    else if (Input.GetKeyDown(KeyCode.A)) {
                        Move(-1, 0, MazeData.Neighbor.Top);
                    }
                    else if (Input.GetKeyDown(KeyCode.S)) {
                        Move(0, -1, MazeData.Neighbor.Left);
                    }
                    else if (Input.GetKeyDown(KeyCode.D)) {
                        Move(1, 0, MazeData.Neighbor.Bottom);
                    }
                }
            }


            //_rb.AddForce(movement * Time.deltaTime * 1000 * MovementSpeed);
            Vector3 delta = new Vector3(x, y, 0) * 8 - transform.position;
            Vector3 direction = delta.normalized;
            Vector3 movement = direction * Mathf.Min(movementSpeed * Time.deltaTime, delta.magnitude);
            
            if ((movement - delta).magnitude <= 1 && onFinishAnimate != null) {
                onFinishAnimate();
                onFinishAnimate = null;
            }
            
            transform.position += movement;
            
        }

        private void Move(int dx, int dy, MazeData.Neighbor n) {
            Debug.Log("Moving " + dx + " " + dy + " from " + x + " " + y);
            while (
                    maze.CanMove(x, y, n)
            ) {
                x += dx;
                y += dy;

                Debug.Log("X: " + x + ", y: " + y);

                GameObject obstacle = maze.GetObstacle(x, y);
                if (obstacle == null) {
                    if (
                        maze.CanMove(x, y, (MazeData.Neighbor) (((int) n + 1) & 3)) ||
                        maze.CanMove(x, y, (MazeData.Neighbor) (((int) n + 3) & 3))
                    ) {
                        return;
                    }
                    continue;
                }
                switch (obstacle.name) {
                    case "Key":
                        onFinishAnimate = () => {
                            obstacle.transform.parent = Camera.main.transform;
                            obstacle.transform.position = Camera.main.transform.position + new Vector3(-58 + 4 * Keys.Count, 29, 5);
                            obstacle.transform.localScale = Vector2.one * 3f;

                            var marker = Instantiate(MarkerPrefab);
                            marker.transform.parent = Camera.main.transform;
                            marker.transform.position = Camera.main.transform.position + new Vector3(-58 + 4 * Keys.Count, 29, 6);
                            marker.transform.localScale = Vector2.one * 4f;
                            marker.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, .1f);
                        };
                        Keys.Add(obstacle.GetComponent<SpriteRenderer>().color.ToString());

                        maze.RemoveObstacle(x, y);
                        break;
                    case "Door":
                        if (Keys.Contains(obstacle.GetComponent<SpriteRenderer>().color.ToString())) {
                            onFinishAnimate = () => {
                                Destroy(obstacle);
                            };
                            maze.RemoveObstacle(x, y);
                        }
                        else {
                            x -= dx;
                            y -= dy;
                        }
                        break;
                    case "Finish":
                        onFinishAnimate = () => {
                            SceneManager.LoadScene("WinScene");
                        };
                        break;
                }

                return;
            }
        }
    }
}