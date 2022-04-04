using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricPole : MonoBehaviour
{
    public bool activated;
    public bool invertSignal;
    public List<PressurePlate> connectedPlates;

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
        bool activatedUpdate = false;
        foreach(PressurePlate p in connectedPlates)
        {
            if(p.pressed)
            {
                activatedUpdate = true;
                break;
            }
        }

        activated = activatedUpdate;

        if ((activated && !invertSignal) || (invertSignal && !activated))
        {
            gm.CheckAndPlayClip("ElectricPole_Off", anim);
            col.enabled = false;
        }
        
        if ((!invertSignal && !activated) || (invertSignal && activated))
        {
            gm.CheckAndPlayClip("ElectricPole_On", anim);
            col.enabled = true;
        }
    }
}
