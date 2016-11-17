using System.Collections.Generic;
using UnityEngine;

namespace Assets.map.extra
{
    public class TileExtrusion
    {
        MapTile tile;
        JSONObject data;
        GameObject parent;
        GameObject geom;
        Vector2 center;
        static private List<int> built = new List<int>();

        float lat, lng;
        public TileExtrusion(MapTile tile, JSONObject data, GameObject parent, Color color, bool checkId = true )
        {

            this.tile = tile;
            this.data = data;
            this.parent = parent;

            List<int> tmpIndices = new List<int>();
            List<Vector3> tmpVertices = new List<Vector3>();
            

            float thickness = 0;
            for (int i = 0; i < data.Count; i++)
            {
                float h = 3;
                if( checkId )
                {
                    h *= 10 * Random.value;
                    int id = (int)data[i]["properties"]["id"].n;
                    if (built.IndexOf(id) != -1) continue;
                    built.Add(id);
                
                    if (data[i]["properties"]["height"] != null)
                    {
                        h = data[i]["properties"]["height"].n;
                        //Debug.Log("height" + h);
                    }
                    if (data[i]["properties"]["min_height"] != null) {
                        h = data[i]["properties"]["min_height"].n;
                        //Debug.Log("min_height" + h);
                    }
                }

                h *= 1 / tile.map.resolution(tile.map.zoom);

                if ( data[i]["geometry"]["type"].str == "Polygon")
                {
                    JSONObject bunch = data[i]["geometry"]["coordinates"];
                    for (int k = 0; k < bunch.Count; k++)
                    {

                        JSONObject poly = bunch[k];

                        int count = poly.Count - 1;

                        List<Vector2> vertices2D = new List<Vector2>();
                        for (int j = 0; j < count; j++)
                        {
                            float[] pos = tile.map.latLonToPixels(poly[j][1].n, poly[j][0].n);
                            vertices2D.Add(new Vector2(pos[0], -pos[1]));
                        }


                        Triangulator tr = new Triangulator(vertices2D);
                        int[] ids = tr.Triangulate();
                        

                        if (ids.Length == 0) continue;
                        if( ids.Length < vertices2D.Count - 2)
                        {
                            Debug.Log("fuck");
                        }

                        int offset = tmpVertices.Count;
                        int[] inds = processIndices(ids, count, offset);
                        for (int j = 0; j < inds.Length; j++)
                        {
                            tmpIndices.Add(inds[j]);
                        }

                        for (int j = 0; j < count * 2; j++)
                        {
                            float x = vertices2D[j % count].x - tile.map.tileSize / 2;
                            float y = vertices2D[j % count].y + tile.map.tileSize / 2;

                            float d = Mathf.Max(1 - Mathf.Sqrt(-x * -x + -y * -y) / (tile.map.width / 2), 0);
                            d = Mathf.Pow(d, 16) * (3 - 2 * d);
                            thickness = Mathf.Max(thickness, d);// doubleExponentialSeat( d * .5f, .25f ) );

                            Vector3 v = new Vector3(x, (j < count) ? 0 : h, y);
                            tmpVertices.Add(v);

                        }
                    }
                }
            }
            
            Vector3[] vertices = new Vector3[ tmpVertices.Count ];
            for (int i = 0; i < tmpVertices.Count; i++)
            {
                vertices[i] = tmpVertices[i];
                //if( i   < 10 ) Debug.Log(vertices[i]);
            }
            int[] indices = new int[ tmpIndices.Count ];
            for(int i = 0; i < tmpIndices.Count; i++ )
            {
                indices[i] = tmpIndices[i];
                //if (i < 10) Debug.Log(indices[i]);
            }

            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = vertices;
            msh.triangles = indices;
            msh.RecalculateNormals();
            msh.RecalculateBounds();
            
            geom = new GameObject();
            geom.transform.parent = parent.transform;
            geom.AddComponent(typeof(MeshRenderer));
            
            MeshFilter filter = geom.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = msh;

            Renderer renderer = geom.GetComponent<Renderer>();
            //renderer.material = new Material(Shader.Find("Toon/Lit Outline"));// Shader.Find("Unlit/Color"));// 
            renderer.material.color = color;// new Color(0.1f, 0.1f, 0.1f);
            //renderer.material.SetFloat("_Outline", .001f);

            //renderer.material = new Material( Shader.Find("Unlit/Color"));
            renderer.material.SetFloat("_Metallic", .5f);
            renderer.material.SetFloat("_Glossiness", .8f);

            /*
            Color c = new Color(1,.65f,0);
            Color c1 = Color.yellow;
            LineRenderer lineRenderer = geom.AddComponent<LineRenderer>();
            lineRenderer.material = new Material( Shader.Find("Particles/Additive"));//Shader.Find("Unlit/Color"));// 
            //lineRenderer.material.color = c;
            
            lineRenderer.SetColors(c1, c);
            lineRenderer.SetWidth( 1 + thickness * 4, 1 + thickness * 4 );
            lineRenderer.SetVertexCount(vertices.Length);
            lineRenderer.SetPositions(vertices);
            //*/
        }
        float doubleExponentialSeat(float x, float a)
        {

            float epsilon = 0.00001f;
            float min_param_a = 0.0f + epsilon;
            float max_param_a = 1.0f - epsilon;
            a = Mathf.Min(max_param_a, Mathf.Max(min_param_a, a));

            float y = 0;
            if (x <= 0.5)
            {
                y = (Mathf.Pow(2.0f * x, 1f - a)) / 2.0f;
            }
            else
            {
                y = 1.0f - (Mathf.Pow(2.0f * (1.0f - x), 1 - a)) / 2.0f;
            }
            return y;
        }

        float doubleExponentialSigmoid(float x, float a)
        {
            float epsilon = 0.00001f;
            float min_param_a = 0 + epsilon;
            float max_param_a = 1 - epsilon;
            a = Mathf.Min(max_param_a, Mathf.Max(min_param_a, a));
            a = 1 - a; // for sensible results

            float y = 0;
            if (x <= 0.5)
            {
                y = (Mathf.Pow( 2 * x, 1 / a ) ) / 2;
            }
            else
            {
                y = 1 - (Mathf.Pow(2 * (1 - x ), 1 / a ) ) / 2;
            }
            return y;
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
            int[] indices = new int[ capIndices.Length + sideIndices.Length ];
            inc = 0;
            for( int i=0; i < capIndices.Length; i++)
            {
                indices[inc++] = capIndices[i] + verticesCount + offset;
            }
            for (int i = 0; i < sideIndices.Length; i++)
            {
                indices[inc++] = sideIndices[i] + offset;
            }
            return indices;
        }

        public void Update(bool active)
        {
            return;
            geom.SetActive(active);
            if (active)
            {
                float[] p = tile.map.latLonToPixels(lat, lng);
                geom.transform.position = new Vector3(p[0], 2, -p[1]);

                if (p[0] < -tile.map.width / 2 || p[0] > tile.map.width / 2) active = false;
                if (-p[1] < -tile.map.height / 2 || -p[1] > tile.map.height / 2) active = false;

            }
            Renderer renderer = geom.GetComponent<Renderer>();
            renderer.material.color = active == true ? new Color(1, 1, 0) : new Color(0, .3f, .6f);


        }

    }
}