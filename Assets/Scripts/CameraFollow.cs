using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public int ppu = 16;
    float ppuInverse;

    // Start is called before the first frame update
    void Start()
    {
        ppuInverse = 1 / ppu;
    }


    Vector3 mouseLerpPos;
    private void FixedUpdate()
    {
        mouseLerpPos = Vector3.Lerp(mouseLerpPos, (PlayerController.i.mousePosition - target.position) * 0.25f, 0.5f);
    }


    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 pos = target.position + mouseLerpPos;
            //float roundedX = Mathf.RoundToInt(pos.x * ppu) * ppuInverse;
            //float roundedY = Mathf.RoundToInt(pos.y * ppu) * ppuInverse;
            transform.position = new Vector3(pos.x, pos.y, -10);
        }
    }

    public void DestroyParallax()
    {
        if (transform.childCount > 1)
            Destroy(transform.GetChild(1).gameObject);
    }
}
