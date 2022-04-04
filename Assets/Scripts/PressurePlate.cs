using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    PlayerController ply;
    SackScript sack;
    Rigidbody2D sackRb;
    GameManager gm;
    Animator anim;

    public bool pressed;
    public List<FragileRockScript> connectedRocks;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        ply = FindObjectOfType<PlayerController>();
        gm = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        pressed = CheckIfPressed();

        string clipName = "PressurePlate_";
        if (pressed)
            clipName += "Pressed";
        else
            clipName += "Released";




        gm.CheckAndPlayClip(clipName, anim);
    }
    bool CheckIfPressed()
    {
        bool p = false;
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position + Vector3.up * 2, new Vector2(1.5f, 5), 0);
        foreach (Collider2D other in cols)
        {
            if (other.tag == "Player")
            {
                if (ply.p_states.grounded)
                    p = true;
            }
            if (other.tag == "Sack")
            {
                if (sackRb == null)
                {
                    sack = FindObjectOfType<SackScript>();
                    sackRb = sack.GetComponent<Rigidbody2D>();
                }

                if (!sack.grounded)
                    sackRb.velocity = Vector2.MoveTowards(sackRb.velocity, new Vector2(0, sackRb.velocity.y), 0.1f);
                else
                    p = true;
            }
            if (p == true)
                break;
        }

        bool pRock = false;
        for(int i = 0; i < connectedRocks.Count; i++)
        {
            if (!connectedRocks[i].broken)
            {
                pRock = true;
                break;
            }
        }

        return p || pRock;
    }
}
