using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (LabyrinthGenerator))]
public class Labyrinth : MonoBehaviour
{
    public int labyrinthSize;
    private LabyrinthGenerator gL;

    // Start is called before the first frame update
    void Start()
    {
        gL = GetComponent<LabyrinthGenerator>();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(100, 100, 200, 100), "Generate"))
            gL.GenerateLabyrinth(labyrinthSize);

        if (GUI.Button(new Rect(100, 300, 200, 100), "Fix Exit"))
            gL.SetExitBlock();
    }
}
