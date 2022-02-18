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

    public bool alreadyChecked { get; private set; } = false;

    public void UseBlock(Vector2Int coordinates, bool recursive, bool initialLabyrinth)
    {
        alreadyChecked = initialLabyrinth;   

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
        alreadyChecked = true;
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
                        if (toForce.pathOpenings[GetOppositeDirection(i)])
                        {
                            if (recursive)
                            {
                                if (!toForce.alreadyChecked)
                                {
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
                        else
                        {

                            bool[] originalOpenings = toForce.pathOpenings;
                            Block newBlock = DetermineBlockToSet(newBlockCoordinates, i, originalOpenings);
                            if (newBlock == null)
                                continue;

                            toForce.ResetBlock();

                            newBlock.UseBlock(newBlockCoordinates, false, false);

                            Debug.LogWarning($"Block at {newBlockCoordinates.x},{newBlockCoordinates.y} replaced!");
                        }
                    }
                }
            }
        }
    }

    public Block DetermineBlockToSet(Vector2Int blockToSetCoordinates, int originalBlockDirectionOpening, bool[] originalOpenings = null, int iteration = 0)
    {
        Debug.Log($"Determining block to set by block {BlockType} ({BlockCoordinates.x},{BlockCoordinates.y}) on ({blockToSetCoordinates.x},{blockToSetCoordinates.y})");

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
            Debug.Log($"Rejected {possibleBlock.BlockType} block. (1)");
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
                Debug.Log($"Rejected {possibleBlock.BlockType} block. (2)");
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
        alreadyChecked = false;
        ReturnBlock();
        Generator.SetAvailableBlock(this);
        Generator.RemoveBlockFromMatrix(this);
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
