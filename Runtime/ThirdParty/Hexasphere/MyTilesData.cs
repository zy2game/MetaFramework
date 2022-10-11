using System;
using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;
using System.Linq;
public class BlockData
{
    public struct TileData
    {
        public int index;
        public bool used;
    }

    public TileData[] blocks = new TileData[7];
    public BlockData(int[] in_data)
    {

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].index = i;
            blocks[i].used = false;
        }

        for (int i = 0; i < in_data.Length; i++)
        {
            int index = in_data[i];
            blocks[index].used = true;
        }
    }




    /// <summary>
    /// ��ת��
    /// </summary>
    /// <param name="times">��ת������˳ʱ��Ϊ �� </param>
    /// <remarks>Ϊ0 ʱ �����ģ�����仯�� ����������1��ʼ</remarks>
    public void BlockRotate(int times=1)
    {
       
       for (int i = 1; i < blocks.Length; i++)
       {
           blocks[i] .index+= times ;
           if (blocks[i].index > 6) blocks[i].index -=6;
            if (blocks[i].index <1) blocks[i].index += 6;
        }
      
    }
}

public class PlacedData
{
    public int TileIndex;
    public int BlockIndex;
}

public class MyTilesData
{
    // �洢ÿһ�� tile �Ƿ��Ѿ���ռ�õ�bool ֵ, int��tile��index
    private Dictionary<int, bool> dicTilsUsed = new Dictionary<int, bool>();
    private Hexasphere hexasphere;

    public MyTilesData(Hexasphere hexasphere)
    {
        this.hexasphere = hexasphere;
        foreach (var tile in this.hexasphere.tiles)
        {
            dicTilsUsed.Add(tile.index, false);
        }
    }


    public List<int> GetUsedTiles()
    {
        List<int> list = new List<int>();
        foreach (var t in dicTilsUsed)
        {
            if (t.Value)
            {
                list.Add(t.Key);
            }
        }
        return list;
    }

    public void Init(Dictionary<int, bool> usedTiles)
    {
        foreach (var t in usedTiles)
        {
            dicTilsUsed[t.Key] = t.Value;
        }
    }

    private void SetTileState(int tileIndex, bool used)
    {
        dicTilsUsed[tileIndex] = used;
    }

    private bool GetTileState(int tileIndex)
    {
        return dicTilsUsed[tileIndex];
    }

    //================================

    private int[] SortedIndices(int tileIndex)
    {
        int[] neighbours = hexasphere.GetTileNeighbours(tileIndex);
        int[] indices = new int[neighbours.Length + 1];
        indices[0] = tileIndex;
        Array.Copy(neighbours, 0, indices, 1, neighbours.Length);

        int[] tempArr = new int[indices.Length];
        tempArr[0] = indices[0];
        //tempArr[1] = indices[1];
        //tempArr[2] = indices[5];
        //tempArr[3] = indices[6];
        //tempArr[4] = indices[3];
        //tempArr[5] = indices[4];
        //tempArr[6] = indices[2];

        Vector3[] vertices = hexasphere.tiles[tempArr[0]].vertices;

        for (int i = 1; i <= 6; i++)
        {
            Vector3[] perVertices = hexasphere.tiles[indices[i]].vertices;
            if (perVertices.Contains(vertices[5]) && perVertices.Contains(vertices[0]))
            {
                tempArr[1] = indices[i];
            }
            else if (perVertices.Contains(vertices[0]) && perVertices.Contains(vertices[1]))
            {
                tempArr[2] = indices[i];
            }
            else if (perVertices.Contains(vertices[1]) && perVertices.Contains(vertices[2]))
            {
                tempArr[3] = indices[i];
            }
            else if (perVertices.Contains(vertices[2]) && perVertices.Contains(vertices[3]))
            {
                tempArr[4] = indices[i];
            }
            else if (perVertices.Contains(vertices[3]) && perVertices.Contains(vertices[4]))
            {
                tempArr[5] = indices[i];
            }
            else if (perVertices.Contains(vertices[4]) && perVertices.Contains(vertices[5]))
            {
                tempArr[6] = indices[i];
            }
        }


        return tempArr;
    }

    /// <summary>
    /// �ܲ��ܷ��ÿ�
    /// </summary>
    /// <param name="tileIndex">��ǰ��tile������Ҳ���Ƿ��õ�����</param>
    /// <param name="block">������</param>
    public bool CanPlace(int tileIndex, BlockData block)
    {
        if (hexasphere.tiles[tileIndex].vertices.Length == 5)
        {
            Debug.LogError("��λ����5���Σ����ܷ���");
            return false;
        }

        int[] indices = SortedIndices(tileIndex);

        bool used = false;

        for (int i = 0; i < block.blocks.Length; i++)
        {
            int index = indices[block.blocks[i].index];
            used = GetTileState(index);
            if (used && block.blocks[i].used)
            {
                UnityEngine.Debug.LogError(i + "   " + block.blocks[i].index + "      " + index);
                return false;

            } else
            {
                if (hexasphere.tiles[index].vertices.Length == 5 && block.blocks[i].used)
                {
                    UnityEngine.Debug.LogError("�˵ؿ���5���Σ����ܷ��ã� �뿼����ת��");
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// ���õ�ǰ��
    /// </summary>
    /// <param name="tileIndex"></param>
    /// <param name="block"></param>
    /// <returns>���ص��ǵؿ�Ķ�Ӧ��ϵ�б� eg. 209 => 1 </returns>
    public List<PlacedData> PlaceBlock(int tileIndex, BlockData block)
    {
        int[] indices = SortedIndices(tileIndex);

        List<PlacedData> placed = new List<PlacedData>();

        for (int i = 0; i < block.blocks.Length; i++)
        {
            int index = indices[block.blocks[i].index];
            if (block.blocks[i].used)
            {
                SetTileState(index, true);
                placed.Add(new PlacedData() { TileIndex = index, BlockIndex = block.blocks[i].index });

            }
        }



        return placed;
    }

    /// <summary>
    /// ȡ�����õ�ǰ��
    /// </summary>
    /// <param name="tileIndex"></param>
    /// <param name="block"></param>
    public void UnPlaceBlock(int tileIndex, BlockData block)
    {
        int[] indices = hexasphere.tiles[tileIndex].neighboursIndices;
        for (int i = 0; i < block.blocks.Length; i++)
        {
            int index = indices[block.blocks[i].index];
            SetTileState(index, false);
        }
    }




    public Matrix4x4 CalcMatrix(int tileIndex)
    {
        Vector3 pos0 = hexasphere.tiles[tileIndex].polygonCenter;
        Vector3 pos1 = hexasphere.tiles[tileIndex].vertices[0];

        Vector3 N = pos0.normalized;
        Vector4 N4 = new Vector4(N.x, N.y, N.z, 0);

        Vector3 T = (pos1 - pos0).normalized;
        Vector4 T4 = new Vector4(T.x, T.y, T.z, 0);

        Vector3 B = Vector3.Cross(N, T);
        Vector4 B4 = new Vector4(B.x,B.y,B.z,0);

        Matrix4x4 mtx = new Matrix4x4(N4,T4,B4,new Vector4(0,0,0,1));


        

        return mtx;
        
    }

}


