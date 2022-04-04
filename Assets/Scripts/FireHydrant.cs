using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireHydrant : MonoBehaviour
{
    public bool active;
    public float force = 20;
    Rigidbody2D prb;
    Rigidbody2D sackRb;

    // Start is called before the first frame update
    void Start()
    {
        prb = FindObjectOfType<PlayerController>().GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (active)
        {
            if (other.tag == "Player")
            {
                print("AddingForce");
                prb.AddForce((Vector2.up * force) / Vector2.Distance(transform.position, prb.transform.position));
            }
            if (other.tag == "Sack")
            {
                if (sackRb == null)
                    sackRb = other.GetComponent<Rigidbody2D>();
                sackRb.AddForce((Vector2.up * force) / Vector2.Distance(transform.position, sackRb.transform.position));
            }
        }
    }
}
