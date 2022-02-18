using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabyrinthGenerator : MonoBehaviour
{
    [SerializeField]
    private List<Block> entranceBlockTypes;

    [SerializeField]
    private List<Block> exitBlockTypes;

    [SerializeField]
    private List<Block> allBlockTypes;

    public bool includeEntranceInAll = false;
    public bool includeExitInAll = false;


    private Dictionary<BlockTypes,List<Block>> availableBlocks;
    private GameObject availableBlocksContainer;
    public int availableBlocksAmount { get; private set; }  = 0;
    private Dictionary<BlockTypes, List<Block>> usedBlocks;
    private GameObject usedBlocksContainer;

    [SerializeField]
    private float initialBlocksFactor = 1f;

    private bool isInitialized = false;
    public int LabyrinthSize { get; private set; }

    Block[,] blocksMatrix;
    Block entranceBlock;
    Block exitBlock;

    public void InitializeLabyrinthGenerator(int size)
    {
        if (initialBlocksFactor > 1)
            initialBlocksFactor = 1;

        if (exitBlockTypes.Count == 0 || exitBlockTypes == null)
        {
            if (exitBlockTypes == null)
            {
                exitBlockTypes = new List<Block>();
            }
            Debug.LogWarning("I need at least one possible exit block!");
            return;
        }

        if (entranceBlockTypes.Count == 0 || entranceBlockTypes == null)
        {
            if (entranceBlockTypes == null)
            {
                entranceBlockTypes = new List<Block>();
            }
            Debug.LogWarning("I need at least one possible entrance block!");
            return;
        }

        if (includeEntranceInAll)
        {
            if (allBlockTypes == null)
            {
                allBlockTypes = new List<Block>();
            }
            foreach (Block b in entranceBlockTypes)
            {
                allBlockTypes.Add(b);
            }
        }

        if (includeExitInAll)
        {
            if (allBlockTypes == null)
            {
                allBlockTypes = new List<Block>();
            }
            foreach (Block b in exitBlockTypes)
            {
                allBlockTypes.Add(b);
            }
        }

        if (allBlockTypes.Count == 0)
        {
            Debug.LogWarning("I don't have any block types to work with...");
            return;
        }

        if(availableBlocksContainer == null)
            availableBlocksContainer = new GameObject("Availible Blocks");
        if (usedBlocksContainer == null)
            usedBlocksContainer = new GameObject("Used Blocks");

        if (availableBlocks == null)
            availableBlocks = new Dictionary<BlockTypes, List<Block>>();
        if (usedBlocks == null)
            usedBlocks = new Dictionary<BlockTypes, List<Block>>();

        foreach (Block b in allBlockTypes)
        {
            List<Block> bInstances = new List<Block>();
            int blockAmount = Mathf.CeilToInt(size * size * initialBlocksFactor);

            for(int i = blockAmount; i>0; i--)
            {
                Block instance = Instantiate(b, new Vector3(-999, -999, -999), Quaternion.identity, availableBlocksContainer.transform);
                bInstances.Add(instance);
                instance.Generator = this;
                availableBlocksAmount++;
            }
            availableBlocks.Add(b.BlockType, bInstances);
            usedBlocks.Add(b.BlockType, new List<Block>());
        }

        blocksMatrix = new Block[LabyrinthSize, LabyrinthSize];
        isInitialized = true;
    }

    public Labyrinth GenerateLabyrinth(int size)
    {
        //David Bowie is working on it

        if(size < 2)
        {
            Debug.LogError("The Labyrinth's size must be at least 2. Setting size to minimum value.");
            size = 2;
        }
        if (size > 50)
        {
            Debug.LogError("The Labyrinth's size can't be larger than 50. Setting size to maximum value.");
            size = 50;
        }
        LabyrinthSize = size;
        if (!isInitialized)
            InitializeLabyrinthGenerator(size);
        if (!isInitialized)
        {
            Debug.LogError("The Labyrinth could not be initialized.");
            return null;
        }

        ResetLabyrinth();

        //Starting at (0,0,0)
        SetEntranceBlock(Vector2Int.zero);

        SetExitBlock();

        //Adjust camera to fit labyrinth on screen
        Camera.main.transform.position = new Vector3(((float)LabyrinthSize-1) * 3 / 2, 1, ((float)LabyrinthSize-1) * 3 / 2);
        Camera.main.orthographicSize = LabyrinthSize*1.7f;

        return new Labyrinth(blocksMatrix, entranceBlock, exitBlock);
    }

    public void SetEntranceBlock(Vector2Int entranceBlockCoordinates)
    {
        Block randomEntranceBlock = GetRandomBlockFromTypeList(entranceBlockTypes);
        randomEntranceBlock.UseBlock(entranceBlockCoordinates, true, true); //This sets in motion the recursive construction of the Labyrinth

        entranceBlock = randomEntranceBlock;
    }

    public void SetExitBlock() //Call this ONLY after generating the Labyrinth
    {
        //Debug.LogWarning("----- SETTING EXIT BLOCK -----");
        Block randomExitBlock = GetRandomBlockFromTypeList(exitBlockTypes);
        Vector2Int farthestCornerCoordinates = new Vector2Int(LabyrinthSize - 1, LabyrinthSize - 1);
        Block farthestCornerBlock = blocksMatrix[farthestCornerCoordinates.x, farthestCornerCoordinates.y];

        if (farthestCornerBlock != null)
        {
            farthestCornerBlock.ResetBlock();
        }

        randomExitBlock.UseBlock(farthestCornerCoordinates, true, false); //This generates adjacent blocks if needed
        exitBlock = randomExitBlock;

        FixLabyrinthExit(randomExitBlock);
    }

    public void FixLabyrinthExit(Block exitBlock)
    {
        if (PathFindingTool.FindThePath(blocksMatrix, entranceBlock, exitBlock) == null)
        {
            if (exitBlock == null)
                return;
            exitBlock.ForceAdjacentConnection(true); //This forces connection to adjacent blocks recursively

            if (PathFindingTool.FindThePath(blocksMatrix, entranceBlock, exitBlock) == null)
            {
                Debug.LogWarning("No solution, generating new labyrinth...");
                GenerateLabyrinth(LabyrinthSize);
            }
        }
    }

    public Block GetRandomBlockFromTypeList(List<Block> targetList) //Select random initial block
    {
        int randType = Random.Range(0, targetList.Count);
        BlockTypes selectedType = targetList[randType].BlockType;

        //If it's already in the instanced available blocks, then take it from there
        if (availableBlocks.ContainsKey(selectedType))
            if (availableBlocks[selectedType].Count > 0)
                return availableBlocks[selectedType][0];        
        
        return Instantiate(targetList[randType], new Vector3(-999, -999, -999), Quaternion.identity);
    }

    public Block GetBlockByType(BlockTypes desiredType)
    {
        //If it's already in the instanced available blocks, then take it from there
        if (availableBlocks.ContainsKey(desiredType))
            if(availableBlocks[desiredType].Count > 0)
                return availableBlocks[desiredType][0];

        Block desiredBlockPrefab = allBlockTypes.Find(x => x.BlockType == desiredType);
        if (desiredBlockPrefab != null)
            return Instantiate(allBlockTypes.Find(x => x.BlockType == desiredType), new Vector3(-999, -999, -999), Quaternion.identity);
        else
            return null;
    }

    public Block GetRandomAvailableBlock()
    {
        if (availableBlocksAmount <= 0)
        {
            Debug.LogWarning("I ran out of blocks :(");
            return null;
        }

        BlockTypes randType = allBlockTypes[Random.Range(0, allBlockTypes.Count)].BlockType;

        if (availableBlocks[randType].Count > 0)
            return availableBlocks[randType][0];
        else
            return GetRandomAvailableBlock();
    }

    public bool RegisterBlockInMatrix(Block block)
    {
        if (blocksMatrix[(int)block.BlockCoordinates.x, (int)block.BlockCoordinates.y] == null)
        {
            blocksMatrix[(int)block.BlockCoordinates.x, (int)block.BlockCoordinates.y] = block;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RemoveBlockFromMatrix(Block block)
    {
        blocksMatrix[(int)block.BlockCoordinates.x, (int)block.BlockCoordinates.y] = null;
    }

    public bool CheckIfCoordinateIsAvailable(Vector2Int coordinateToCheck)
    {
        return !(CheckIfCoordinateIsOutsideLabyrinth(coordinateToCheck) || CheckIfCoordinateIsOccupied(coordinateToCheck));
    }

    public bool CheckIfCoordinateIsOutsideLabyrinth(Vector2Int coordinateToCheck)
    {
        if (coordinateToCheck.x < 0 || coordinateToCheck.y < 0 || coordinateToCheck.x >= LabyrinthSize || coordinateToCheck.y >= LabyrinthSize)
            return true;
        else
            return false;
    }

    public bool CheckIfCoordinateIsOccupied(Vector2Int coordinateToCheck)
    {
        if (blocksMatrix[coordinateToCheck.x, coordinateToCheck.y] == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public Block GetBlockAtCoordinates(Vector2Int coordinatesToGet)
    {
        return blocksMatrix[coordinatesToGet.x, coordinatesToGet.y];
    }

    public void SetUsedBlock(Block block)
    {
        availableBlocks[block.BlockType].Remove(block);
        availableBlocksAmount--;
        usedBlocks[block.BlockType].Add(block);
        block.gameObject.transform.SetParent(usedBlocksContainer.transform);
    }

    public void SetAvailableBlock(Block block)
    {
        usedBlocks[block.BlockType].Remove(block);
        availableBlocks[block.BlockType].Add(block);
        availableBlocksAmount++;
        block.gameObject.transform.SetParent(availableBlocksContainer.transform);
    }

    public void ResetLabyrinth()
    {
        foreach(Block b in blocksMatrix)
        {
            if(b != null)
                b.ResetBlock();            
        }
    }
}
