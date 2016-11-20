using XYZMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XYZMap
{
    class TileLoader : MonoBehaviour
    {
        private int maxLoad = 4;
        private int loading;

        private Map map;
        private MapObject parent;
        private List<MapTile> queue;

        public TileLoader() {}

        public void init(Map map, MapObject parent )
        {
            this.map = map;
            this.parent = parent;
            queue = new List<MapTile>();
        }

        public void addTile(MapTile tile)
        {
            
            if( loading >= maxLoad)
            {
                queue.Add(tile);
                return;
            }

            if( tile is TileImage  ){
                if( tile.gameObject == null)
                {
                    GameObject plane = createMesh();// GameObject.CreatePrimitive(PrimitiveType.Quad );
                    plane.transform.parent = parent.tiles.transform;
                    plane.transform.localScale = new Vector3(map.tileSize, 1, map.tileSize);
                    tile.gameObject = plane;
                }
                StartCoroutine(loadImage(tile));

            }
            if( tile is TileVector )
            {
                if( tile.gameObject == null)
                {
                    tile.gameObject = new GameObject();
                    tile.gameObject.transform.parent = parent.tiles.transform;
                }
                StartCoroutine(loadData(tile));
            }
            
        }

        public IEnumerator loadImage(MapTile tile)
        {
            //Debug.Log("loading: " + tile.url);
            loading++;

            WWW www = new WWW(tile.url);
            yield return www;
            tile.onDataLoaded(www);

            loading--;
            if (queue.Count > 0)
            {
                MapTile next = queue[0];
                addTile(next);
                queue.RemoveAt(0);
            }
        }

        //https://tile.mapzen.com/mapzen/vector/v1/all/15/9644/12318.json?api_key=mapzen-foW3wh2
        //https://tile.mapzen.com/mapzen/vector/v1/all/15/9648/12322.json?api_key=mapzen-foW3wh2
        public IEnumerator loadData(MapTile tile)
        {
            Debug.Log("loading: " + tile.url );
            loading++;

            WWW www = new WWW(tile.url);
            yield return www;

            tile.onDataLoaded(www);



            loading--;
            if( queue.Count > 0)
            {
                MapTile next = queue[0];
                addTile(next);
                queue.RemoveAt(0);
            }
            
           // Debug.Log("loaded");
            /*
            string provider = "https://tile.mapzen.com/mapzen/vector/v1/all/{z}/{x}/{y}.json?api_key=mapzen-foW3wh2";
            string url = tile.getMapUrl(provider, null, tile.tx, tile.ty, map.zoom);
            www = new WWW(url);
            yield return www;
            tile.onJSONLoaded( www, container );
            //*/
        }

        GameObject createMesh()
        {
            GameObject go = new GameObject();

            Vector3[] newVertices = new Vector3[] {
                new Vector3( -0.5f, 0, -0.5f ),
                new Vector3(  0.5f, 0, -0.5f ),
                new Vector3(  0.5f, 0,  0.5f ),
                new Vector3( -0.5f, 0,  0.5f ),
            };

            Vector2[] newUV = new Vector2[] {
                new Vector2( 0,0 ),
                new Vector2( 1,0 ),
                new Vector2( 1,1 ),
                new Vector2( 0,1 ),
            };

            int[] newTriangles = new int[] {
                0, 3, 1, 2, 1, 3,   //front
                0, 1, 3, 1, 2, 3    //back
            };

            Mesh mesh = new Mesh();
            mesh.vertices = newVertices;
            mesh.uv = newUV;
            mesh.triangles = newTriangles;
            go.AddComponent(typeof(MeshFilter));
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.AddComponent(typeof(MeshRenderer));
            go.SetActive(false);
            go.hideFlags = HideFlags.HideInHierarchy;
            if ( parent.renderToTexture )go.layer = LayerMask.NameToLayer( parent.layerName );
            return go;

        }

    }
}
