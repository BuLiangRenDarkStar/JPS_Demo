using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode
{
    public bool Walkable=true;
    public Vector3 WorldPosition;
    public int xNum, yNum;
    public float NodeSize;
    public GridNode Parent;
    public float fCost;
    public float gCost;
    public float hCost;

    public GridNode(Vector3 worldPos, int xNum, int yNum, float size,bool walkable)
    {
        WorldPosition = worldPos;
        this.xNum = xNum;
        this.yNum = yNum;
        NodeSize = size;
        Walkable = walkable;
    }
}
