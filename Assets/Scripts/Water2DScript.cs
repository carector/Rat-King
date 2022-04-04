// 2016 - Damien Mayance (@Valryon)
// Source: https://github.com/valryon/water2d-unity/
// modified slightly
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Water surface script (update the shader properties).
/// </summary>
public class Water2DScript : MonoBehaviour
{
    public Vector2 speed = new Vector2(0.01f, 0f);
    
    private Renderer rend;
    private Material mat;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
        StartCoroutine(WaterScrollDelay());
    }

    void LateUpdate()
    {
        //Vector2 scroll = Time.deltaTime * speed;

        //mat.mainTextureOffset += scroll;
    }

    IEnumerator WaterScrollDelay()
    {
        float delay = 0.05f;
        yield return new WaitForSeconds(delay);

        Vector2 scroll = speed * delay;

        mat.mainTextureOffset += scroll;
        StartCoroutine(WaterScrollDelay());
    }

    
}