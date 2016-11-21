using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace XYZMap
{
    class TileLine
    {
        //LineString, MultiLineString
        MapTile tile;
        JSONObject data;
        GameObject parent;
        GameObject gameObject;
        Vector2 center;
        static private List<int> built = new List<int>();

        float lat, lng;
        public TileLine(MapTile tile, JSONObject data, GameObject parent, Color color, float height = 1, bool checkId = false)
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

                if( checkId && data[i]["properties"]["id"] != null)
                {
                    int id = (int)data[i]["properties"]["id"].n;
                    if (checkId && built.IndexOf(id) != -1) continue;
                    built.Add(id);
                }

                JSONObject geometry = data[i]["geometry"];

                if ( geometry["type"].str == "LineString" )
                {
                    processSegment(tileCenter, geometry["coordinates"], ref tmpIndices, ref tmpVertices);
                }
                
                if ( geometry["type"].str == "MultiLineString")
                {
                    for (int j = 0; j < geometry["coordinates"].Count; j++)
                    {
                        JSONObject poly = geometry["coordinates"][j];
                        processSegment(tileCenter, poly, ref tmpIndices, ref tmpVertices);
                    }
                }
                //*/
            }


            // Create the mesh
            Mesh mesh = new Mesh();
            mesh.vertices = tmpVertices.ToArray();
            mesh.triangles = tmpIndices.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            gameObject = new GameObject();
            gameObject.transform.parent = parent.transform;
            gameObject.hideFlags = HideFlags.HideInHierarchy;

            float[] p = tile.map.latLonToPixels(lat, lng);
            gameObject.transform.position = new Vector3(p[0], height, -p[1]);

            gameObject.AddComponent(typeof(MeshRenderer));
            MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = mesh;

            Renderer renderer = gameObject.GetComponent<Renderer>();
            renderer.material.color = color;

        }


        Vector3 normal( Vector3 a, Vector3 b)
        {
            Vector3 v = new Vector3(-(b.y - a.y), ( a.y + b.y ) / 2, (b.x - a.x));
            v.Normalize();
            return v;
        }

        public void processSegment(float[] center, JSONObject polygon, ref List<int> tmpIndices, ref List<Vector3> tmpVertices)
        {

            float[] pos = tile.map.latLonToPixels(polygon[0][1].n, polygon[0][0].n);
            Vector3 a = new Vector3(pos[0] - center[0] - tile.map.tileSize / 2, 5, -pos[1] + center[1] + tile.map.tileSize / 2);

            pos = tile.map.latLonToPixels(polygon[1][1].n, polygon[1][0].n);
            Vector3 b = new Vector3(pos[0] - center[0] - tile.map.tileSize / 2, 5, -pos[1] + center[1] + tile.map.tileSize / 2);

            float width = 5 * 1 / tile.map.resolution(tile.map.zoom);
            Vector3 norm = normal( a, b ) * width;
            Vector3 ln0 = a + norm;
            Vector3 ln1 = b + norm;

            norm = normal( b, a ) * width;
            Vector3 rn0 = a + norm;
            Vector3 rn1 = b + norm;

            int id = tmpVertices.Count;

            tmpIndices.Add(id);
            tmpIndices.Add(id+1);
            tmpIndices.Add(id+2);

            tmpIndices.Add(id+2);
            tmpIndices.Add(id+3);
            tmpIndices.Add(id);
            

            tmpVertices.Add(ln0);
            tmpVertices.Add(ln1);
            tmpVertices.Add(rn1);
            tmpVertices.Add(rn0);


            /*
            LineBuffer.addPoint(v);
            }
            try{
                //Debug.Log("ok");// geometry["type"].str + " " + geometry["coordinates"].Count);
            }
            catch(Exception e)
            {
                //Debug.Log("no " + e.ToString() );// geometry["type"].str + " " + geometry["coordinates"].Count);

            }
            //*/
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
