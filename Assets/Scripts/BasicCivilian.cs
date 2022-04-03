using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCivilian : Civilian
{
    public string animationPrefix;

    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
        UpdateAppearance();
        StartCoroutine(IdleCycle());
        gm.CheckAndPlayClip(animationPrefix + "_Default", anim);
    }

    IEnumerator IdleCycle()
    {
        yield return new WaitForSeconds(Random.Range(4, 8));
        gm.CheckAndPlayClip(animationPrefix + "_Idle", anim);
        StartCoroutine(IdleCycle());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
