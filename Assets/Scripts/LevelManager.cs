using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public int musicIndex = 0;
    public int points;
    public int requiredPoints;
    public bool timerRunning;
    public float startingTime = 30;
    public float runtime;

    public Transform chamberCenter;

    public bool levelOver;
    float lerpPoints;

    public bool shredding;
    ParticleSystem blood;
    Transform shredSackHolder;
    Transform sack;
    GameManager gm;
    Animator anim;
    PlayerController ply;
    CameraFollow cam;

    IEnumerator spawnRats;

    // Start is called before the first frame update
    void Start()
    {
        blood = GetComponentInChildren<ParticleSystem>();
        ply = FindObjectOfType<PlayerController>();
        cam = FindObjectOfType<CameraFollow>();
        cam.target = chamberCenter;
        anim = GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
        gm.SetScoreValue(0, requiredPoints);
        shredSackHolder = transform.GetChild(0).GetChild(0);

        runtime = startingTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (runtime > 0)
        {
            if (timerRunning && cam.target != chamberCenter && !shredding)
                runtime -= Time.deltaTime;
        }
        else
            runtime = 0;

        if (!shredding)
            lerpPoints = points;

        gm.SetScoreValueLerp(points, requiredPoints);

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
        if (gm.gm_gameSaveData.furthestUnlockedLevel + gm.gm_gameVars.startingLevelBuildIndex <= SceneManager.GetActiveScene().buildIndex)
            gm.UnlockNextLevel();

        ply.p_states.victory = true;
        gm.PlaySFX(gm.gm_gameSfx.playerSfx[6]);
        yield return new WaitForSeconds(1.5f);
        gm.ShowGameOverPopup(0);
    }

    IEnumerator GameOverSequence()
    {
        timerRunning = false;
        yield return new WaitForSeconds(2);
        gm.ShowGameOverPopup(1);
    }

    public IEnumerator SpawnRatsCoroutine()
    {
        while (runtime == 0)
        {
            Instantiate(gm.gm_gameRefs.ratPrefab, new Vector2(cam.transform.position.x + Random.Range(-14f, 28f), cam.transform.position.y + 13), Quaternion.identity);
            yield return new WaitForSeconds(0.35f);
        }
    }

    public void GameOver()
    {
        gm.ScreenShake(15);
        ply.Die();
        cam.target = null;
        levelOver = true;
        StartCoroutine(GameOverSequence());
    }

    public void BeginLevel()
    {
        timerRunning = true;
        gm.PlayMusic(gm.gm_gameSfx.musicTracks[musicIndex]);
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

        float extendedTime = runtime + (20 * ply.p_sackVars.heldCivilians);
        while (shredding)
        {
            shredSackHolder.transform.localPosition = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
            sack.transform.localPosition = Vector2.MoveTowards(sack.transform.localPosition, Vector2.zero, 0.2f);

            runtime = Mathf.MoveTowards(runtime, extendedTime, 0.35f * ply.p_sackVars.heldCivilians);
            //sack.transform.rotation = Quaternion.Lerp(sack.transform.rotation, Quaternion.identity, 0.1f);
            yield return new WaitForFixedUpdate();
        }
        runtime = extendedTime;
        Destroy(sack.gameObject);
    }
}
