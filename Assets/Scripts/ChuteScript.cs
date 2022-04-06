using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuteScript : MonoBehaviour
{
    public Animator exit;
    public Vector2 exitVelocity;

    Animator anim;
    SackScript sack;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();
        gm.CheckAndPlayClip("ChuteIn_Default", anim);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ChuteTransfer()
    {
        sack.rb.isKinematic = true;
        sack.rb.velocity = Vector2.zero;
        while(sack.transform.position != transform.position)
        {
            sack.transform.position = Vector2.MoveTowards(sack.transform.position, transform.position, 0.5f);
            yield return new WaitForFixedUpdate();
        }

        
        gm.CheckAndPlayClip("ChuteIn_Bounce", anim);
        yield return new WaitForSeconds(0.05f);
        sack.transform.position = transform.position + Vector3.down * 100;
        yield return new WaitForSeconds(0.5f);
        gm.CheckAndPlayClip("ChuteOut_Bounce", exit);
        sack.transform.position = exit.transform.position;
        sack.rb.isKinematic = false;
        sack.rb.velocity = exitVelocity;
        yield return new WaitForSeconds(0.5f);
        sack.movingThroughChute = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Sack")
        {
            if(sack == null)
                sack = collision.GetComponent<SackScript>();
            if (!sack.movingThroughChute)
            {
                sack.movingThroughChute = true;
                StartCoroutine(ChuteTransfer());
            }
        }
    }
}
