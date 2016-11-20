﻿
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
        GameObject gameObject;
        Vector2 center;
        static private List<int> built = new List<int>();

        float lat, lng;
        public TileFlat(MapTile tile, JSONObject data, GameObject parent, Color color, float height = 1, bool checkId = false )
        {

            this.tile = tile;
            this.data = data;
            this.parent = parent;

            List<int> tmpIndices = new List<int>();
            List<Vector3> tmpVertices = new List<Vector3>();
            
            lat = tile.lat;
            lng = tile.lng;
            float[] tileCenter = tile.map.latLonToPixels(lat, lng);
            
            for (int i = 0; i < data.Count; i++)
            {
                int id = (int)data[i]["properties"]["id"].n;
                if (checkId && built.IndexOf(id) != -1) continue;
                built.Add(id);
                
                if (data[i]["geometry"]["type"].str == "Polygon")
                {
                    processPolygon( tileCenter, data[i]["geometry"]["coordinates"], ref tmpIndices, ref tmpVertices);
                }
                else if( data[i]["geometry"]["type"].str == "MultiPolygon")
                {
                    for( int j =0; j < data[i]["geometry"]["coordinates"].Count; j++)
                    {
                        JSONObject poly = data[i]["geometry"]["coordinates"][j];
                        processPolygon( tileCenter, poly, ref tmpIndices, ref tmpVertices);
                    }
                }
            }
            
            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = tmpVertices.ToArray();
            msh.triangles = tmpIndices.ToArray();
            
            msh.RecalculateNormals();
            msh.RecalculateBounds();

            gameObject = new GameObject();
            gameObject.transform.parent = parent.transform;
            gameObject.hideFlags = HideFlags.HideInHierarchy;

            float[] p = tile.map.latLonToPixels(lat, lng);
            gameObject.transform.position = new Vector3(p[0], height, -p[1]);

            gameObject.AddComponent(typeof(MeshRenderer));

            MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = msh;

            Renderer renderer = gameObject.GetComponent<Renderer>();
            //renderer.material = new Material(  Shader.Find("Toon/Lit Outline"));//Shader.Find("Unlit/Color"));//
            renderer.material.color = color;// new Color(0.1f, 0.1f, 0.1f);
            //renderer.material.SetFloat("_Outline", .001f);

            //renderer.material = new Material( Shader.Find("Unlit/Color"));
            renderer.material.SetFloat("_Metallic", .5f);
            renderer.material.SetFloat("_Glossiness", .8f);
            
        }
        
        public void processPolygon( float[] center, JSONObject polygon, ref List<int> tmpIndices, ref List<Vector3> tmpVertices )
        {

            for (int k = 0; k < polygon.Count; k++)
            {
                JSONObject poly = polygon[k];
                int count = poly.Count;

                List<Vector2> vertices2D = new List<Vector2>();
                for (int j = 0; j < count; j++)
                {
                    float[] pos = tile.map.latLonToPixels(poly[j][1].n, poly[j][0].n);
                    Vector2 v = new Vector2(poly[j][1].n, poly[j][0].n);
                    vertices2D.Add(v);
                }
                vertices2D.Reverse();

                Triangulator tr = new Triangulator(vertices2D.Distinct().ToList<Vector2>());
                int[] ids = tr.Triangulate();

                if (ids.Length == 0)
                {
                    Debug.LogWarning("couldn't triangulate object in tile " + tile.quadKey );
                    continue;
                }

                for (int j = 0; j < ids.Length; j++)
                {
                    tmpIndices.Add(tmpVertices.Count + ids[j]);
                }
                for (int j = 0; j < count; j++)
                {
                    float[] pos = tile.map.latLonToPixels(vertices2D[j].x, vertices2D[j].y);
                    Vector3 v = new Vector3(pos[0] - center[0] - tile.map.tileSize / 2, 0, -pos[1] + center[1] + tile.map.tileSize / 2);
                    tmpVertices.Add(v);
                }
            }
        }

        public void Update(bool active)
        {
            gameObject.SetActive(active);

            Vector3 pos = tile.map.parent.gameObject.transform.position;

            if (tile.map.parent.renderToTexture)
            {
                gameObject.hideFlags = HideFlags.HideInHierarchy;
                pos.y = tile.map.parent.orthographicCamera.transform.position.y;
            }
            if (active)
            {
                float[] p = tile.map.latLonToPixels(lat, lng);
                gameObject.transform.position = new Vector3(p[0], 0, -p[1]);
            }
        }

    }
}