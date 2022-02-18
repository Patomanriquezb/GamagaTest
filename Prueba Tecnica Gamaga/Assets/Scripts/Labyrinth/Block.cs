using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private BlockTypes blockType;
    public BlockTypes BlockType { get { return blockType; } private set { blockType = value; } }

    private Vector2Int blockCoordinates;
    public Vector2Int BlockCoordinates { get { return blockCoordinates; } private set { blockCoordinates = value; } }


    private LabyrinthGenerator generator;
    public LabyrinthGenerator Generator { get { return generator; } set { if (generator == null) generator = value; } }

    [SerializeField]
    bool[] pathOpenings = new bool[4]; // 0 = up, 1 = right, 2 = down, 3 = left

    [SerializeField]
    public bool canForceOtherBlocks = true;
    public bool isInitiallyGenerated { get; private set; } = false;

    public GameObject solutionMarker;


    public void UseBlock(Vector2Int coordinates, bool recursive, bool initialLabyrinth)
    {
        isInitiallyGenerated = initialLabyrinth;

        BlockCoordinates = coordinates;
        transform.position = new Vector3(BlockCoordinates.x * 3, 0, BlockCoordinates.y * 3);
        generator.SetUsedBlock(this);
        if (!generator.RegisterBlockInMatrix(this))
        {
            Debug.LogWarning("Block not registered in matrix.");
            return;
        }

        if (recursive)
            SetAdjacentBlocks(recursive, initialLabyrinth);
    }

    public void ReturnBlock()
    {
        transform.position = new Vector3(-999, -999, -999);
    }

    public void SetAdjacentBlocks(bool recursive, bool initialLabyrinth)
    {
        for (int i = 0; i < 4; i++)
        {
            if (pathOpenings[i])
            {
                Vector2Int newBlockCoordinates = GetNewCoordinates(BlockCoordinates, i);
                if (Generator.CheckIfCoordinateIsAvailable(newBlockCoordinates))
                {
                    Block blockToSet = DetermineBlockToSet(newBlockCoordinates, i);
                    if (blockToSet != null)
                        blockToSet.UseBlock(newBlockCoordinates, recursive, initialLabyrinth);
                }
            }
        }
    }

    public void ForceAdjacentConnection(bool recursive)
    {
        canForceOtherBlocks = false;
        for (int i = 0; i < 4; i++)
        {
            if (pathOpenings[i])
            {
                Vector2Int newBlockCoordinates = GetNewCoordinates(BlockCoordinates, i);
                if (!Generator.CheckIfCoordinateIsOutsideLabyrinth(newBlockCoordinates) && Generator.CheckIfCoordinateIsOccupied(newBlockCoordinates))
                {
                    Block toForce = Generator.GetBlockAtCoordinates(newBlockCoordinates);
                    if(toForce != null)
                    {
                        if (IsConnectedTo(toForce))
                        {
                            if (recursive)
                            {
                                if (toForce.canForceOtherBlocks)
                                {
                                    //Debug.LogWarning($"Block to force ({toForce.BlockCoordinates.x},{toForce.BlockCoordinates.y}), already checked? {canForceOtherBlocks}");
                                    toForce.ForceAdjacentConnection(recursive);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if(toForce.isInitiallyGenerated)
                        {

                            bool[] originalOpenings = toForce.pathOpenings;
                            Block newBlock = DetermineBlockToSet(newBlockCoordinates, i, originalOpenings);
                            if (newBlock == null)
                                continue;

                            toForce.ResetBlock();

                            newBlock.UseBlock(newBlockCoordinates, false, false);

                            //Debug.LogWarning($"Block at {newBlockCoordinates.x},{newBlockCoordinates.y} replaced!");
                        }
                    }
                }
            }
        }
    }

    public Block DetermineBlockToSet(Vector2Int blockToSetCoordinates, int originalBlockDirectionOpening, bool[] originalOpenings = null, int iteration = 0)
    {
        //Debug.Log($"Determining block to set by block {BlockType} ({BlockCoordinates.x},{BlockCoordinates.y}) on ({blockToSetCoordinates.x},{blockToSetCoordinates.y})");

        Block possibleBlock;
            possibleBlock = generator.GetRandomAvailableBlock();
            if (possibleBlock == null)
                return null;

        //iteration++;
        if (iteration >= Generator.LabyrinthSize * Generator.LabyrinthSize)
        {
            Debug.LogWarning($"Block at ({BlockCoordinates.x},{BlockCoordinates.y}) took too long to find next block for ({blockToSetCoordinates.x},{blockToSetCoordinates.y})");
            return null;
        }

        //Invert direction to check for opening
        int pathOpeningToCheck = GetOppositeDirection(originalBlockDirectionOpening);

        if (!possibleBlock.pathOpenings[pathOpeningToCheck] || CheckBlockBreachesBorder(possibleBlock, blockToSetCoordinates))
        {
            //Debug.Log($"Rejected {possibleBlock.BlockType} block. (1)");
            return DetermineBlockToSet(blockToSetCoordinates, originalBlockDirectionOpening, originalOpenings, iteration);
        }
        else if(originalOpenings != null)
        {
            bool matchesOriginalOpenings = true;
            for(int i = 0; i <4; i++)
            {
                if (i == pathOpeningToCheck)
                    continue;
                if (possibleBlock.pathOpenings[i] != originalOpenings[i])
                    matchesOriginalOpenings = false;
            }
            if (matchesOriginalOpenings)
            {
                return possibleBlock;
            }
            else
            {
                //Debug.Log($"Rejected {possibleBlock.BlockType} block. (2)");
                return DetermineBlockToSet(blockToSetCoordinates, originalBlockDirectionOpening, originalOpenings, iteration);
            }
        }
        else
        {
            return possibleBlock;
        }

    }

    public int GetOppositeDirection(int originalDirection)
    {
        return (originalDirection + 2) % 4;
    }

    public bool CheckBlockBreachesBorder(Block possibleBlock, Vector2Int blockToSetCoordinates)
    {
        bool breachesBorder = false;

        if (blockToSetCoordinates.x == 0 && possibleBlock.pathOpenings[3])
            breachesBorder = true;

        if (blockToSetCoordinates.y == 0 && possibleBlock.pathOpenings[2])
            breachesBorder = true;

        if (blockToSetCoordinates.x == Generator.LabyrinthSize - 1 && possibleBlock.pathOpenings[1])
            breachesBorder = true;

        if (blockToSetCoordinates.y == Generator.LabyrinthSize - 1 && possibleBlock.pathOpenings[0])
            breachesBorder = true;

        return breachesBorder;
    }

    public Vector2Int GetNewCoordinates(Vector2Int originalCoordinates, int direction)
    {
        Vector2Int newCoordinates = new Vector2Int(originalCoordinates.x, originalCoordinates.y);
        switch (direction)
        {
            case 0:
                newCoordinates += Vector2Int.up;
                break;
            case 1:
                newCoordinates += Vector2Int.right;
                break;
            case 2:
                newCoordinates += Vector2Int.down;
                break;
            case 3:
                newCoordinates += Vector2Int.left;
                break;
        }
        return newCoordinates;
    }

    public void ResetBlock()
    {
        isInitiallyGenerated = false;
        canForceOtherBlocks = true;
        ReturnBlock();
        Generator.SetAvailableBlock(this);
        Generator.RemoveBlockFromMatrix(this);
        solutionMarker.SetActive(false);
    }

    public List<Block> GetConnectedBlocks()
    {
        List<Block> connectedBlocks = new List<Block>();
        for (int i = 0; i < 4; i++)
        {
            if (pathOpenings[i])
            {
                Vector2Int newBlockCoordinates = GetNewCoordinates(BlockCoordinates, i);
                if (!Generator.CheckIfCoordinateIsOutsideLabyrinth(newBlockCoordinates) && Generator.CheckIfCoordinateIsOccupied(newBlockCoordinates))
                {
                    Block newBlock = Generator.GetBlockAtCoordinates(newBlockCoordinates);
                    if(newBlock != null && IsConnectedTo(newBlock))
                        connectedBlocks.Add(newBlock);
                }
            }
        }
        return connectedBlocks;
    }

    public bool IsConnectedTo(Block other)
    {
        if (other == null)
            return false;

        Vector2Int vectorDifference = other.BlockCoordinates - BlockCoordinates;
        {
            if (vectorDifference.x == 1 && vectorDifference.y == 0)
            {
                //other is on my right
                return pathOpenings[1] && other.pathOpenings[3];

            }
            else if (vectorDifference.x == -1 && vectorDifference.y == 0)
            {
                //other is on my left
                return pathOpenings[3] && other.pathOpenings[1];

            }
            else if (vectorDifference.x == 0 && vectorDifference.y == 1)
            {
                //other is above me
                return pathOpenings[0] && other.pathOpenings[2];
            }
            else if (vectorDifference.x == 0 && vectorDifference.y == -1)
            {
                //other is below me
                return pathOpenings[2] && other.pathOpenings[0];
            }
            else
                return false;

        }

    }


}

public enum BlockTypes
{
    None_NeverAssignToBlock,
    Full,
    VerticalLine,
    HorizontalLine,
    RightL,
    LeftL,
    RightR,
    LeftR,
    Cross,
    TDown,
    TRight,
    TUp,
    TLeft
}
