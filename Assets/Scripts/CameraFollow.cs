using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;
    public float cameraFollowSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 delta = target.transform.position - transform.position;
        if (delta.magnitude < 0.001f) {
            transform.position += (Vector3) delta;
        }
        else {
            Vector2 movement = delta * (1 - Mathf.Exp(-Time.deltaTime * cameraFollowSpeed));
            transform.position += (Vector3) movement;
        }
    }
}