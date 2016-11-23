using System.Collections.Generic;
using UnityEngine;

namespace XYZMap
{
    class TileImage3d : MapTile
    {
        public Texture2D texture;


        private int texId = 0;
        private Material material;

        public TileImage3d(Map map, string quadKey)
        {
            this.map = map;
            this.quadKey = quadKey;
            initFromQuadKey(quadKey);

            url = getMapUrl( map.parent.elevationProviderUrl );

        }

        public override void onDataLoaded(WWW www)
        {

            if( texId == 0)
            {
                //Debug.Log( "0 >" + url);
                gameObject = createMesh( www.texture );
                gameObject.transform.parent = map.parent.tiles.transform;
                gameObject.transform.localScale = new Vector3(map.tileSize, map.tileSize, map.tileSize);
                
                Renderer renderer = gameObject.GetComponent<Renderer>();
                //renderer.material = new Material();// Shader.Find("Standard") );
                material = renderer.material;

                url = getMapUrl(map.parent.normalProviderUrl);
                loaded = false;
                texId++;

            }
            else if( texId == 1)
            {
                //Debug.Log("1 >" + url);
                material.SetTexture( "_BumpMap", www.texture );
                //material.SetFloat("_Metallic", 0.8f);
                //material.SetFloat("_Glossiness", 0.8f);
                //material.SetColor("_Color", new Color( 0.1f, 0.1f, 0.1f));

                url = getMapUrl(map.parent.diffuseProviderUrl);
                loaded = false;
                texId++;
            }
            else
            {
                //Debug.Log("2 >" + url);
                material.mainTexture = www.texture;
                //material.SetFloat("_Metallic", 0.2f);
                //material.SetFloat("_Glossiness", 0.25f);
                //material.SetColor("_Color", new Color(1,1,1));

                loaded = true;
                texId++;

            }
            

            Update(true);
        }

        public override void Update(bool active)
        {
            if (!loaded) return;
            Vector3 pos = map.parent.gameObject.transform.position;
            
            gameObject.SetActive(active);
            if (active)
            {
                float[] p = map.latLonToPixels(lat, lng);
                gameObject.transform.position = new Vector3(p[0], 0, -p[1]) + pos;
            }
        }



        float lerp(float t, float a, float b) { return a + t * (b - a); }
        float norm(float t, float a, float b) { return (t - a) / (b - a); }
        float _map(float t, float a0, float b0, float a1, float b1 ) { return lerp(norm(t, a0, b0), a1, b1); }
        GameObject createMesh( Texture2D texture)
        {
            GameObject go = new GameObject();
            

            Mesh mesh = new Mesh();

            int xSize = 64;
            int ySize = 64;

            float scale = (float) 1.0 / xSize;
            float mapScale = 1 / map.resolution(map.zoom);

            Color[] colors = texture.GetPixels(0,0,255,255);

            Vector3[] vertices = new Vector3[(xSize + 1) * (ySize + 1)];
            Vector2[] uvs = new Vector2[(xSize + 1) * (ySize + 1)];

            for (int i = 0, y = 0; y <= ySize; y++)
            {
                for (int x = 0; x <= xSize; x++, i++)
                {

                    int px = (int)_map( x, 0, xSize, 0,255 );
                    int py = (int)_map( y, 0, xSize, 0, 255);

                    Color c = texture.GetPixel( px,py );
                    float height = mapScale * (c.r * 256f + c.g + c.b / 256f);// - 32768f;

                    vertices[i] = new Vector3( x*scale, height, y * scale );  
                    uvs[i] = new Vector2(x*scale, y*scale);
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;

            int[] triangles = new int[xSize * ySize * 6];
            for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
            {
                for (int x = 0; x < xSize; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                    triangles[ti + 5] = vi + xSize + 2;
                }
            }
            mesh.triangles = triangles;
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            go.AddComponent(typeof(MeshFilter));
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.AddComponent(typeof(MeshRenderer));

            //go.hideFlags = HideFlags.HideInHierarchy;
            go.SetActive(false);
            return go;

        }
    }
}
