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
        private Color color;

        static private List<int> built = new List<int>();
        float lat, lng;
        public TileLine(MapTile tile, JSONObject data, GameObject parent, Color color, List<string> type, float width = 1, bool checkId = false )
        {

            this.tile = tile;
            this.data = data;
            this.parent = parent;
            this.color = color;

            List<int> tmpIndices = new List<int>();
            List<Vector3> tmpVertices = new List<Vector3>();

            lat = tile.lat;
            lng = tile.lng;
            float[] tileCenter = tile.map.latLonToPixels(lat, lng);

            for (int i = 0; i < data.Count; i++)
            {
                JSONObject properties = data[i]["properties"];
                if( checkId && properties["id"] != null)
                {
                    int id = (int)properties["id"].n;
                    if (checkId && built.IndexOf(id) != -1) continue;
                    built.Add(id);
                }

                string kind = properties["kind"].str;
                if (type.IndexOf(kind) == -1)
                {
                    Debug.Log(kind);
                    continue;
                }

                
                JSONObject geometry = data[i]["geometry"];
                if ( geometry["type"].str == "LineString" )
                {
                    
                    JSONObject polygon = geometry["coordinates"];
                    for (int k = 0; k < polygon.Count - 1; k++)
                    {
                        float[] pos = tile.map.latLonToPixels(polygon[k][1].n, polygon[k][0].n);
                        Vector3 a = new Vector3(pos[0] - tileCenter[0] - tile.map.tileSize / 2, 0, -pos[1] + tileCenter[1] + tile.map.tileSize / 2);

                        pos = tile.map.latLonToPixels(polygon[k + 1][1].n, polygon[k + 1][0].n);
                        Vector3 b = new Vector3(pos[0] - tileCenter[0] - tile.map.tileSize / 2, 0, -pos[1] + tileCenter[1] + tile.map.tileSize / 2);

                        appendSegment(a, b, width, ref tmpIndices, ref tmpVertices);
                    }

                    processSegment(tileCenter, width, geometry["coordinates"], ref tmpIndices, ref tmpVertices);
                }
                
                if ( geometry["type"].str == "MultiLineString")
                {

                    for (int j = 0; j < geometry["coordinates"].Count; j++ )
                    {
                        JSONObject polygon = geometry["coordinates"];
                        
                        for (int k = 0; k < polygon[ j ].Count - 1; k++)
                        {
                            JSONObject subpolygon = polygon[ j ];

                            float[] pos = tile.map.latLonToPixels(subpolygon[ k ][1].n, subpolygon[ k ][0].n);
                            Vector3 a = new Vector3(pos[0] - tileCenter[0] - tile.map.tileSize / 2, 0, -pos[1] + tileCenter[1] + tile.map.tileSize / 2);

                            pos = tile.map.latLonToPixels(subpolygon[k + 1][1].n, subpolygon[k + 1][0].n);
                            Vector3 b = new Vector3(pos[0] - tileCenter[0] - tile.map.tileSize / 2, 0, -pos[1] + tileCenter[1] + tile.map.tileSize / 2);

                            appendSegment(a, b, width, ref tmpIndices, ref tmpVertices);
                            //Debug.Log( i + " multi " + j + " " + a.x + " " + b.x);
                        }
                    }
                }
            }

            commitMesh(ref tmpIndices, ref tmpVertices);

        }

        void commitMesh( ref List<int> tmpIndices, ref List<Vector3> tmpVertices )
        {

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
            gameObject.transform.position = new Vector3(p[0], 0, -p[1]);

            gameObject.AddComponent(typeof(MeshRenderer));
            MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = mesh;

            Renderer renderer = gameObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Unlit/Color"));
            renderer.material.SetColor("_Color", color);

            tmpIndices.Clear();
            tmpIndices.Clear();

        }

        Vector3 normal( Vector3 a, Vector3 b, float width = 1)
        {
            Vector3 v = new Vector3(-(b.z - a.z), 0, (b.x - a.x));
            v.Normalize();
            v *= width;
            return v;
        }

        public void processSegment(float[] center, float width, JSONObject polygon, ref List<int> tmpIndices, ref List<Vector3> tmpVertices)
        {
            
            float[] pos = tile.map.latLonToPixels(polygon[0][1].n, polygon[0][0].n);
            Vector3 a = new Vector3(pos[0] - center[0] - tile.map.tileSize / 2, 0, -pos[1] + center[1] + tile.map.tileSize / 2);

            pos = tile.map.latLonToPixels(polygon[1][1].n, polygon[1][0].n);
            Vector3 b = new Vector3(pos[0] - center[0] - tile.map.tileSize / 2, 0, -pos[1] + center[1] + tile.map.tileSize / 2);

            appendSegment(a, b, width, ref tmpIndices, ref tmpVertices);

        }

        private void appendSegment( Vector3 a, Vector3 b, float width, ref List<int> tmpIndices, ref List<Vector3> tmpVertices) { 
            
            //prevent buffer overflow
            if (tmpVertices.Count + 4 >= Mathf.Pow(2, 16))
            {
                commitMesh(ref tmpIndices, ref tmpVertices);
            }

            Vector3 norm = normal( a, b, width * .5f );
            Vector3 ln0 = a + norm;
            Vector3 ln1 = b + norm;

            norm = normal( b, a, width * .5f);
            Vector3 rn0 = a + norm;
            Vector3 rn1 = b + norm;

            int id = tmpVertices.Count;

            //front
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
            
        }

        public void Update(bool active)
        {
            gameObject.SetActive(active);

            if (tile.map.parent.renderToTexture)
            {
                Vector3 pos = tile.map.parent.gameObject.transform.position;
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
