using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    private static PlayerController _i;
    public static PlayerController i { get { return _i; } }

    [System.Serializable]
    public class PlayerMovement
    {
        public float maxSpeed;
        public float acceleration;
        public float deceleration;
        public float midairAcceleration;
        public float midairDeceleration;
        public float jumpForce;
        public PhysicsMaterial2D[] mats;
    }

    [System.Serializable]
    public class SackVariables
    {
        public bool holdingSack;
        public int heldCivilians = 0;
        public GameObject sackObjectPrefab;
        public SackScript spawnedSack;
    }

    [System.Serializable]
    public class PlayerStates
    {
        public bool canMove;
        public bool grabbing;
        public bool throwing;
        public bool jumping;
        public bool grounded;
        public bool dead;
        public bool victory;
        public bool showingSackContents;
        public int facingDirection = 1;
    }

    public PlayerMovement p_movement;
    public SackVariables p_sackVars;
    public PlayerStates p_states;

    [Header("References")]
    [SerializeField]
    Transform handTransformHolder;
    [SerializeField]
    Transform handTransform;
    [SerializeField]
    SpriteRenderer[] aimNodes;
    [SerializeField]
    Rigidbody2D rb;
    [SerializeField]
    SpriteRenderer bodySprite;
    [SerializeField]
    Animator bodyAnimator;

    Vector2 lastPosition;
    Transform grabbedCivilian;

    public Vector3 mousePosition;

    void Awake()
    {
        if(_i != null)
        {
            Destroy(this.gameObject);
            return;
        }

        _i = this;
        DisplayAimingLine(false);
        lastPosition = transform.position;
    }

    bool CanMove()
    {
        return p_states.canMove && !p_states.throwing && !p_states.grabbing && !p_states.dead && !GameManager.i.gm_gameVars.gamePaused;
    }

    public void Die()
    {
        p_states.dead = true;
        GameManager.i.CheckAndPlayClip("Player_Die", bodyAnimator);
        GameManager.i.PlaySFX(GameManager.i.gm_gameSfx.playerSfx[7]);
        bodySprite.sortingLayerName = "Foreground";
        bodySprite.sortingOrder = 200;
        rb.velocity = Vector2.up * p_movement.jumpForce/2;
        GetComponent<CircleCollider2D>().enabled = false;
        for (int i = 0; i < 8; i++)
        {
            aimNodes[i].color = Color.clear;
        }
        StartCoroutine(DieSequence());
    }

    IEnumerator DieSequence()
    {
        yield return new WaitForSeconds(3);
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (!CanMove())
            return;

        if (p_sackVars.holdingSack)
            rb.mass = 1 + p_sackVars.heldCivilians;
        else
            rb.mass = 1;

        p_states.grounded = CheckIfGrounded();
        if(p_states.grounded)
            rb.sharedMaterial = p_movement.mats[0];
        else
            rb.sharedMaterial = p_movement.mats[1];

        UpdateInputButtons();

    }

    private void FixedUpdate()
    {
        UpdateAnimations();

        if (!CanMove())
        {
            MovePlayer(0);
            return;
        }

        UpdateInputAxes();

        Vector2 mouse = new Vector2(Mathf.Clamp(Input.mousePosition.x, 0, Screen.width), Mathf.Clamp(Input.mousePosition.y, 0, Screen.height));
        mousePosition = Camera.main.ScreenToWorldPoint(mouse);
    }

    Collider2D[] ScanForInteractions()
    {
        return Physics2D.OverlapCircleAll(transform.position + Vector3.down * 0.5f, 1.75f);
    }

    bool CheckIfGrounded()
    {
        //Debug.DrawRay(transform.position + Vector3.down, Vector2.down * 0.15f, Color.red);
        if (p_states.jumping && rb.velocity.y > 0)
            return false;
        else
            p_states.jumping = false;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position + Vector3.down, 0.4f, Vector2.down, 0.15f, ~(1 << 6));
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform != null && hit.transform.tag == "Ground")
                return true;
        }

        return false;
    }

    void UpdateInputAxes()
    {
        MovePlayer(Input.GetAxis("Horizontal"));
    }

    void UpdateAnimations()
    {
        if (p_states.grabbing || p_states.throwing || p_states.dead)
            return;

        string clipName = "Player_";
        bodyAnimator.SetFloat("WalkSpeed", Mathf.Abs(rb.velocity.x) / 10);
        bodyAnimator.SetFloat("FallSpeed", Mathf.Clamp(Mathf.Abs(rb.velocity.y) / 4, 0, 5));

        if (p_states.victory)
        {
            clipName += "Victory";
        }
        else
        {
            if (p_states.grounded)
            {
                if (p_states.canMove && Vector2.Distance(transform.position, lastPosition) > 0.1f)
                    clipName += "Walk";
                else
                    clipName += "Idle";
            }
            else
                clipName += "Fall";

            if (p_sackVars.holdingSack)
            {
                if (p_sackVars.heldCivilians <= 0)
                    clipName += "Empty";
                clipName += "Sack";
            }
        }

        GameManager.i.CheckAndPlayClip(clipName, bodyAnimator);
        lastPosition = transform.position;
    }

    void MovePlayer(float horizForce)
    {
        int horizSign = (int)Mathf.Sign(horizForce);
        float accel = p_movement.acceleration;
        float decel = p_movement.deceleration;

        if (horizForce != 0)
        {
            p_states.facingDirection = horizSign;
            bodySprite.flipX = (horizSign < 0);
        }
        if (!p_states.grounded)
        {
            accel = p_movement.midairAcceleration;
            decel = p_movement.midairDeceleration;
        }

        if (horizForce != 0)
        {
            float slowdownMultiplier = 1 / (0.75f + (rb.mass * 0.25f));

            if (Mathf.Abs(rb.velocity.x) >= p_movement.maxSpeed * slowdownMultiplier && horizSign == Mathf.Sign(rb.velocity.x))
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -p_movement.maxSpeed * slowdownMultiplier, p_movement.maxSpeed * slowdownMultiplier), rb.velocity.y);
            else
            {
                Vector3 force = Vector2.right * horizForce * accel * rb.mass;
                rb.AddForce(force);
            }
        }
        else
            rb.velocity = Vector2.MoveTowards(rb.velocity, new Vector2(0, rb.velocity.y), decel);
    }

    void UpdateInputButtons()
    {
        if (Input.GetKeyDown(KeyCode.Space) && p_states.grounded)
            Jump();

        if (!p_states.showingSackContents && p_sackVars.holdingSack && p_sackVars.heldCivilians > 0 && !p_states.dead)
        {
            if (Input.GetMouseButton(0))
                DisplayAimingLine(true);
            else
            {
                DisplayAimingLine(false);
                if (Input.GetMouseButtonUp(0))
                    ThrowSack();
            }
        }
        else DisplayAimingLine(false);

        Collider2D[] hits = ScanForInteractions();
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].tag == "Civilian" && p_sackVars.heldCivilians < 3 && p_sackVars.holdingSack)
            {
                hits[i].GetComponent<Civilian>().enabled = false;
                grabbedCivilian = hits[i].transform;
                string grabClip = "Player_Grab";
                if (p_sackVars.heldCivilians == 0)
                    grabClip += "Empty";
                grabClip += "Sack";
                GameManager.i.CheckAndPlayClip(grabClip, bodyAnimator);
                GameManager.i.PlaySFX(GameManager.i.gm_gameSfx.playerSfx[5]);
                p_states.grabbing = true;
                break;
            }
            if (hits[i].tag == "Sack" && p_sackVars.spawnedSack.grabbable && !p_sackVars.spawnedSack.gettingCrushed && !p_sackVars.spawnedSack.movingThroughChute)
            {
                p_sackVars.holdingSack = true;
                p_sackVars.spawnedSack = null;
                Destroy(hits[i].gameObject);
                break;
            }
        }
    }

    void ThrowSack()
    {
        Vector2 vertex = (transform.position + mousePosition) * 0.5f;
        vertex.y += 6;
        Vector2 vel = (BezierPoint(transform.position, mousePosition, vertex, 0.1f) - (Vector2)transform.position) * 15;

        GameManager.i.PlaySFX(GameManager.i.gm_gameSfx.playerSfx[4]);
        p_sackVars.spawnedSack = Instantiate(p_sackVars.sackObjectPrefab, transform.position, Quaternion.identity).GetComponent<SackScript>();
        p_sackVars.spawnedSack.Initialize(vel + Vector2.up * rb.velocity.y);
        p_sackVars.holdingSack = false;
        p_states.throwing = true;
        GameManager.i.CheckAndPlayClip("Player_Throw", bodyAnimator);
    }

    public void FinishThrowingAnimation()
    {
        p_states.throwing = false;
    }

    public void AttachCivilianToHand()
    {
        if (grabbedCivilian == null)
            return;
        handTransformHolder.localScale = new Vector2(p_states.facingDirection, 1);
        grabbedCivilian.transform.parent = handTransform;
        grabbedCivilian.localPosition = Vector3.zero;
        grabbedCivilian.localRotation = Quaternion.identity;
    }

    public void FinishGrabbingAnimation()
    {
        if (grabbedCivilian != null)
        {
            p_sackVars.heldCivilians++;
            Destroy(grabbedCivilian.gameObject);
            grabbedCivilian = null;
        }
        p_states.grabbing = false;
    }

    void Jump()
    {
        int index = 0;
        if (p_sackVars.holdingSack)
            index = p_sackVars.heldCivilians;

        GameManager.i.PlaySFX(GameManager.i.gm_gameSfx.playerSfx[index]);
        rb.velocity = new Vector2(rb.velocity.x, p_movement.jumpForce / (1 + ((rb.mass - 1) / 4.5f)));
        p_states.jumping = true;
    }

    void DisplayAimingLine(bool visible)
    {
        Color c = Color.white;
        if (!visible)
            c = Color.clear;
        Vector2 vertex = (transform.position + mousePosition) * 0.5f;
        vertex.y += 6;

        for (int i = 0; i < 8; i++)
        {
            aimNodes[i].transform.position = BezierPoint(transform.position, mousePosition, vertex, 0.1f + 0.12f * i);
            aimNodes[i].color = c;
        }
    }

    public void EmptySack()
    {
        p_sackVars.heldCivilians = 0;
    }

    public Vector2 BezierPoint(Vector2 start, Vector2 end, Vector2 vertex, float delta) // delta being a number between 0 and 1
    {
        // what?
        return Mathf.Pow((1 - delta), 2) * start + 2 * (1 - delta) * delta * vertex + Mathf.Pow(delta, 2) * end;
    }
}
