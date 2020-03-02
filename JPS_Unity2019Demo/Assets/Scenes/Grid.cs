using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : Singleton<Grid>
{
    public int NodeXNum;
    public int NodeYNum;

    public float UnitGridSize;

    private GridNode[,] _gridMap;
    public GridNode[,] GridMap {
        get{ return _gridMap; }
    }

    public int UnityGridNum{
        get
        {
            return NodeXNum * NodeYNum;
        }
    }

    private Transform GridTransform;
    private LayerMask UnwalkableLayerMask;
    public void InitializeGrid( Transform gridTransform)
    {
        GridTransform = gridTransform;
        _gridMap = new GridNode[NodeXNum, NodeYNum];
    }

    //将地图网格化，二维数组保存每一个单元网格节点
    public void CreateGrid()
    {
        Vector3 xOffset = new Vector3(1, 1, 1);
        Vector3 yOffset = new Vector3(1, 1, 1);
        Vector3 worldBottomLeft = GridTransform.position - xOffset - yOffset;

        for( int xNum=0; xNum < NodeXNum; ++xNum)
        {
            for(int yNum=0; yNum < NodeYNum; ++yNum) {
                float x = xNum * NodeXNum;
                float y = yNum * NodeYNum;
                Vector3 worldPoint = worldBottomLeft + Vector3.right * x + Vector3.forward * y;

                RaycastHit hit;
                bool walkable = !Physics.SphereCast(worldPoint + Vector3.up * 500, UnitGridSize / 2, 
                    Vector3.down, out hit, Mathf.Infinity, UnwalkableLayerMask);

                GridNode gridNode = new GridNode(worldPoint,xNum,yNum,UnitGridSize,walkable);

                _gridMap[xNum, yNum] = gridNode;
            }
        }
    }

    //防止越界
    public GridNode GetNodeFromIndex( int x, int y)
    {
        if (x > _gridMap.Length || x < 0)
            return null;
        if (y > _gridMap.GetLength(0) || y < 0)
            return null;
        return _gridMap[x, y];
    }

    public bool IsWalkable(int x, int y)
    {
        return InBounds(x, y) && _gridMap[x, y].Walkable;
    }

    public bool IsWalkable(GridNode gridNode)
    {
        if (gridNode != null)
        {
            int x = gridNode.xNum, y = gridNode.yNum;
            return InBounds(x,y) && _gridMap[x, y].Walkable;
        }
        else
        {
            return false;
        }
    }

    public bool InBounds(int x, int y)
    {
        return x >= 0 && x < NodeXNum &&
            y >= 0 && y < NodeYNum;
    }
}
