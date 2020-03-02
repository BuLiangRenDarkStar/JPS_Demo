using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindPath
{
    private GridNode StartNode;
    private GridNode TargetNode;
    
    private List<GridNode> JumpNodeList;
    private HashSet<GridNode> OpenSet;
    private HashSet<GridNode> ClosedSet;

    private List<GridNode> Path;

    private Grid grid;
    public List<GridNode> GetPath()
    {
        Init();
        return Path;
    }

    private void Init()
    {
        JumpNodeList = new List<GridNode>();
        OpenSet = new HashSet<GridNode>();
        ClosedSet = new HashSet<GridNode>();
        grid = Grid.Instance;
    }

    private bool FindShortestPath()
    {
        GridNode currentNode = StartNode;

        OpenSet.Add(currentNode);
        
        while( OpenSet.Count>0)
        {
            currentNode = RemoveMinNode();

            if( currentNode==TargetNode)
            {
                return true;
            }
            else
            {
                ClosedSet.Add(currentNode);
                List<GridNode> jumpNodes = GetSuccessors(currentNode);
                foreach( GridNode jumpNode in jumpNodes)
                {
                    if (ClosedSet.Contains(jumpNode))
                        continue;

                   float newGCost = currentNode.gCost + GetGCost(currentNode, jumpNode);
                   bool isInOpenset = OpenSet.Contains(jumpNode);
                   if ( !isInOpenset || newGCost<jumpNode.gCost )//不被包含在开放列表里，g值还未计算
                   {
                        jumpNode.gCost = newGCost;
                        jumpNode.hCost = GetHCost(jumpNode, TargetNode);
                        jumpNode.Parent = currentNode;
                        if ( !isInOpenset )
                        {
                            OpenSet.Add(jumpNode);
                        }
                   }
                }
            }
        }

        return false;
    }

    GridNode RemoveMinNode()
    {
        GridNode minNode=null;
        foreach ( GridNode node in OpenSet)
        {
            if(minNode == null)
            {
                minNode = node;
            }
            else
            {
                minNode = node.fCost < minNode.fCost ? node : minNode;
            }
        }
        OpenSet.Remove(minNode);
        return minNode;
    }

    //从所有指向的邻居节点的方向搜索跳点
    private List<GridNode> GetSuccessors( GridNode currentNode)
    {
        GridNode jumpNode;
        List<GridNode> neighbours = GetNeighbors(currentNode);
        List<GridNode> allJumpNodes = new List<GridNode>();

        for(int i=0; i<neighbours.Count; ++i)
        {
            int xDir = neighbours[i].xNum - currentNode.xNum;
            int yDir = neighbours[i].yNum - currentNode.yNum;

            jumpNode = GetJumpNode(neighbours[i], currentNode, xDir, yDir);
            if (jumpNode != null)
                JumpNodeList.Add(jumpNode);
        }
        return JumpNodeList;
    }


    private List<GridNode> GetNeighbors( GridNode currentNode )
    {
        List<GridNode> neighbors = new List<GridNode>();
        GridNode parentNode = currentNode.Parent;
        int x = currentNode.xNum, y = currentNode.yNum;
        //如果是起点，每个方向都需要搜索
        if (parentNode == null)
        {
            int[,] dir = new int[8, 2] { {0,1 },{ 1,1},{ 1,0},{ 1,-1},
                        { 0,-1},{ -1,-1}, { -1,0},{ -1,1} };
            for (int i = 0; i < 8; ++i)
            {        
                if(grid.GridMap[dir[i, 1]+x, dir[i, 1]+y].Walkable)
                {
                    neighbors.Add(grid.GridMap[x, y]);
                }
            }
        }
        else
        {//对于非起点
            int xDir = Mathf.Clamp(x - parentNode.xNum, -1, 1);
            int yDir = Mathf.Clamp(y - parentNode.yNum, -1, 1);

            GridNode neighbourUp = grid.GetNodeFromIndex(x, y + yDir);
            GridNode neighbourRight = grid.GetNodeFromIndex(x + xDir, y);
            GridNode neighbourDown = grid.GetNodeFromIndex(x, y - yDir);
            GridNode neighbourLeft = grid.GetNodeFromIndex(x - xDir, y);

            if ( xDir!=0&&yDir!=0)
            {
                if ( grid.IsWalkable(neighbourUp) )
                    neighbors.Add(neighbourUp);

                if (grid.IsWalkable(neighbourRight))
                    neighbors.Add(neighbourRight);

                if (grid.IsWalkable(neighbourUp) || grid.IsWalkable(neighbourRight))
                {   //如果沿当前方向的下个位置可走
                    GridNode nextNode = grid.GetNodeFromIndex(x + xDir, y + yDir);
                    if(grid.IsWalkable(nextNode))
                        neighbors.Add(nextNode);
                }

                if(!grid.IsWalkable(neighbourLeft)&& grid.IsWalkable(neighbourUp))
                {
                    GridNode leftUp = grid.GetNodeFromIndex(x - xDir, y + yDir);
                    if( grid.IsWalkable(leftUp) )
                        neighbors.Add(leftUp);
                }

                if( !grid.IsWalkable(neighbourRight)&&grid.IsWalkable(neighbourDown))
                {
                    GridNode rightDown = grid.GetNodeFromIndex(x +xDir, y - yDir);
                    if (grid.IsWalkable(rightDown))
                        neighbors.Add(rightDown);
                }

            }
            else
            {
                if (xDir == 0)
                {
                    if(grid.IsWalkable(neighbourUp))
                    {
                        neighbors.Add(neighbourUp);

                        if (!grid.IsWalkable(x + 1, y) && grid.IsWalkable(x + 1, y + yDir))
                            neighbors.Add(grid.GridMap[x + 1, y + yDir] );

                        if (!grid.IsWalkable(x - 1, y) && grid.IsWalkable(x - 1, y + yDir))
                            neighbors.Add(grid.GridMap[x - 1, y + yDir]);
                    }
                }
                else
                {
                    if( grid.IsWalkable(neighbourRight) )
                    {
                        neighbors.Add(neighbourRight);

                        if (!grid.IsWalkable(x, y+1)&& grid.IsWalkable(x+xDir, y + 1) )
                            neighbors.Add(grid.GridMap[x + xDir, y + 1]);
                        
                        if(!grid.IsWalkable(x, y - 1)&&grid.IsWalkable(x + xDir, y - 1))
                            neighbors.Add(grid.GridMap[x + xDir, y - 1]);                        
                    }
                }
            }
        }

        return neighbors;
    }
    
    private GridNode GetJumpNode(GridNode currentNode, GridNode parentNode, int xDir, int yDir)
    {
        int x = currentNode.xNum, y = currentNode.yNum;
        if (currentNode == null || !grid.IsWalkable(currentNode))
            return null;
        if (currentNode == StartNode || currentNode == TargetNode)
            return currentNode;

        if( xDir!=0 && yDir!=0)
        {//斜线方向
            //斜方向有强迫邻居，该节点为跳点,但这里不认为斜线方向有障碍物能直接到达
            /*
            1 2
            3 4  */
            //假设2是障碍物，不认为1能直接到达4.
            //if( (!grid.IsWalkable(currentNode.xNum-xDir, currentNode.yNum)&& grid.IsWalkable(currentNode.xNum - xDir, currentNode.yNum+yDir)) 
            //    || (!grid.IsWalkable(currentNode.xNum + xDir, currentNode.yNum)&& grid.IsWalkable(currentNode.xNum + xDir, currentNode.yNum + yDir))  )
            //{
            //    return currentNode;
            //}
            
            //水平竖直方向分解，查看沿分解的水平/竖直方向能否到达一个跳点，如果能则该点也是跳点
            if( GetJumpNode(grid.GetNodeFromIndex(x+xDir,y),currentNode,xDir,0)!=null||
                GetJumpNode(grid.GetNodeFromIndex(x, y+yDir),currentNode,0,yDir)!=null )
            {
                return currentNode;
            }
        }
        else
        {
            //px（px为x为任一父节点）->x->n n有上下左右四个邻居节点是障碍节点，任何px到n最快路径是px->x->n，则n为x的强迫邻居节点，x为跳点 (  节点y有强迫邻居则y为跳点)
            if (xDir != 0)
            {   /*
                 2 3 
                 5 6  //当前节点为6(x,y)，父节点为5 假设2(x-xDir,y+1)为障碍物，不允许5到3直接穿过，那么3(x,y+1)是6的强迫邻居，6为跳点         
                 */
                if ( ( grid.IsWalkable(grid.GetNodeFromIndex(x, y + 1)) && !grid.IsWalkable(grid.GetNodeFromIndex(x - xDir, y + 1)) )  ||
                    ( grid.IsWalkable(grid.GetNodeFromIndex(x, y - 1)) && !grid.IsWalkable(grid.GetNodeFromIndex(x - xDir, y - 1)) ) )
                    return currentNode;
            }
            else
            { /*
                 2 3 
                 5 6  //当前节点为5(x,y)，父节点为2 假设3(x+1,y-yDir)为障碍物，不允许2到6直接穿过，那么6(x+1,y)是5的强迫邻居，5为跳点         
                 */
                if ( ( grid.IsWalkable(grid.GetNodeFromIndex(x+1, y)) && !grid.IsWalkable(grid.GetNodeFromIndex(x+1, y-yDir)) ) ||
                   (grid.IsWalkable(grid.GetNodeFromIndex(x-1, y)) && !grid.IsWalkable(grid.GetNodeFromIndex(x-1, y-yDir))) )
                    return currentNode;
            }
        }

        if (grid.IsWalkable(x + xDir, y) || grid.IsWalkable(x, y + yDir))
            return GetJumpNode(grid.GetNodeFromIndex(x + xDir, y + yDir), currentNode, xDir, yDir);

        return null;
    }

    float GetGCost(GridNode n1, GridNode n2)
    {
        return GetDistance(n1, n2);
    }

    float GetHCost(GridNode n1, GridNode n2)
    {
        return GetDistance(n1, n2);
    }

    float GetDistance(GridNode n1, GridNode n2)
    {
        if (n1.xNum == n2.xNum || n1.yNum == n2.yNum)
        {
            return Mathf.Abs(n1.xNum - n2.xNum) + Mathf.Abs(n1.yNum - n2.yNum);
        }
        else
        {
            return Mathf.Sqrt((n1.xNum - n2.xNum) ^ 2 + (n1.yNum - n2.yNum) ^ 2);
        }
    }
}
