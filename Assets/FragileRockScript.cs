using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragileRockScript : MonoBehaviour
{
    Collider2D col;
    PlayerController ply;
    GameManager gm;
    Animator anim;

    bool broken;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();
        ply = FindObjectOfType<PlayerController>();
        col = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Sack" && ply.p_sackVars.heldCivilians >= 1 && !broken)
        {
            broken = true;
            col.enabled = false;
            gm.CheckAndPlayClip("FragileRock_Break", anim);
            gm.ScreenShake();
            gm.PlaySFXStoppable(gm.gm_gameSfx.generalSfx[2]);
        }
    }
}
