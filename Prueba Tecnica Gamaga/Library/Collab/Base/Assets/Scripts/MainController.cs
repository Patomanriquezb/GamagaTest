using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    public int sizeToGenerate;
    private Labyrinth labyrinth;
    private LabyrinthGenerator gL;



    // Start is called before the first frame update
    void Start()
    {
        gL = GetComponent<LabyrinthGenerator>();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(100, 100, 200, 100), "Generate"))
            labyrinth = gL.GenerateLabyrinth(sizeToGenerate);

        if (GUI.Button(new Rect(100, 200, 200, 100), "Get Solution"))
        {
            if (labyrinth != null)
            {
                labyrinth.solution = PathFindingTool.FindThePath(labyrinth.blocksMatrix, labyrinth.entrance, labyrinth.exit);
                if (labyrinth.solution != null)
                {
                    Debug.LogWarning("Solution found!! :D");
                    Node currentNode = labyrinth.solution;
                    while (currentNode != null)
                    {
                        currentNode.b.solutionMarker.SetActive(true);
                        currentNode = currentNode.prevNode;
                    }
                }
            }
        }
    }
}
