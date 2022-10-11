using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace GameFramework
{
    public class PathGrid
    {
        private SortedDictionary<int, List<Vector3>> openTree = new SortedDictionary<int, List<Vector3>>();

        private HashSet<Vector3> openSet = new HashSet<Vector3>();
        private HashSet<Vector3> closeSet = new HashSet<Vector3>();
        private Dictionary<Vector3, PathNode> allNodes = new Dictionary<Vector3, PathNode>();

        private Vector3 endPos;
        private Vector3 gridSize;

        private List<Vector3> currentPath;

        //这一部分在实际寻路中并不需要，只是为了方便外部程序实现寻路可视化
        public HashSet<Vector3> GetCloseList()
        {
            return closeSet;
        }

        //这一部分在实际寻路中并不需要，只是为了方便外部程序实现寻路可视化
        public HashSet<Vector3> GetOpenList()
        {
            return openSet;
        }

        //这一部分在实际寻路中并不需要，只是为了方便外部程序实现寻路可视化
        public List<Vector3> GetCurrentPath()
        {
            return currentPath;
        }

        //新建一个PathGrid，包含了网格大小和障碍物信息
        public PathGrid(float x, float y, List<Vector3> walls)
        {
            gridSize = new Vector3(x, y);
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    Vector3 newPos = new Vector3(i, j);
                    PathNode node = new PathNode(walls == null ? false : walls.Contains(newPos), newPos);
                    allNodes.Add(newPos, node);
                }
            }
        }

        //寻路主要逻辑，通过调用该方法来获取路径信息，由一串Vector3代表
        public List<Vector3> FindPath(Vector3 beginPos, Vector3 endPos)
        {
            List<Vector3> result = new List<Vector3>();

            this.endPos = endPos;
            Vector3 currentPos = beginPos;
            openSet.Add(currentPos);

            while (!currentPos.Equals(this.endPos))
            {
                UpdatePath(currentPos);
                if (openSet.Count == 0) return null;

                currentPos = openTree.First().Value.First();
            }

            Vector3 path = currentPos;

            while (!path.Equals(beginPos))
            {
                result.Add(path);
                path = allNodes[path].parent.position;
                currentPath = result;
            }

            result.Add(beginPos);
            return result;
        }

        //寻路
        private void UpdatePath(Vector3 currentPos)
        {
            closeSet.Add(currentPos);
            RemoveOpen(currentPos, allNodes[currentPos]);
            List<Vector3> neighborNodes = FindNeighbor(currentPos);
            foreach (Vector3 nodePos in neighborNodes)
            {

                PathNode newNode = new PathNode(false, nodePos);
                newNode.parent = allNodes[currentPos];

                int g;
                int h;

                g = currentPos.x == nodePos.x || currentPos.y == nodePos.y ? 10 : 14;

                int xMoves = (int)Math.Abs(nodePos.x - endPos.x);
                int yMoves = (int)Math.Abs(nodePos.y - endPos.y);

                int min = Math.Min(xMoves, yMoves);
                int max = Math.Max(xMoves, yMoves);
                h = min * 14 + (max - min) * 10;


                newNode.gCost = g + newNode.parent.gCost;
                newNode.hCost = h;

                PathNode originNode = allNodes[nodePos];

                if (openSet.Contains(nodePos))
                {
                    if (newNode.fCost < originNode.fCost)
                    {
                        UpdateNode(newNode, originNode);
                    }
                }
                else
                {
                    allNodes[nodePos] = newNode;
                    AddOpen(nodePos, newNode);
                }
            }
        }

        //将旧节点更新为新节点
        private void UpdateNode(PathNode newNode, PathNode oldNode)
        {
            Vector3 nodePos = newNode.position;
            int oldCost = oldNode.fCost;
            allNodes[nodePos] = newNode;
            List<Vector3> sameCost;

            if (openTree.TryGetValue(oldCost, out sameCost))
            {
                sameCost.Remove(nodePos);
                if (sameCost.Count == 0) openTree.Remove(oldCost);
            }

            if (openTree.TryGetValue(newNode.fCost, out sameCost))
            {
                sameCost.Add(nodePos);
            }
            else
            {
                sameCost = new List<Vector3> { nodePos };
                openTree.Add(newNode.fCost, sameCost);
            }
        }

        //将目标节点移出开启列表
        private void RemoveOpen(Vector3 pos, PathNode node)
        {
            openSet.Remove(pos);
            List<Vector3> sameCost;
            if (openTree.TryGetValue(node.fCost, out sameCost))
            {
                sameCost.Remove(pos);
                if (sameCost.Count == 0) openTree.Remove(node.fCost);
            }
        }

        //将目标节点加入开启列表
        private void AddOpen(Vector3 pos, PathNode node)
        {
            openSet.Add(pos);
            List<Vector3> sameCost;
            if (openTree.TryGetValue(node.fCost, out sameCost))
            {
                sameCost.Add(pos);
            }
            else
            {
                sameCost = new List<Vector3> { pos };
                openTree.Add(node.fCost, sameCost);
            }
        }

        //找到某节点的所有相邻节点
        private List<Vector3> FindNeighbor(Vector3 nodePos)
        {
            List<Vector3> result = new List<Vector3>();

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (x == 0 && y == 0) continue;

                    Vector3 currentPos = new Vector3(nodePos.x + x, nodePos.y + y);

                    if (currentPos.x >= gridSize.x || currentPos.y >= gridSize.y || currentPos.x < 0 || currentPos.y < 0) continue; //out of bondary
                    if (closeSet.Contains(currentPos)) continue; // already in the close list
                    if (allNodes[currentPos].isWall) continue;  // the node is a wall

                    result.Add(currentPos);
                }
            }

            return result;
        }
    }
}