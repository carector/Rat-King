using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(target != null)
            transform.position = new Vector3(target.position.x, target.position.y, -10);
    }

    public void DestroyParallax()
    {
        if (transform.childCount > 1)
            Destroy(transform.GetChild(1).gameObject);
    }
}
