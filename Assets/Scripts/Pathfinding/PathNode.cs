using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public int gridX; // X position in the grid
    public int gridY; // Y position in the grid

    public bool isWalkable;
    public Vector3 worldPosition;
    public PathNode parentNode; // For A* algorithm, a pointer to the previous node on the path

    public int gCost; // The cost of moving from the start to this node
    public int hCost; // The estimated cost of moving from this node to the end (heuristic)
    public int FCost { get { return gCost + hCost; } } // G cost + H cost

    public PathNode(bool _isWalkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        isWalkable = _isWalkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        gCost = int.MaxValue;
        hCost = 0;
        parentNode = null;
    }

    public void ResetCosts()
    {
        gCost = int.MaxValue;
        hCost = 0;
        parentNode = null;
    }

}
