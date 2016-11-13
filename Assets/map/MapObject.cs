using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapObject : MonoBehaviour {

    public string provider;

    public int width = 512;
    public int height = 512;

    public float latitude;
    public float longitude;
    public float zoom;

    public MapTile titi;
    public string[] domains;

    GameObject tiles;
    private Map map;
    private RenderTexture RT;
    private Camera cam;
    private GameObject quad;
    
    private void resize(int width, int height)
    {
        if( map != null )map.setSize(width, height);
        if (quad != null)
        {
            quad.transform.localScale = new Vector3(width, height, 1);
            RT.width = width;
            RT.height = height;
            cam.orthographicSize = Mathf.Max( width, height );
        }

    }
    
    void Start() {
        
        RT = new RenderTexture(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
        RT.name = "camRtt";
        
        cam = gameObject.AddComponent<Camera>() as Camera;
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max( width, height ) / 2;
        cam.targetTexture = RT;
        cam.nearClipPlane = 1f;
        cam.farClipPlane = 1000;
        cam.depth = -1;

        cam.cullingMask = (1 << LayerMask.NameToLayer("map"));
        cam.transform.parent = gameObject.transform.parent;
        cam.transform.position = new Vector3(0, 0, -0);

        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "rttMesh";

        quad.transform.parent = gameObject.transform.parent;
        quad.transform.Rotate(new Vector3(90, 0, 0));
        //quad.transform.position = new Vector3(width * 2, 0, -1);
        quad.transform.localScale = new Vector3(width, height, 1);

        Renderer renderer = quad.GetComponent<Renderer>();
        renderer.material.mainTexture = RT;
        renderer.material.shader = Shader.Find("Unlit/Texture");
        renderer.enabled = false;        

        //
        /*
        provider = provider == null ? "http://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}" : provider;
        Width = width == 0 ? 512 : width;
        Height = height == 0 ? 256 : height;
        provider = "http://stamen-tiles-{s}.a.ssl.fastly.net/terrain/{z}/{x}/{y}.png";
        //*/
        domains = new string[] { "a", "b", "c" };//,"d"};

        //provider = provider == null ? "http://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}" : provider;
        //Width = width == 0 ? 512 : width;
        //Height = height == 0 ? 256 : height;
        //provider = "https://tile.mapzen.com/mapzen/vector/v1/all/16/19293/24641.json";
        tiles = new GameObject();
        tiles.name = "tiles";
        tiles.SetActive(false);

        map = new Map( this, provider, domains, width, height );
        map.zoom = zoom;

    }

    GameObject createMesh()
    {
        GameObject go = new GameObject();

        Vector3[] newVertices = new Vector3[] {
            new Vector3( -0.5f, -0.5f, 0 ),
            new Vector3(  0.5f, -0.5f, 0 ),
            new Vector3(  0.5f,  0.5f, 0 ),
            new Vector3( -0.5f,  0.5f, 0 ),
            };

        Vector2[] newUV = new Vector2[] {
            new Vector2( 0,0 ),
            new Vector2( 1,0 ),
            new Vector2( 1,1 ),
            new Vector2( 0,1 ),
        };

        int[] newTriangles = new int[] {
            0, 1, 3, 1, 2, 3,//back
            0, 3, 1, 2, 1, 3 //front
        };

        Mesh mesh = new Mesh();
        mesh.vertices = newVertices;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;
        go.AddComponent(typeof(MeshFilter));
        go.GetComponent<MeshFilter>().mesh = mesh;

        go.AddComponent(typeof(MeshRenderer));
        go.layer = 8;
        return go;

    }

    public void addTile(MapTile tile)
    {

        GameObject plane = createMesh();// GameObject.CreatePrimitive(PrimitiveType.Quad );//

        //plane.transform.parent = gameObject.transform;
        //plane.transform.position = new Vector3(tile.px, 0, tile.py);

        plane.transform.parent = tiles.transform;
        plane.transform.localScale = new Vector3( 256f, 256f, 1);
        tile.plane = plane;

        StartCoroutine( loadImage(tile) );

    }

    public IEnumerator loadImage( MapTile tile )
    {

        WWW www = new WWW(tile.url);
        yield return www;
        tile.onTextureLoaded(www);
        
        string provider = "https://tile.mapzen.com/mapzen/vector/v1/all/{z}/{x}/{y}.json?api_key=mapzen-foW3wh2";
        string url = tile.getMapUrl(provider, null, tile.tx, tile.ty, map.zoom);
        www = new WWW(url);
        yield return www;

        tile.onJSONLoaded(www, quad);
        
    }

    void Update()
    {
        
        /*
        map.provider = "http://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}";
        map.domains = new string[] { "a", "b", "c", "d" };
        */
        map.zoom = 15;
        //map.renderTiles();
        float dx = 0;//athf.Sin(Time.time * 0.01f) * .05f;
        float dy = 0;//Mathf.Sin(Time.time * 0.01f) * .05f;
        
        map.setView(40.70719977f, -74.01516826f, 15 );// + Mathf.Round( ( .5f + Mathf.Sin( Time.time ) * .5f ) * 15 ) );
        //map.setView(48.80f + dx, 2.32f + dy, map.zoom );// + Mathf.Round( ( .5f + Mathf.Sin( Time.time ) * .5f ) * 15 ) );

        /*
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        cam.Render();
        RenderTexture.active = currentRT;
        //*/

        if (Input.GetKey("up"))
        {
            map.provider = "http://stamen-tiles-{s}.a.ssl.fastly.net/terrain/{z}/{x}/{y}.png";
            map.domains = new string[] { "a", "b", "c", "d" };
            map.zoom = 8;
        }
        if (Input.GetKey("down"))
        {
            map.provider = "http://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}";
            map.domains = new string[] { "a", "b", "c", "d" };
            map.zoom = 12;
        }
        if (Input.GetKey("left"))
        {
            map.provider = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
            map.domains = new string[] { "a", "b", "c" };
            map.zoom = 16;
        }
        
    }

}
