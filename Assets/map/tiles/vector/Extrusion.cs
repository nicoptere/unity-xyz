using System.Collections.Generic;
using UnityEngine;

namespace XYZMap
{
    public class Extrusion
    {
        MapTile tile;
        JSONObject data;
        GameObject parent;
        GameObject geom;
        Vector2 center;
        float lat, lng;
        public Extrusion(MapTile tile, JSONObject data, GameObject parent, float height )
        {

            this.tile = tile;
            this.data = data;
            this.parent = parent;

            //Debug.Log( "-> " + data["coordinates"][0]);
            int count = data["coordinates"][0].Count - 1;
            List<Vector2> vertices2D = new List<Vector2>();
            center = new Vector2();
            for (int i = 0; i < count; i++)
            {
                center.x += data["coordinates"][0][i][0].n;
                center.y += data["coordinates"][0][i][1].n;
            }
            lng = center.x / count;
            lat = center.y / count;

            for (int i = 0; i < count; i++)
            {
                float[] pos = tile.map.latLonToPixels( data["coordinates"][0][i][1].n, data["coordinates"][0][i][0].n );
                Vector2 v = new Vector2(pos[0], -pos[1]);
                vertices2D.Add(v);
            }

            float thickness = 0;
            float h = height * 1 / tile.map.resolution( tile.map.zoom );// 250 * Random.value;
            Vector3[] vertices = new Vector3[ count * 2 ];
            for (int i = 0; i < count * 2; i++)
            {
                /*
                vertices[i] = new Vector3(  vertices2D[ i % count ].x - tile.map.tileSize / 2, 
                                            ( i < count ) ? 0 : h, 
                                            vertices2D[ i % count ].y + tile.map.tileSize / 2  );

                float[] pos = tile.map.latLonToPixels(data["coordinates"][0][i][1].n, data["coordinates"][0][i][0].n);
                float x = pos[0] - tile.map.tileSize / 2;
                float y = -pos[1] + tile.map.tileSize / 2;
                                            */

                float x = vertices2D[i % count].x - tile.map.tileSize / 2; 
                float y = vertices2D[i % count].y + tile.map.tileSize / 2;
                /*
                float d = Mathf.Max(1 - Mathf.Sqrt(-x * -x + -y * -y) / ( tile.map.width / 2 ), 0);
                thickness = Mathf.Max(thickness, d);
                //*/
                Vector3 v = new Vector3(x, (i < count) ? 0 : h, y);
                vertices[i] = v;


            }


            // Use the triangulator to get indices for creating triangles
            Triangulator tr = new Triangulator(vertices2D);
            int[] inds = tr.Triangulate();
            int[] indices = processIndices(inds, count);

            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = vertices;
            msh.triangles = indices;
            msh.RecalculateNormals();
            msh.RecalculateBounds();
            
            geom = new GameObject();
            geom.transform.parent = parent.transform;
            geom.AddComponent(typeof(MeshRenderer));
            geom.hideFlags = HideFlags.HideInHierarchy;

            MeshFilter filter = geom.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = msh;
            Renderer renderer = geom.GetComponent<Renderer>();
            renderer.enabled = false;
            /*
            renderer.material.color = new Color(0.1f, .1f, 0.1f);
            renderer.material.SetFloat("_Metallic", .5f);
            renderer.material.SetFloat("_Glossiness", .8f);
            //*/

            Color c = new Color(1,.65f,0);
            Color c1 = Color.yellow;
            LineRenderer lineRenderer = geom.AddComponent<LineRenderer>();
            lineRenderer.material = new Material( Shader.Find("Particles/Additive"));//Shader.Find("Unlit/Color"));// 
            //lineRenderer.material.color = c;
            
            lineRenderer.SetColors(c1, c);
            lineRenderer.SetWidth( 1, 1 );
            lineRenderer.SetVertexCount(vertices.Length);
            lineRenderer.SetPositions(vertices);
        }

        private int[] processIndices(int[] capIndices, int vertices)
        {
            //tesselation sides: extrusion
            var sideIndices = new int[vertices * 3 * 2];
            var sides = vertices;
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
                indices[inc++] = capIndices[i] + vertices;
            }
            for (int i = 0; i < sideIndices.Length; i++)
            {
                indices[inc++] = sideIndices[i];
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