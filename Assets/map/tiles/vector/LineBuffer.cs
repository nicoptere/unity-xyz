using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//http://gamedev.stackexchange.com/questions/96964/how-to-correctly-draw-a-line-in-unity
public class LineBuffer : MonoBehaviour {

    static public List<Vector3> points;


    // Choose the Unlit/Color shader in the Material Settings
    // You can change that color, to change the color of the connecting lines
    public Material lineMat;
    
    // Fill/drag these in from the editor
    private Color color;

    // Use this for initialization
    void Awake() {

        lineMat = new Material(Shader.Find("Unlit/Color"));//Shader.Find("Diffuse"));// 
        lineMat.color = new Color(1, 1, 1, 1);
        
        points = new List<Vector3>();

        /*
        points.Add(new Vector3(0, 0, 0));
        points.Add(new Vector3(50, 100, 0));
        points.Add(new Vector3(50, 200, 100));
        points.Add(new Vector3(20, 250, 0));
        points.Add(new Vector3(0, 0, 0));


        addPoint(new Vector3(0, 0, 0));
        addPoint(new Vector3(50, 100, 0));
        addPoint(new Vector3(50, 200, 100));
        addPoint(new Vector3(20, 250, 0));
        addPoint(new Vector3(0, 0, 0));
        //*/

        color = new Color(lineMat.color.r, lineMat.color.g, lineMat.color.b, lineMat.color.a);

    }
	static public void addPoint( Vector3 p)
    {
        Debug.Log("add point");
        points.Add(p);
    }

	// Update is called once per frame
	void Update () {
	
	}
    // Connect all of the `points` to the `mainPoint`
    void DrawConnectingLines()
    {
        if ( points.Count > 1 )
        {
            GL.Begin(GL.LINES);
            lineMat.SetPass(0);
            GL.Color(color);// lineMat.color.g, lineMat.color.b, lineMat.color.a));
            // Loop through each point to connect to the mainPoint
            for( var i = 1; i < points.Count; i++ )
            {
                Vector3 prev = points[i - 1];
                Vector3 cur = points[i];

                GL.Vertex3( prev.x, prev.y, prev.z);
                GL.Vertex3( cur.x, cur.y, cur.z);

            }
            GL.End();
        }
    }

    // To show the lines in the game window whne it is running
    void OnPostRender()
    {
        DrawConnectingLines();
    }

    // To show the lines in the editor
    void OnDrawGizmos()
    {
        DrawConnectingLines();
    }
}