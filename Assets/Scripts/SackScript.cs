using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SackScript : MonoBehaviour
{
    public bool grabbable;
    public bool gettingCrushed;
    Rigidbody2D rb;
    PlayerController ply;
    Animator anim;
    GameManager gm;

    public bool grounded;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InitialPickupDelay());
    }

    public void Initialize(Vector2 velocity)
    {
        GetReferences();
        rb.velocity = velocity;
        rb.angularVelocity = 275 * ply.p_states.facingDirection;
        rb.mass = ply.p_sackVars.heldCivilians;
    }
    void GetReferences()
    {
        gm = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();
        ply = FindObjectOfType<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    IEnumerator InitialPickupDelay()
    {
        yield return new WaitForSeconds(0.33f);
        grabbable = true;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.down, Vector2.down, 0.75f, ~(1 << 7));
        if (hit.transform != null)
        {
            if (hit.transform.tag == "Ground" && !grounded && !gettingCrushed && grabbable)
            {
                gm.PlaySFX(gm.gm_gameSfx.generalSfx[0]);
                grounded = true;
                rb.angularVelocity = 0;
                rb.freezeRotation = true;
                transform.rotation = Quaternion.identity;
                anim.Play("Sack_Impact", 0, 0);
            }
            else if (hit.transform.tag != "Ground" && grounded)
            {
                grounded = false;
                anim.Play("Sack_Midair", 0, 0);
            }
        }
        else if(grounded && hit.transform == null)
        {
            grounded = false;
            anim.Play("Sack_Midair", 0, 0);
        }

    }
}
