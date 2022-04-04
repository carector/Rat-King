using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ConveyerBelt : MonoBehaviour
{
    public int direction = 1;
    public float speedMultiplier = 1.5f;

    PlayerController ply;
    SackScript sack;
    List<Collider2D> cols;

    // Start is called before the first frame update
    void Start()
    {
        cols = new List<Collider2D>();
        ply = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (Collider2D c in cols)
            c.transform.position += Vector3.right * direction * 0.05f * speedMultiplier;

        cols = cols.Where(item => item != null).ToList();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Collider2D t = null;
        bool grounded = false;

        /*
        if (other.tag == "Player")
        {
            grounded = ply.p_states.grounded;
            t = other;
        }
        else */

        if (other.tag == "Sack")
        {
            if (sack == null)
                sack = FindObjectOfType<SackScript>();
            grounded = sack.grounded;
            t = other;
        }

        if (t != null)
        {
            bool contains = cols.Contains(other);
            if (grounded && !contains)
                cols.Add(other);
            else if (contains)
                cols.Remove(other);
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if ((other.tag == "Player" || other.tag == "Sack") && cols.Contains(other))
            cols.Remove(other);
    }
}
