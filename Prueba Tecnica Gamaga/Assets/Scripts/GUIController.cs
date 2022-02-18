using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{
    public TMP_Text labyrinthSizeText;
    public Slider labyrinthSizeSlider;
    public GameObject mobileButtons;

    private void Start()
    {
        //#if unity_android (scripting symbol not showing)
        //mobileButtons.SetActive(true);
        //#endif

    }

    private void Update()
    {
        labyrinthSizeText.text = "Labyrinth size = " + labyrinthSizeSlider.value;
    }

    public void Quit()
    {
        Application.Quit();
    }


}
