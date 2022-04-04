using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricPole : MonoBehaviour
{
    public PressurePlate connectedButton;
    public bool inverted;

    Animator anim;
    GameManager gm;
    Collider2D col;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (connectedButton.pressed || (inverted && !connectedButton.pressed))
        {
            gm.CheckAndPlayClip("ElectricPole_Off", anim);
            col.enabled = false;
        }
        else if (!connectedButton.pressed || (inverted && connectedButton.pressed))
        {
            gm.CheckAndPlayClip("ElectricPole_On", anim);
            col.enabled = true;
        }
    }
}
