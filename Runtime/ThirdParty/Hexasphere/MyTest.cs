using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;
public class MyTest : MonoBehaviour
{
    public GameObject txt_Index;
    public Shader shader;
    HexasphereGrid.Hexasphere hexasphere;


    private MyTilesData MyTiles;

   
    // Start is called before the first frame update
    IEnumerator Start()
    {
        hexasphere = Hexasphere.GetInstance("Hexasphere");

        Debug.Log(hexasphere.name);
        // hexasphere.OnTileMouseOver ;
        for (int i = 0; i < hexasphere.tiles.Length; i++)
        {
            Tile tile = hexasphere.tiles[i];
            GameObject indexObj = GameObject.Instantiate(txt_Index);
            indexObj.transform.position = hexasphere.transform.TransformPoint(tile.polygonCenter);
            indexObj.GetComponent<TextMesh>().text = tile.index.ToString();
            indexObj.transform.parent = hexasphere.transform;

            Vector3 dir = (indexObj.transform.position - hexasphere.transform.position).normalized;
            indexObj.transform.rotation = Quaternion.LookRotation(-dir);

            // yield return new WaitForSeconds(0.5f);
        }

        yield return null;

        MyTiles = new MyTilesData(hexasphere);


        //Dictionary<int, bool> initData = new Dictionary<int, bool>();
        //initData.Add(209, true);
        //initData.Add(242, true);
        //initData.Add(220, true);
        //initData.Add(222, true);

        //MyTiles.Init(initData);

        BlockData blockData = new BlockData(new int[] { 0, 1,2 });

        int index = 210;
        //  blockData.BlockRotate();
        if ( MyTiles.CanPlace(index, blockData))
        {
            Debug.Log("当前中心tile:"+ index + ",可以放置");
            List<PlacedData> placedList=  MyTiles.PlaceBlock(index, blockData);

            foreach (var p in placedList)
            {
                D.Log("------{0} => {1}-----", "#ff5500", p.BlockIndex,p.TileIndex);
                Transform t= Instantiate<Transform>(GameObject.FindObjectOfType<BlockConfig>().data[p.BlockIndex].trans);
                t.parent = null;
                t.position = hexasphere.tiles[p.TileIndex].polygonCenter;
                t.rotation = Quaternion.LookRotation(t.position);

            }
           
        }
        else
        {
            Debug.Log("当前中心tile:"+ index + ",不能放置");
        }


        blockData = new BlockData(new int[] { 0, 1, 3, 4 });

        blockData.BlockRotate(2);     

         index = 207;
        if (MyTiles.CanPlace(index, blockData))
        {
            Debug.Log("111当前中心tile:"+ index + ",可以放置");
            MyTiles.PlaceBlock(index, blockData);
        }
        else
        {
            

            Debug.Log("111当前中心tile:"+ index + ",不能放置");
        }


        foreach (var t in MyTiles.GetUsedTiles())
        {
            hexasphere.SetTileMaterial(t, new Material(shader), true);
            Debug.Log(t);
        }
    }

    private void OnDrawGizmos()
    {
        //Debug.Log("OnDrawGizmos");
        if (hexasphere == null) return;

        #region 验证顶点顺序
        //float d = 0.003f;
        //foreach (var v in hexasphere.tiles[67].vertices)
        //{

        //    Gizmos.DrawSphere(hexasphere.transform.TransformPoint(v), d);
        //    d += 0.003f;
        //}


        //d = 0.003f;
        //foreach (var v in hexasphere.tiles[60].vertices)
        //{

        //    Gizmos.DrawSphere(hexasphere.transform.TransformPoint(v), d);
        //    d += 0.003f;
        //}


        //d = 0.003f;
        //foreach (var v in hexasphere.tiles[90].vertices)
        //{

        //    Gizmos.DrawSphere(hexasphere.transform.TransformPoint(v), d);
        //    d += 0.003f;
        //}
        #endregion

        #region 验证邻居6边形顺序

        //int[] neighbours = hexasphere.tiles[67].neighboursIndices;
        //for (int i = 0; i < neighbours.Length; i++)
        //{
        //    Vector3 pc = hexasphere.tiles[neighbours[i]].polygonCenter;
        //    Gizmos.DrawSphere(hexasphere.transform.TransformPoint(pc), 0.003f*(i+1));
        //}



        // neighbours = hexasphere.tiles[60].neighboursIndices;
        //for (int i = 0; i < neighbours.Length; i++)
        //{
        //    Vector3 pc = hexasphere.tiles[neighbours[i]].polygonCenter;
        //    Gizmos.DrawSphere(hexasphere.transform.TransformPoint(pc), 0.003f * (i + 1));
        //}


        //neighbours = hexasphere.tiles[90].neighboursIndices;
        //for (int i = 0; i < neighbours.Length; i++)
        //{
        //    Vector3 pc = hexasphere.tiles[neighbours[i]].polygonCenter;
        //    Gizmos.DrawSphere(hexasphere.transform.TransformPoint(pc), 0.003f * (i + 1));
        //}


        #endregion

        //for (int i=0; i<  hexasphere.tiles.Length;i+=12)
        // {
        //     Tile tile = hexasphere.tiles[i];
        //     //tile.polygonCenter
        //     Gizmos.DrawSphere( hexasphere.transform.TransformPoint(  tile.polygonCenter), 0.003f);

        //     float d = 0.003f;
        //     foreach (var v in tile.vertices)
        //     {
        //         Gizmos.DrawSphere(hexasphere.transform.TransformPoint(v), d);
        //         d += 0.001f;
        //     } 


        // }
    }
    // Update is called once per frame
    void Update()
    {
       
    }
}
