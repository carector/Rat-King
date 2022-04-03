using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CreditsNameMouseOver : MonoBehaviour
{
    public string url;
        public bool mouseOver;
    GameManager gm;
    TextMeshProUGUI text;
    AudioSource sfx;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseEnter()
    {
        mouseOver = true;
        text.color = Color.red;
        int rand = Random.Range(0, 3);
        gm.PlaySFX(gm.gm_gameSfx.uiSfx[rand]);

    }

    private void OnMouseExit()
    {
        text.color = Color.white;
        mouseOver = false;
    }

    public void OpenCreditsLink()
    {
        if (url == "")
            return;
        Application.OpenURL(url);
    }
}
