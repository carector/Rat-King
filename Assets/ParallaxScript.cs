using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScript : MonoBehaviour
{
    CameraFollow cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = FindObjectOfType<CameraFollow>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        transform.parent = cam.transform;
        transform.localPosition = new Vector3(0, 0, 10);
    }
}
