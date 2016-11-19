
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Assets.map.extra
{
    public class FlatTile
    {
        MapTile tile;
        JSONObject data;
        GameObject parent;
        GameObject geom;
        Vector2 center;
        static private List<int> built = new List<int>();

        float lat, lng;
        public FlatTile(MapTile tile, JSONObject data, GameObject parent, Color color, float height = 1, bool checkId = true)
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
                if (built.IndexOf(id) != -1) continue;
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
                            break;
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
            //*/
            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = tmpVertices.ToArray();
            msh.triangles = indices;
            
            msh.RecalculateNormals();
            msh.RecalculateBounds();

            geom = new GameObject();
            geom.transform.parent = parent.transform;
            
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

        private int[] triangulate( List<Vector2> vertices)
        {
            // Create an instance of the tessellator. Can be reused.
            var tess = new LibTessDotNet.Tess();

            // Construct the contour from inputData.
            // A polygon can be composed of multiple contours which are all tessellated at the same time.
            int numPoints = vertices.Count;
            Debug.Log("numpoints " + numPoints);
            var contour = new LibTessDotNet.ContourVertex[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                // NOTE : Z is here for convenience if you want to keep a 3D vertex position throughout the tessellation process but only X and Y are important.
                contour[i].Position = new LibTessDotNet.Vec3 { X = vertices[i].x, Y = vertices[i].y, Z = 0 };
                //Debug.Log("contour i: " + contour[i].Position.X + " " + contour[i].Position.Y);

            }
            // Add the contour with a specific orientation, use "Original" if you want to keep the input orientation.
            tess.AddContour(contour, LibTessDotNet.ContourOrientation.CounterClockwise );

            // Tessellate!
            // The winding rule determines how the different contours are combined together.
            // See http://www.glprogramming.com/red/chapter11.html (section "Winding Numbers and Winding Rules") for more information.
            // If you want triangles as output, you need to use "Polygons" type as output and 3 vertices per polygon.
            //tess.Tessellate(LibTessDotNet.WindingRule.EvenOdd, LibTessDotNet.ElementType.Polygons, 3, VertexCombine);

            // Same call but the last callback is optional. Data will be null because no interpolated data would have been generated.
            tess.Tessellate(LibTessDotNet.WindingRule.EvenOdd, LibTessDotNet.ElementType.Polygons, 3); // Some vertices will have null Data in this case.
            
            int numtriangles = tess.ElementCount;
            Debug.Log("numTriangles " + numtriangles);
            for (int i = 0; i < numtriangles; i++)
            {
                //var v0 = tess.vertices[tess.elements[i * 3]].position;
                //var v1 = tess.vertices[tess.elements[i * 3 + 1]].position;
                //var v2 = tess.vertices[tess.elements[i * 3 + 2]].position;
                //Debug.Log(tess.Elements[i * 3] + " " + tess.Elements[i * 3 + 1] + " " + tess.Elements[i * 3 + 2]);

            }
            return tess.Elements;

        }

        public void Update(bool active)
        {
            geom.SetActive(active);
            if (active)
            {
                float[] p = tile.map.latLonToPixels(lat, lng);
                geom.transform.position = new Vector3(p[0], 0, -p[1]);

                //if (p[0] < -tile.map.width / 2 || p[0] > tile.map.width / 2) active = false;
                //if (-p[1] < -tile.map.height / 2 || -p[1] > tile.map.height / 2) active = false;

            }
            //Renderer renderer = geom.GetComponent<Renderer>();
            //renderer.material.color = active == true ? new Color(1, 1, 0) : new Color(0, .3f, .6f);


        }

    }
}