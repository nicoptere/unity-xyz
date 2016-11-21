using UnityEngine;
using System.Collections;

public class testWireframe : MonoBehaviour {

	// Use this for initialization
	void Start () {

        GameObject plane = createMesh();
        plane.transform.parent = gameObject.transform;

        Renderer rend = plane.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Unlit/Wireframe"));
        rend.material = mat;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    GameObject createMesh()
    {
        GameObject go = new GameObject();
        
        Vector3 p0 = new Vector3(-1.25f, 0, -0.0f);
        Vector3 p1 = new Vector3(0.25f, 0, -0.0f);
        Vector3 p2 = new Vector3(0, 0, 5.5f);

        Vector3[] newVertices = new Vector3[] { p0, p1, p2 };
        
        Vector3 cen = new Vector3(0,0,0);
        for( int i = 0; i< newVertices.Length; i++)
        {
            //newVertices[i].Normalize();
            cen += newVertices[i];
        }
        cen /= newVertices.Length;
        //cen.Normalize();

        Debug.Log(Vector3.Distance(cen, p0));
        Debug.Log(Vector3.Distance(cen, p1));
        Debug.Log(Vector3.Distance(cen, p2));

        float a = Vector3.Distance(cen, p0);
        float b = Vector3.Distance(cen, p1);
        float c = Vector3.Distance(cen, p2);
        float v = Mathf.Min( a, Mathf.Min( b,c ) );

        Color[] newColors = new Color[] {

                /*
                new Color( 1,b,c, Vector3.Distance(cen, p0) ),
                new Color( a,1,c, Vector3.Distance(cen, p1) ),
                new Color( a,b,1, Vector3.Distance(cen, p2) )

                new Color( a, b-a, c-a, Vector3.Distance(cen, p0) ),
                new Color( a-b, b, c-b, Vector3.Distance(cen, p1) ),
                new Color( a-c, b-c, c, Vector3.Distance(cen, p2) )

                new Color( a, 0, 0, Vector3.Distance(cen, p0) ),
                new Color( 0, b, 0, Vector3.Distance(cen, p1) ),
                new Color( 0, 0, c, Vector3.Distance(cen, p2) )

                //*/
                
                new Color( 1, 0, 0, Vector3.Distance(cen, p0) ),
                new Color( 0, 1, 0, Vector3.Distance(cen, p1) ),
                new Color( 0, 0, 1, Vector3.Distance(cen, p2) )


            };
        /*
        Color[] newColors = new Color[] {
                new Color( 1,0,0),
                new Color( 0,1,0),
                new Color( 0,0,1)
            };
        //*/

        Vector2[] newUV = new Vector2[] {
                new Vector2( 0,0 ),
                new Vector2( 1,0 ),
                new Vector2( 1,1 )
            };

        int[] newTriangles = new int[] {
                0, 1, 2,   //front
                0, 2, 1    //back
            };


        Mesh mesh = new Mesh();
        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.colors = newColors;
        mesh.triangles = newTriangles;
        go.AddComponent(typeof(MeshFilter));
        go.GetComponent<MeshFilter>().mesh = mesh;
        go.AddComponent(typeof(MeshRenderer));
        return go;

    }
}
