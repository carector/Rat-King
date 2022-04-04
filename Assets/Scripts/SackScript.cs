using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SackScript : MonoBehaviour
{
    public bool grabbable;
    public bool gettingCrushed;
    public PhysicsMaterial2D[] mats;
    
    [HideInInspector]
    public Rigidbody2D rb;
    PlayerController ply;
    Animator anim;
    GameManager gm;

    public bool grounded;
    public bool movingThroughChute;
    bool impactDelayInProgress;

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

    IEnumerator ImpactDelayInProgress()
    {
        yield return null;
        //yield return new WaitForSeconds(0.5f);
        impactDelayInProgress = false;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position + Vector3.down, 0.35f, Vector2.down, 0.25f, ~(1 << 7));
        Debug.DrawRay(transform.position + Vector3.down, Vector2.down * 0.25f, Color.red, Time.deltaTime);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform != null)
            {
                if (hit.transform.tag == "Ground" && !gettingCrushed && grabbable && !movingThroughChute)
                {
                    if (!impactDelayInProgress && !grounded)
                    {
                        gm.PlaySFX(gm.gm_gameSfx.generalSfx[0]);
                        grounded = true;
                        rb.angularVelocity = 0;
                        rb.freezeRotation = true;
                        transform.rotation = Quaternion.identity;
                        anim.Play("Sack_Impact", 0, 0);
                        impactDelayInProgress = true;
                        rb.sharedMaterial = mats[0];
                        StartCoroutine(ImpactDelayInProgress());
                    }
                    return;
                }
            }
        }

        if (grounded)
        {
            grounded = false;
            rb.sharedMaterial = mats[1];
            anim.Play("Sack_Midair", 0, 0);
        }
    }

}