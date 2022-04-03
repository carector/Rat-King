using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public int points;
    public int requiredPoints;
    public bool timerRunning;
    public float startingTime = 120;
    public float runtime;

    public Transform chamberCenter;
    
    public bool levelOver;

    bool shredding;
    ParticleSystem blood;
    Transform shredSackHolder;
    Transform sack;
    GameManager gm;
    Animator anim;
    PlayerController ply;
    CameraFollow cam;

    Slider progressSlider;
    Text progressText;

    // Start is called before the first frame update
    void Start()
    {
        blood = GetComponentInChildren<ParticleSystem>();
        ply = FindObjectOfType<PlayerController>();
        cam = FindObjectOfType<CameraFollow>();
        cam.target = chamberCenter;
        anim = GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
        shredSackHolder = transform.GetChild(0).GetChild(0);

        runtime = startingTime;

        progressSlider = transform.GetChild(2).GetChild(0).GetComponent<Slider>();
        progressSlider.maxValue = requiredPoints;
        progressText = transform.GetChild(2).GetChild(1).GetComponent<Text>();
        progressText.text = points + " / " + requiredPoints;
    }

    // Update is called once per frame
    void Update()
    {
        if (runtime > 0)
        {
            if(timerRunning)
                runtime -= Time.deltaTime;
        }
        else
            runtime = 0;

        if (Input.GetKeyDown(KeyCode.E))
            GameOver();

        progressText.text = points + " / " + requiredPoints;
        progressSlider.value = Mathf.Lerp(progressSlider.value, points, 0.25f);

        if (!levelOver)
        {
            if (points >= requiredPoints)
                WinLevel();
            if (ply.p_states.dead)
                GameOver();
        }
    }

    void WinLevel()
    {
        levelOver = true;
        StartCoroutine(WinLevelSequence());
    }

    IEnumerator WinLevelSequence()
    {
        timerRunning = false;
        ply.p_states.canMove = false;
        yield return new WaitForSeconds(2);
        gm.ShowGameOverPopup(0);
    }

    IEnumerator GameOverSequence()
    {
        timerRunning = false;
        ply.p_states.canMove = false;
        yield return new WaitForSeconds(1);
        gm.ShowGameOverPopup(1);
    }

    void GameOver()
    {
        levelOver = true;
        StartCoroutine(GameOverSequence());
    }

    public void BeginLevel()
    {
        timerRunning = true;
        gm.PlayMusic(gm.gm_gameSfx.musicTracks[0]);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Sack" && !shredding)
        {
            StartCoroutine(ShredShake(collision.transform));
        }
    }

    public void StopShredding()
    {
        shredding = false;
        points += ply.p_sackVars.heldCivilians;
        ply.EmptySack();
        ply.p_sackVars.holdingSack = true;
    }

    IEnumerator ShredShake(Transform t)
    {
        sack = t;
        anim.Play("Shred", 0, 0);
        blood.Play();
        gm.PlaySFX(gm.gm_gameSfx.generalSfx[1]);
        shredding = true;
        sack.transform.parent = shredSackHolder;
        sack.GetComponent<SackScript>().gettingCrushed = true;
        Rigidbody2D sackRb = sack.GetComponent<Rigidbody2D>();
        sackRb.isKinematic = true;
        sackRb.velocity = Vector2.zero;
        sackRb.angularVelocity = 0;

        while (shredding)
        {
            shredSackHolder.transform.localPosition = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
            sack.transform.localPosition = Vector2.MoveTowards(sack.transform.localPosition, Vector2.zero, 0.1f);
            //sack.transform.rotation = Quaternion.Lerp(sack.transform.rotation, Quaternion.identity, 0.1f);
            yield return null;
        }
        Destroy(sack.gameObject);
    }
}
