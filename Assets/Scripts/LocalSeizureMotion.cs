using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalSeizureMotion : MonoBehaviour
{
    public bool usesRectTransform;
    public float uiShakeAmount = 10;
    RectTransform t;

    Vector2 initialPos;
    // Start is called before the first frame update
    void Start()
    {
        if (usesRectTransform)
        {
            t = GetComponent<RectTransform>();
            initialPos = t.anchoredPosition;
        }
        else
        {
            initialPos = transform.localPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 rand = new Vector2(Random.Range(-1, 1f), Random.Range(-1, 1f));

        if (usesRectTransform)
            t.anchoredPosition = initialPos + rand * uiShakeAmount;
        else
            transform.localPosition = initialPos + rand * 0.25f;
    }
}
