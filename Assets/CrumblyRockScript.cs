using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumblyRockScript : MonoBehaviour
{
    Collider2D col;
    Animator anim;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider2D>();
        gm = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.position.y > transform.position.y && (collision.tag == "Player" || collision.tag == "Sack"))
        {
            gm.CheckAndPlayClip("CrumblyRock_Break", anim);
        }
    }

    public void SetColliderState(int enabled)
    {
        if (enabled == 0)
            col.enabled = true;
        else
            col.enabled = false;
    }
}
