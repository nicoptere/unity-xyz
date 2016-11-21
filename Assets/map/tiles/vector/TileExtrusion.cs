
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace XYZMap
{
    public class TileExtrusion
    {
        MapTile tile;
        JSONObject data;
        GameObject parent;
        GameObject gameObject;
        Vector2 center;
        private Color color;

        static private List<int> built = new List<int>();
        float lat, lng;

        public TileExtrusion(MapTile tile, JSONObject data, GameObject parent, Color color, bool checkId = false )
        {

            this.tile = tile;
            this.data = data;
            this.parent = parent;
            this.color = color;
            List<int> tmpIndices = new List<int>();
            List<Vector3> tmpVertices = new List<Vector3>();
            
            for (int i = 0; i < data.Count; i++)
            {
                JSONObject properties = data[i]["properties"];
                
                if (checkId && properties["id"] != null)
                {
                    int id = (int)properties["id"].n;
                    if (built.IndexOf(id) != -1) continue;
                    built.Add(id);
                }
                
                float height = getHeight(data[i]["properties"]);

                JSONObject geometry = data[i]["geometry"];
                if (geometry["type"].str == "Polygon")
                {
                    processPolygon( height, geometry["coordinates"], ref tmpIndices, ref tmpVertices);
                }

                if (geometry["type"].str == "MultiPolygon")
                {
                    for (int j = 0; j < geometry["coordinates"].Count; j++)
                    {
                        JSONObject poly = geometry["coordinates"][j];
                        processPolygon( height, poly, ref tmpIndices, ref tmpVertices);
                    }
                }
            }

            commitMesh(ref tmpIndices, ref tmpVertices);

            
        }

        private float getHeight( JSONObject data)
        {

            float h = 1;
            if (data["height"] != null)
            {
                h = data["height"].n;
            }
            if (data["min_height"] != null)
            {
                h = data["min_height"].n;
            }
            //world scale
            h *= 1 / tile.map.resolution(tile.map.zoom);
            return h;

        }

        public void processPolygon(float height, JSONObject polygon, ref List<int> tmpIndices, ref List<Vector3> tmpVertices)
        {
            for (int k = 0; k < polygon.Count; k++)
            {
                JSONObject poly = polygon[k];
                int count = poly.Count;


                List<Vector2> vertices2D = new List<Vector2>();
                for (int j = 0; j < count; j++)
                {
                    float[] pos = tile.map.latLonToPixels(poly[j][1].n, poly[j][0].n);
                    vertices2D.Add(new Vector2(pos[0], -pos[1]));
                }

                vertices2D = vertices2D.Distinct().ToList<Vector2>();
                count = vertices2D.Count;

                Triangulator tr = new Triangulator(vertices2D);
                int[] ids = tr.Triangulate();
                int offset = tmpVertices.Count;
                int[] inds = processIndices(ids, count, offset);

                //prevent buffer overflow
                if (tmpIndices.Count + inds.Length >= Mathf.Pow(2, 16))
                {
                    commitMesh(ref tmpIndices, ref tmpVertices);
                }

                for (int j = 0; j < inds.Length; j++)
                {
                    tmpIndices.Add(inds[j]);
                }

                for (int j = 0; j < count * 2; j++)
                {
                    float x = vertices2D[j % count].x - tile.map.tileSize / 2;
                    float y = vertices2D[j % count].y + tile.map.tileSize / 2;
                    Vector3 v = new Vector3(x, (j >= count) ? height : 0, y);
                    tmpVertices.Add(v);
                }
            }
        }

        private int[] processIndices(int[] capIndices, int verticesCount, int offset)
        {
            //tesselation sides: extrusion
            var sideIndices = new int[verticesCount * 3 * 2];
            var sides = verticesCount;
            int step = 0, back, j, i0, i1, i2, i3;
            int inc = 0;
            for (int i = 0; i < 2; i++)
            {
                //create faces
                if (step > 0)
                {
                    back = step - sides;
                    for (j = 0; j < sides; j++)
                    {
                        i0 = back + j;
                        i1 = back + (j + 1) % sides;
                        i2 = step + j;
                        i3 = step + (j + 1) % sides;

                        sideIndices[inc++] = i0;
                        sideIndices[inc++] = i1;
                        sideIndices[inc++] = i2;

                        sideIndices[inc++] = i2;
                        sideIndices[inc++] = i1;
                        sideIndices[inc++] = i3;
                    }
                }
                step += sides;
            }

            //merging faces indices
            int[] indices = new int[capIndices.Length + sideIndices.Length ];
            inc = 0;
            for( int i=0; i < capIndices.Length; i++)
            {
                indices[inc++] = capIndices[capIndices.Length - 1 - i] + verticesCount + offset;
            }
            for (int i = 0; i < sideIndices.Length; i++)
            {
                indices[inc++] = sideIndices[i] + offset;
            }
            return indices;
        }

        private void commitMesh( ref List<int> tmpIndices, ref List<Vector3> tmpVertices)
        {
            
            gameObject = new GameObject();
            gameObject.transform.parent = parent.transform;
            gameObject.AddComponent(typeof(MeshRenderer));
            gameObject.hideFlags = HideFlags.HideInHierarchy;
            
            // Create the mesh
            Mesh mesh = new Mesh();
            if (tile.map.parent.flatNormals)
            {
                List<Vector3> vertices = new List<Vector3>();
                List<Color> colors = new List<Color>();
                List<int> indices = new List<int>();
                for (var i = 0; i < tmpIndices.Count; i += 3)
                {

                    int i0 = tmpIndices[i];
                    int i1 = tmpIndices[i + 1];
                    int i2 = tmpIndices[i + 2];

                    vertices.Add(tmpVertices[i0]);
                    vertices.Add(tmpVertices[i1]);
                    vertices.Add(tmpVertices[i2]);

                    colors.Add(new Color(1, 0, 0));
                    colors.Add(new Color(0, 1, 0));
                    colors.Add(new Color(0, 0, 1));

                    indices.Add(i);
                    indices.Add(i + 1);
                    indices.Add(i + 2);

                }
                mesh.vertices = vertices.ToArray();
                mesh.colors = colors.ToArray();
                mesh.triangles = indices.ToArray();
            }
            else
            {
                mesh.vertices = tmpVertices.ToArray();
                mesh.triangles = tmpIndices.ToArray();
            }
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = mesh;

            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (tile.map.parent.flatNormals)
            {
                /*
                Material mat = new Material(Shader.Find("Unlit/Wireframe"));
                mat.SetColor("Color", color);
                renderer.material = mat;
                //*/
            }
            else
            {
                renderer.material.color = color;
            }
                renderer.material.color = color;

            tmpIndices.Clear();
            tmpIndices.Clear();

        }

        public void Update(bool active)
        {
            return;
            gameObject.SetActive(active);
            if (active)
            {
                float[] p = tile.map.latLonToPixels(lat, lng);
                gameObject.transform.position = new Vector3(p[0], 2, -p[1]);

                if (p[0] < -tile.map.width / 2 || p[0] > tile.map.width / 2) active = false;
                if (-p[1] < -tile.map.height / 2 || -p[1] > tile.map.height / 2) active = false;

            }
            Renderer renderer = gameObject.GetComponent<Renderer>();
            renderer.material.color = active == true ? new Color(1, 1, 0) : new Color(0, .3f, .6f);
            
        }

    }
}