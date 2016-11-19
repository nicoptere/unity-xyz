using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapObject : MonoBehaviour {

    public string provider;
    public string[] domains = new string[] { "a", "b", "c" };

    public int width = 512;
    public int height = 512;

    public float latitude;
    public float longitude;
    public float zoom;

    public bool renderToTexture = false;
    public string layerName = "map";

    public bool vectorTiles = true;

    private Map map;
    private Camera cam;
    private RenderTexture RT;

    public GameObject tiles;
    public GameObject quad;
    
    void Awake()
    {
        Debug.Log(this);

        //tiles container
        tiles = new GameObject();
        tiles.transform.parent = gameObject.transform;
        tiles.name = "tiles";

        if (renderToTexture)
        {

            RT = new RenderTexture(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            RT.name = "camRtt";

            cam = gameObject.AddComponent<Camera>() as Camera;
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(width, height) / 2;
            cam.targetTexture = RT;
            cam.nearClipPlane = 1f;
            cam.farClipPlane = 1000;
            cam.depth = -1;

            cam.cullingMask = (1 << LayerMask.NameToLayer("map"));
            cam.transform.parent = tiles.transform;
            cam.transform.position = new Vector3(0, 100, 0);
            cam.transform.LookAt(new Vector3());

            quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "rttMesh";
            quad.transform.parent = gameObject.transform.parent;
            quad.transform.Rotate(new Vector3(90, 0, 0));
            quad.transform.localScale = new Vector3(width, height, 1);

            Renderer renderer = quad.GetComponent<Renderer>();
            renderer.material.mainTexture = RT;
            renderer.material.shader = Shader.Find("Unlit/Texture");
            
        }
        //map object
        map = new Map(this, provider, domains, width, height);
        
    }

    void Start() {
    }

    void Update()
    {

        map.setView( latitude, longitude, zoom );

        if( renderToTexture)
        {
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = cam.targetTexture;
            cam.Render();
            RenderTexture.active = currentRT;

        }

        /*
        
        provider = provider == null ? "http://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}" : provider;
        Width = width == 0 ? 512 : width;
        Height = height == 0 ? 256 : height;
        provider = "http://stamen-tiles-{s}.a.ssl.fastly.net/terrain/{z}/{x}/{y}.png";
        
        //domains = new string[] { "a", "b", "c" };//,"d"};

        //provider = provider == null ? "http://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}" : provider;
        //Width = width == 0 ? 512 : width;
        //Height = height == 0 ? 256 : height;
        //provider = "https://tile.mapzen.com/mapzen/vector/v1/all/16/19293/24641.json";

        map.provider = "http://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}";
        map.domains = new string[] { "a", "b", "c", "d" };
        
        map.zoom = 15;
        //map.renderTiles();
        float dx = 0;//athf.Sin(Time.time * 0.01f) * .05f;
        float dy = 0;//Mathf.Sin(Time.time * 0.01f) * .05f;

        //-22.90979, -43.17625 Rio
        //map.setView(40.70719977f, -74.01516826f, 15 );// + Mathf.Round( ( .5f + Mathf.Sin( Time.time ) * .5f ) * 15 ) );
        //map.setView(48.80f + dx, 2.32f + dy, map.zoom );// + Mathf.Round( ( .5f + Mathf.Sin( Time.time ) * .5f ) * 15 ) );
        
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        cam.Render();
        RenderTexture.active = currentRT;

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
        //*/

    }

    private void resize(int width, int height)
    {
        if (map != null) map.setSize(width, height);
        if (quad != null)
        {
            quad.transform.localScale = new Vector3(width, height, 1);
            RT.width = width;
            RT.height = height;
            cam.orthographicSize = Mathf.Max(width, height);
        }

    }

}
