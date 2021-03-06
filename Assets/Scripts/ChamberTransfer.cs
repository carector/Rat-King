using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChamberTransfer : MonoBehaviour
{
    public bool isExit;
    public bool disappearAfterUsing;
    public ChamberTransfer destination;
    public Transform spawnPosition;
    public bool inBounds;

    bool canEnter = true;
    bool playingIntroAnimation;

    [HideInInspector]
    public Animator anim;
    PlayerController ply;
    Rigidbody2D prb;
    LevelManager lm;
    CameraFollow cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = FindObjectOfType<CameraFollow>();
        anim = GetComponent<Animator>();
        lm = FindObjectOfType<LevelManager>();
        ply = FindObjectOfType<PlayerController>();
        prb = ply.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inBounds && canEnter && Input.GetKeyDown(KeyCode.W) && !lm.levelOver && !playingIntroAnimation && !(isExit && lm.points + ply.p_sackVars.heldCivilians >= lm.requiredPoints && ply.p_sackVars.spawnedSack != null && !ply.p_sackVars.spawnedSack.grounded))
        {
            if (isExit)
            {
                if (!lm.timerRunning)
                {
                    playingIntroAnimation = true;
                    StartCoroutine(InitialPlayerSpawn());
                }
                else
                {
                    ply.transform.position = destination.spawnPosition.position;
                    cam.target = ply.transform;
                }
            }
            else
            {
                ply.transform.position = destination.spawnPosition.position;
                cam.target = lm.chamberCenter;
                destination.destination = this;
            }
            prb.velocity = Vector2.zero;
        }
    }

    IEnumerator InitialPlayerSpawn()
    {
        ply.p_states.canMove = false;
        prb.velocity = Vector2.zero;
        destination.anim.Play("TransferDoorAppear");
        yield return new WaitForEndOfFrame();
        cam.target = destination.spawnPosition;
        yield return new WaitForSeconds(1.5f);
        ply.transform.position = destination.spawnPosition.position;
        cam.target = ply.transform;
        ply.p_states.canMove = true;
        lm.timerRunning = true;
        playingIntroAnimation = false;
        lm.BeginLevel();
        if (destination.disappearAfterUsing)
            destination.WaitAndFade();
    }

    public void WaitAndFade()
    {
        canEnter = false;
        StartCoroutine(WaitForExitBounds());
    }

    IEnumerator WaitForExitBounds()
    {
        yield return new WaitForSeconds(0.5f);
        while (inBounds)
        {
            print("Waiting");
            yield return null;
        }
        anim.Play("TransferDoorDisappear");
        
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
            inBounds = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            inBounds = false;
        }
    }
}
