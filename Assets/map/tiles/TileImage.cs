using System.Collections.Generic;
using UnityEngine;

namespace XYZMap
{
    class TileImage : MapTile
    {
        public Texture2D texture;
        public TileImage(Map map, string quadKey)
        {
            this.map = map;
            this.quadKey = quadKey;
            initFromQuadKey(quadKey);
        }
        
        public override void onDataLoaded(WWW www)
        {

            if ( gameObject == null)
            {
                GameObject plane = createMesh();
                plane.transform.parent = map.parent.tiles.transform;
                plane.transform.localScale = new Vector3(map.tileSize, 1, map.tileSize);
                gameObject = plane;
            }

            texture = www.texture;

            Renderer renderer = gameObject.GetComponent<Renderer>();
            renderer.material.shader = Shader.Find("Unlit/Texture");
            renderer.material.mainTexture = texture;
            loaded = true;
            Update(true);
        }

        public override void Update(bool active)
        {
            if (!loaded) return;
            Vector3 pos = map.parent.gameObject.transform.position;
            
            if (map.parent.renderToTexture)
            {
                gameObject.hideFlags = HideFlags.HideInHierarchy;
                pos.y = map.parent.orthographicCamera.transform.position.y;
            }

            gameObject.SetActive(active);
            if( active)
            {
                float[] p = map.latLonToPixels(lat, lng);
                gameObject.transform.position = new Vector3(p[0], -1, -p[1]) + pos;
            }
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
            //go.hideFlags = HideFlags.HideInHierarchy;
            if (map.parent.renderToTexture) go.layer = LayerMask.NameToLayer(map.parent.layerName);
            return go;

        }

    }
}
