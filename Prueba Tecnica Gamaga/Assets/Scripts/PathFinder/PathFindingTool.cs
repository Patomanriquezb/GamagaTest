using System.Collections;
using System.Collections.Generic;

public static class PathFindingTool
{
   public static Node FindThePath(Block[,] blocksMatrix, Block _entrance, Block _exit)
    {
        PathFinder pF = new PathFinder();
        pF.FindThePath(blocksMatrix, _entrance, _exit);
        return pF.finalNode;
    }
}
