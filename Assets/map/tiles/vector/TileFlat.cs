
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace XYZMap
{
    public class TileFlat
    {
        MapTile tile;
        JSONObject data;
        GameObject parent;
        GameObject geom;
        Vector2 center;
        static private List<int> built = new List<int>();

        float lat, lng;
        public TileFlat(MapTile tile, JSONObject data, GameObject parent, Color color, float height = 1, bool checkId = true)
        {

            this.tile = tile;
            this.data = data;
            this.parent = parent;

            List<int> tmpIndices = new List<int>();
            List<Vector3> tmpVertices = new List<Vector3>();
            
            lat = tile.lat;
            lng = tile.lng;
            float[] pp = tile.map.latLonToPixels(lat, lng);
            //Debug.Log(pp[0] + " " + pp[1]);
            float h = height;
            for (int i = 0; i < data.Count; i++)
            {
                int id = (int)data[i]["properties"]["id"].n;
                if (checkId && built.IndexOf(id) != -1) continue;
                built.Add(id);

                if (data[i]["properties"]["height"] != null)
                {
                    h = data[i]["properties"]["height"].n;
                    h *= 1 / tile.map.resolution(tile.map.zoom);
                }
                if (data[i]["properties"]["min_height"] != null)
                {
                    h = data[i]["properties"]["min_height"].n;
                    h *= 1 / tile.map.resolution(tile.map.zoom);
                }

                if (data[i]["geometry"]["type"].str == "Polygon")
                {
                    JSONObject bunch = data[i]["geometry"]["coordinates"];
                    for (int k = 0; k < bunch.Count; k++)
                    {
                        JSONObject poly = bunch[k];
                        int count = poly.Count;

                        List<Vector2> vertices2D = new List<Vector2>();
                        for (int j = 0; j < count; j++)
                        {
                            float[] pos = tile.map.latLonToPixels(poly[j][1].n, poly[j][0].n );
                            Vector2 v = new Vector2(poly[j][1].n, poly[j][0].n);
                            vertices2D.Add(v);
                        }
                        vertices2D.Reverse();
                        Triangulator tr = new Triangulator(vertices2D.Distinct().ToList<Vector2>());
                        int[] ids = tr.Triangulate();
                        
                        //int[] ids = triangulate(vertices2D.Distinct().ToList<Vector2>());

                        if (ids.Length == 0)
                        {
                            Debug.Log("fuck 0");
                        }
                        if (ids.Length < vertices2D.Count - 2)
                        {
                            Debug.Log("fuck < n-2");

                        }
                        
                        for (int j = 0; j < ids.Length; j++)
                        {
                            tmpIndices.Add( tmpVertices.Count + ids[j] );
                        }
                        for (int j = 0; j < count; j++)
                        {
                            float[] pos = tile.map.latLonToPixels(vertices2D[j].x, vertices2D[j].y );
                            Vector3 v = new Vector3(pos[0] - pp[0] - tile.map.tileSize / 2, h, -pos[1] + pp[1]+ tile.map.tileSize / 2);
                            tmpVertices.Add(v);
                        }
                    }
                }
            }
            
            //tmpIndices.Reverse();
            int[] tmp = tmpIndices.ToArray();

            //hard coded double side
            int[] indices = new int[tmp.Length * 2];
            for (int i = 0; i < tmp.Length; i++)
            {
                indices[i] = tmp[i];
            }
            tmp.Reverse();
            for (int i = 0; i < tmp.Length; i++)
            {
                indices[i] = tmp[i];
            }

            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = tmpVertices.ToArray();
            msh.triangles = indices;
            
            msh.RecalculateNormals();
            msh.RecalculateBounds();

            geom = new GameObject();
            geom.transform.parent = parent.transform;
            geom.hideFlags = HideFlags.HideInHierarchy;

            float[] p = tile.map.latLonToPixels(lat, lng);
            geom.transform.position = new Vector3(p[0], 2, -p[1]);

            geom.AddComponent(typeof(MeshRenderer));

            MeshFilter filter = geom.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = msh;

            Renderer renderer = geom.GetComponent<Renderer>();
            //renderer.material = new Material(  Shader.Find("Toon/Lit Outline"));//Shader.Find("Unlit/Color"));//
            renderer.material.color = color;// new Color(0.1f, 0.1f, 0.1f);
            //renderer.material.SetFloat("_Outline", .001f);

            //renderer.material = new Material( Shader.Find("Unlit/Color"));
            renderer.material.SetFloat("_Metallic", .5f);
            renderer.material.SetFloat("_Glossiness", .8f);
            
        }
        
        public void Update(bool active)
        {
            geom.SetActive(active);
            if (active)
            {
                float[] p = tile.map.latLonToPixels(lat, lng);
                geom.transform.position = new Vector3(p[0], 0, -p[1]);
            }
        }

    }
}