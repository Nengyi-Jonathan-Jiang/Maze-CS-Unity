using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlScript : MonoBehaviour
{
    Rigidbody2D rb;

    public float movementSpeed;
    
    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update() {
        Vector2 movement = new Vector2();
        if (Input.GetKey(KeyCode.W)) movement += new Vector2(0, 1);
        if (Input.GetKey(KeyCode.A)) movement += new Vector2(-1, 0);
        if (Input.GetKey(KeyCode.S)) movement += new Vector2(0, -1);
        if (Input.GetKey(KeyCode.D)) movement += new Vector2(1, 0);
        
        rb.AddForce(movement * Time.deltaTime * 1000 * movementSpeed);
    }
}