using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts {
    public class PlayerControlScript : MonoBehaviour {
        private Rigidbody2D _rb;

        public float MovementSpeed;

        public HashSet<string> Keys;
        
        // Start is called before the first frame update
        private void Start() {
            _rb = GetComponent<Rigidbody2D>();
            Keys = new HashSet<string>();
        }

        // Update is called once per frame
        private void Update() {
            var movement = new Vector2();
            if (Input.GetKey(KeyCode.W)) movement += new Vector2(0, 1);
            if (Input.GetKey(KeyCode.A)) movement += new Vector2(-1, 0);
            if (Input.GetKey(KeyCode.S)) movement += new Vector2(0, -1);
            if (Input.GetKey(KeyCode.D)) movement += new Vector2(1, 0);
            
            _rb.AddForce(movement * Time.deltaTime * 1000 * MovementSpeed);
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            var o = collision.gameObject;
            var color = o.GetComponent<SpriteRenderer>().color.ToString();
            switch (o.name) {
                case "Key":
                    Keys.Add(color);
                    Destroy(o);
                    break;
                case "Door":
                    if (Keys.Contains(color)) {
                        Destroy(o);
                    }
                    break;
                case "Finish":
                    SceneManager.LoadScene("WinScene");
                    break;
            }
        }
    }
}