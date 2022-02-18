using System.Collections;
using System.Collections.Generic;

public class Labyrinth
{
    public Block[,] blocksMatrix;
    public Block entrance;
    public Block exit;

    public Node solution;

    public Labyrinth(Block[,] _blocksMatrix, Block _entrance, Block _exit)
    {
        blocksMatrix = _blocksMatrix;
        entrance = _entrance;
        exit = _exit;
    }
}
