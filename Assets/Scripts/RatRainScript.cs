using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatRainScript : MonoBehaviour
{
    public float fallSpeed = 4;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += new Vector3(-1, -1) * 0.025f * fallSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            LevelManager l = FindObjectOfType<LevelManager>();
            if (!l.levelOver)
            {
                GameManager gm = FindObjectOfType<GameManager>();
                gm.PlaySFX(gm.gm_gameSfx.generalSfx[4]);
                l.GameOver();
            }
        }
    }
}
