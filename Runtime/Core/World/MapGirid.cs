using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace GameFramework
{
    public sealed class MapGrid : GObject
    {
        public Rect view { get; private set; }
        public float width
        {
            get
            {
                return view.width;
            }
        }
        public float height
        {
            get
            {
                return view.height;
            }
        }

        public int Count
        {
            get;
            private set;
        }

        public float GridSize
        {
            get;
            private set;
        }

        public int RowCount
        {
            get;
            private set;
        }

        private MapNode[,] maps;

        public void Dispose()
        {
        }

        public void Initialize(float gridSize, int rowGridCount, List<bool> isWall)
        {
            GridSize = gridSize;
            RowCount = rowGridCount;
            Count = rowGridCount * rowGridCount;
            view = new Rect(-(rowGridCount * gridSize / 2), -(rowGridCount * gridSize / 2), rowGridCount * gridSize, rowGridCount * gridSize);
            //view = new Rect(0, 0, rowGridCount * gridSize, rowGridCount * gridSize);
            maps = new MapNode[rowGridCount, rowGridCount];
            for (var i = 0; i < rowGridCount * rowGridCount; i++)
            {
                float offset_x = view.x + i % rowGridCount * gridSize;
                float offset_y = view.y + i / rowGridCount * gridSize;
                MapNode node = new MapNode(i, rowGridCount, offset_x, offset_y, gridSize, isWall == null ? true : isWall[i]);
                maps[i / rowGridCount, i % rowGridCount] = node;
            }
        }


        public Vector3 ToMapPoint(int index)
        {
            if (index < 0 || index > Count - 1)
            {
                throw new IndexOutOfRangeException();
            }
            int x = index % RowCount;
            int y = index / RowCount;
            return maps[index / RowCount, index % RowCount].center;
        }

        public Vector3 ToMapPoint(Vector3 position)
        {
            return ToMapPoint(PositionToIndex(position));
        }
        private MapNode ToMapNode(int index)
        {
            if (index < 0 || index > Count - 1)
            {
                throw new IndexOutOfRangeException("坐标：" + index.ToString());
            }
            return maps[index / RowCount, index % RowCount];
        }
        private MapNode ToMapNode(Vector3 position)
        {
            MapNode node = ToMapNode(PositionToIndex(position));
            if (node == null)
            {
                throw new KeyNotFoundException(position.ToString());
            }
            return node;
        }
        public bool IsObstacle(Vector3 position)
        {
            return ToMapNode(PositionToIndex(position)).isWall;
        }

        private int PositionToIndex(Vector3 position)
        {
            float range = width / 2;
            float offset_x = position.x + range;
            float offset_y = position.z + range;
            float mod_x = Math.Abs(offset_x % GridSize);
            int count_x = (int)Math.Abs(offset_x / GridSize);
            if (mod_x > GridSize / 2)
            {
                count_x++;
            }

            float mod_y = Math.Abs(offset_y % GridSize);
            int count_y = (int)Math.Abs(offset_y / GridSize);
            if (mod_y > GridSize / 2)
            {
                count_y++;
            }

            return Math.Max(0, count_y * RowCount) + count_x;
        }


        public void AddObstacle(int index)
        {
            if (index < 0 || index > Count - 1)
            {
                throw new IndexOutOfRangeException();
            }
            ToMapNode(index)?.Closed();
        }

        public void AddObstacle(Vector3 position)
        {
            MapNode node = ToMapNode(PositionToIndex(position));
            if (node == null)
            {
                throw new KeyNotFoundException(position.ToString());
            }
            node.Closed();
        }

        public void RemoveObstacle(int index)
        {
            if (index < 0 || index > Count - 1)
            {
                throw new IndexOutOfRangeException();
            }
            ToMapNode(index)?.Open();
        }

        public void RemoveObstacle(Vector3 position)
        {
            MapNode node = ToMapNode(PositionToIndex(position));
            if (node == null)
            {
                throw new KeyNotFoundException(position.ToString());
            }
            node.Open();
        }
        public Vector3[] FindPath(Vector3 startPos, Vector3 targetPos)
        {
            MapNode startNode = ToMapNode(startPos);
            MapNode targetNode = ToMapNode(targetPos);
            Queue<MapNode> openSet = new Queue<MapNode>();//开放节点   需要被评估的节点
            HashSet<MapNode> closeSet = new HashSet<MapNode>();//闭合节点   已经评估的节点
            openSet.Enqueue(startNode);
            while (openSet.Count > 0 )
            {
                MapNode template = openSet.Dequeue();
                closeSet.Add(template);
                if (template == targetNode)
                {
                    return GeneratePath(startNode, targetNode);
                }
                List<MapNode> subs = FindSubNode(template);
                foreach (var temp in subs)
                {
                    if (closeSet.Contains(temp) || !temp.isWall)
                    {
                        continue;
                    }
                    int gCost = template.gCost + GetDistanceNodes(template, temp);
                    if (gCost < temp.gCost || !openSet.Contains(temp))
                    {
                        openSet.Enqueue(temp);
                        temp.gCost = gCost;
                        temp.hCost = GetDistanceNodes(temp, targetNode);
                        temp.parent = template;
                    }

                }
            }
            return default;
        }
        /// <summary>
        /// 生成路径
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        private Vector3[] GeneratePath(MapNode startNode, MapNode endNode)
        {
            Stack<Vector3> path = new Stack<Vector3>();
            MapNode node = endNode;
            while (node.parent != startNode)
            {
                path.Push(node.center);
                node = node.parent;
            }
            return path.ToArray();
        }
        private List<MapNode> FindSubNode(MapNode current)
        {
            List<MapNode> nodes = new List<MapNode>();
            int offset_x = current.id % RowCount;
            int offset_y = current.id / RowCount;
            if (offset_x - 1 >= 0 && offset_x + 1 < RowCount)
            {
                nodes.Add(maps[offset_y, offset_x - 1]);
                nodes.Add(maps[offset_y, offset_x + 1]);
            }
            if (offset_y - 1 >= 0 && offset_y + 1 < RowCount)
            {
                nodes.Add(maps[offset_y + 1, offset_x]);
                nodes.Add(maps[offset_y - 1, offset_x]);
            }
            return nodes;
        }

        /// <summary>
        /// 获得两个节点的距离
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        private int GetDistanceNodes(MapNode node1, MapNode node2)
        {
            int deltaX = Mathf.Abs(node1.ColIndex - node2.ColIndex);
            int deltaY = Mathf.Abs(node1.RowIndex - node2.RowIndex);
            if (deltaX > deltaY)
            {
                return deltaY * 14 + 10 * (deltaX - deltaY);
            }
            else
            {
                return deltaX * 14 + 10 * (deltaY - deltaX);
            }
        }


        class MapNode : GObject
        {
            private float gridSize;
            private int rowCount;
            public bool isWall { get; private set; }
            public int hCost;//当前点到终点的距离
            public int gCost;//起始点到当前点的距离

            public MapNode parent;
            public Vector3 center
            {
                get;
            }

            public Vector3 size
            {
                get;
            }

            public int id
            {
                get;
            }

            public float fCost { get { return hCost + gCost; } }

            public int RowIndex
            {
                get
                {
                    return id / rowCount;
                }
            }

            public int ColIndex
            {
                get
                {
                    return id % rowCount;
                }
            }
            public void Dispose()
            {

            }

            public MapNode(int id, int rowCount, float offset_x, float offset_y, float gridSize, bool isWall)
            {
                this.id = id;
                this.isWall = isWall;
                this.rowCount = rowCount;
                this.gridSize = gridSize / 2;
                center = new Vector3(offset_x, 0, offset_y);
                size = new Vector3(gridSize, 0, gridSize);
            }

            public bool HasContains(Vector3 position)
            {
                bool x = position.x > center.x - gridSize && position.x < center.x + gridSize;
                bool z = position.z > center.z - gridSize && position.z < center.z + gridSize;
                return x && z;
            }

            public void Open()
            {
                isWall = true;
            }

            public void Closed()
            {
                isWall = false;
            }
        }
    }
}