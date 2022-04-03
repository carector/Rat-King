using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Civilian : MonoBehaviour
{
    public CivilianData data;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer bodyBaseSpr;
    protected GameManager gm;

    protected void GetReferences()
    {
        gm = FindObjectOfType<GameManager>();
        bodyBaseSpr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateAppearance(CivilianData data)
    {
        this.data = data;
        UpdateAppearance();
    }
    public void UpdateAppearance()
    {
        if (data == null)
        {
            bodyBaseSpr.color = Color.clear;
            return;
        }

        bodyBaseSpr.color = Color.white;
        bodyBaseSpr.sprite = data.baseSprite;
    }
}
