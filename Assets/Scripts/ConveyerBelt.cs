using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerBelt : MonoBehaviour
{
    public int direction = 1;

    PlayerController ply;
    SackScript sack;

    // Start is called before the first frame update
    void Start()
    {
        ply = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player" && ply.p_states.grounded)
            ply.transform.position += Vector3.right * direction * 0.05f;
        else if(other.tag == "Sack")
        {
            if (sack == null)
                sack = FindObjectOfType<SackScript>();

            if(sack.grounded)
            sack.transform.position += Vector3.right * direction * 0.05f;
        }
    }
}
