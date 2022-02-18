using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    public int sizeToGenerate;

    private Labyrinth labyrinth;
    private LabyrinthGenerator gL;

    public PlayerController player;
    private Camera mainCamera;

    public GUIController guiControl;

    // Start is called before the first frame update
    void Start()
    {
        gL = GetComponent<LabyrinthGenerator>();
        mainCamera = Camera.main;
    }

    public void GenerateLabyrinth()
    {
        if (!mainCamera.gameObject.activeInHierarchy)
            ChangeCamera();

        labyrinth = gL.GenerateLabyrinth(sizeToGenerate);
        player.Reset();
        player.MoveToPoint(labyrinth.entrance.transform.position + Vector3.up * 2);
        player.SetPhysicsActive(true);
    }

    public void ShowSolution()
    {
        if (labyrinth != null)
        {
            if (labyrinth.solution != null)
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
            else
            {
                Debug.LogError("There's no solution for this Labyrinth.");
            }
        }
    }

    public void ChangeCamera()
    {
        if (mainCamera.gameObject.activeInHierarchy)
        {
            mainCamera.gameObject.SetActive(false);
            player.camera.SetActive(true);
        }
        else
        {
            mainCamera.gameObject.SetActive(true);
            player.camera.SetActive(false);
        }
    }

    public void UpdateSizeToGenerate()
    {
        sizeToGenerate = (int)guiControl.labyrinthSizeSlider.value;
    }
}
