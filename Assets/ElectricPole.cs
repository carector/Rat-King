using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricPole : MonoBehaviour
{
    public PressurePlate connectedButton;
    public bool inverted;

    Animator anim;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (connectedButton.pressed || (inverted && !connectedButton.pressed))
            gm.CheckAndPlayClip("ElectricPole_Off", anim);
        else if(!connectedButton.pressed || (inverted && connectedButton.pressed))
            gm.CheckAndPlayClip("ElectricPole_On", anim);
    }
}
