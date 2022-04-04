using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialText : MonoBehaviour
{
    public bool requiresCivilian;
    
    PlayerController ply;
    Vector2 startingPos;

    // Start is called before the first frame update
    void Start()
    {
        ply = FindObjectOfType<PlayerController>();
        startingPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if ((requiresCivilian && ply.p_sackVars.heldCivilians > 0) || !requiresCivilian)
            transform.position = startingPos;
        else
            transform.position = startingPos + Vector2.down * 100;
    }
}
