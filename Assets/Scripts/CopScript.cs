using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopScript : MonoBehaviour
{
    public bool noticedPlayer;
    public float speed;
    public float walkSpeed;
    public float runSpeed;
    public float leftBound;
    public float rightBound;
    public int facingDirection;
    public bool startFlipped;
    public float shootingDistance;
    public float timeBeforeForget = 4;

    bool flipped;
    float timeSinceSeen;
    PlayerController ply;
    GameManager gm;
    Animator anim;
    SpriteRenderer spr;
    LevelManager lm;

    // Start is called before the first frame update
    void Start()
    {
        lm = FindObjectOfType<LevelManager>();
        ply = FindObjectOfType<PlayerController>();
        gm = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();
        spr = transform.GetChild(0).GetComponent<SpriteRenderer>();

        StartCoroutine(WalkCycle());
    }

    void Move()
    {
        anim.SetFloat("WalkSpeed", speed / walkSpeed);
        transform.position += Vector3.right * speed * 0.025f * facingDirection;
    }

    IEnumerator WalkCycle()
    {
        // Walk right
        while (transform.position.x < rightBound && !flipped && !ply.p_states.dead)
        {
            gm.CheckAndPlayClip("Cop_Walk", anim);
            Move();
            yield return new WaitForFixedUpdate();

            if (noticedPlayer)
            {
                if (WithinShootingDistance())
                {
                    yield return Shoot();
                    break;
                }
                if (ForceFlip())
                    break;
            }
        }


        if (!noticedPlayer)
        {
            transform.position = new Vector2(rightBound, transform.position.y);
            yield return WaitBeforeFlipping();
        }
        flipped = true;

        // Walk left
        while (transform.position.x > leftBound && flipped && !ply.p_states.dead)
        {
            gm.CheckAndPlayClip("Cop_Walk", anim);
            Move();
            yield return new WaitForFixedUpdate();

            if (noticedPlayer)
            {
                if (WithinShootingDistance())
                {
                    yield return Shoot();
                    break;
                }
                if (ForceFlip())
                    break;
            }
        }

        if (!noticedPlayer)
        {
            transform.position = new Vector2(leftBound, transform.position.y);
            yield return WaitBeforeFlipping();
        }
        flipped = false;

        if (!ply.p_states.dead)
            StartCoroutine(WalkCycle());
        else
            gm.CheckAndPlayClip("Cop_Default", anim);
    }

    IEnumerator WaitBeforeFlipping()
    {
        gm.CheckAndPlayClip("Cop_Default", anim);
        yield return new WaitForSeconds(1);
    }

    IEnumerator Shoot()
    {
        gm.CheckAndPlayClip("Cop_Shoot", anim);
        yield return new WaitForSeconds(1);
    }

    public void PlayGunSound()
    {
        gm.PlaySFX(gm.gm_gameSfx.generalSfx[3]);
    }
    public void KillPlayer()
    {
        lm.GameOver();
    }

    bool ForceFlip()
    {
        float distance = (ply.transform.position - transform.position).x;
        print(distance);
        return (Mathf.Abs(distance) > 8 && Mathf.Sign(distance) != facingDirection);
    }

    bool WithinShootingDistance()
    {
        float distance = Mathf.Abs((transform.position - ply.transform.position).x);
        if (distance <= shootingDistance)
            return true;
        return false;
    }

    bool SeesSuspiciousActivity()
    {
        bool hit = PlayerHitFromRaycast();
        if (noticedPlayer)
        {
            if (hit)
                timeSinceSeen = 0;
            timeSinceSeen += Time.deltaTime;
            if (timeSinceSeen > timeBeforeForget || (ply.transform.position.x < leftBound || ply.transform.position.x > rightBound))
                return false;
            return true;
        }
        else if (hit && ply.p_sackVars.heldCivilians > 0 && ply.p_sackVars.holdingSack)
            return true;

        return false;
    }

    bool PlayerHitFromRaycast()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position + new Vector3(8 * facingDirection, 1), new Vector2(13, 6), 0);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].tag == "Player")
                return true;
        }
        return false;
    }

    public void PlayStompSound()
    {
        if (noticedPlayer)
            gm.PlaySFXStoppable(gm.gm_gameSfx.generalSfx[4]);
    }

    public void PlayCockGunSound()
    {
        gm.PlaySFX(gm.gm_gameSfx.generalSfx[5]);
    }

    // Update is called once per frame
    void Update()
    {
        noticedPlayer = SeesSuspiciousActivity();

        if (noticedPlayer)
            speed = runSpeed;
        else
            speed = walkSpeed;

        if (flipped)
            facingDirection = -1;
        else
            facingDirection = 1;

        if (!ply.p_states.dead)
            spr.flipX = flipped;
    }
}
