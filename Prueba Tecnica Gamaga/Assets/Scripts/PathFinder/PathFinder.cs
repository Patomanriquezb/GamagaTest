using System.Collections;
using System.Collections.Generic;

public class PathFinder
{
    List<Block> uncheckedBlocks = new List<Block>();
    List<Block> checkedBlocks = new List<Block>();

    List<Node> baseBranchHeads = new List<Node>();
    List<Node> newBranchHeads = new List<Node>();

    Block entrance;
    Block exit;

    public Node finalNode { get; private set; } = null;

    public bool FindThePath(Block[,] blocksMatrix, Block _entrance, Block _exit)
    {
        uncheckedBlocks.Clear();
        checkedBlocks.Clear();

        foreach (Block b in blocksMatrix)
        {
            uncheckedBlocks.Add(b);
        }

        entrance = _entrance;
        exit = _exit;

        Node startNode = new Node(null, entrance, entrance, exit);
        BlockChecked(startNode.b);
        newBranchHeads.Add(startNode);
        while(finalNode == null && newBranchHeads.Count > 0)
        {
            foreach (Node newBase in newBranchHeads)
                baseBranchHeads.Add(newBase);

            newBranchHeads.Clear();
            foreach(Node n in baseBranchHeads)
            {
                EvaluateAdjacentNodes(n);
            }
        }

        if (finalNode == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void BlockChecked(Block checkedBlock)
    {
        uncheckedBlocks.Remove(checkedBlock);
        checkedBlocks.Add(checkedBlock);
    }

    public void EvaluateAdjacentNodes(Node n)
    {
        List<Block> connectedBlocks = n.b.GetConnectedBlocks();
        foreach(Block connected in connectedBlocks)
        {
            if (uncheckedBlocks.Contains(connected))
            {
                Node newNode = new Node(n, connected, entrance, exit);
                BlockChecked(newNode.b);
                newBranchHeads.Add(newNode);
                if (newNode.b == exit)
                {
                    finalNode = newNode;
                    return;
                }
                //else
                //{
                //    EvaluateAdjacentNodes(newNode);
                //}
            }        
        }

    }
}

public class Node
{
    int G = 0;
    int H = 0;
    int F = 0;
    public Block b;

    public Node prevNode;

    public Node(Node _prevNode, Block _b, Block entrance, Block exit)
    {
        prevNode = _prevNode;
        b = _b;
        if (prevNode != null)
            G = prevNode.G + 1;
        H = (exit.BlockCoordinates.x - b.BlockCoordinates.x) + (exit.BlockCoordinates.y - b.BlockCoordinates.y);
        F = G + H;
    }

}

