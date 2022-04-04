using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumblyRockScript : MonoBehaviour
{
    Collider2D col;
    Animator anim;
    GameManager gm;
    PlayerController ply;

    // Start is called before the first frame update
    void Start()
    {
        ply = FindObjectOfType<PlayerController>();
        col = GetComponent<Collider2D>();
        gm = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();
    }

    public void PlayCrumbleSound()
    {
        gm.PlaySFXStoppable(gm.gm_gameSfx.generalSfx[6]);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.position.y > transform.position.y && (collision.tag == "Player" || collision.tag == "Sack"))
        {
            if ((ply.p_sackVars.heldCivilians > 0 && ply.p_sackVars.holdingSack && collision.tag == "Player") || (ply.p_sackVars.heldCivilians > 1 && collision.tag == "Sack"))
                gm.CheckAndPlayClip("CrumblyRock_BreakFast", anim);
            else
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
