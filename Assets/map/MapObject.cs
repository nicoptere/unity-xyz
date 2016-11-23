using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapObject : MonoBehaviour {

    public string provider = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
    public string[] domains = new string[] { "a", "b", "c" };

    public int width = 512;
    public int height = 512;

    public float latitude;
    public float longitude;
    public float zoom;

    public GameObject tiles;
    public bool renderToTexture = false;
    public GameObject rttMesh;
    public string layerName = "map";

    public bool vectorTiles = true;
    public string vectorTileUrl = "https://tile.mapzen.com/mapzen/vector/v1/all/{z}/{x}/{y}.json?api_key=mapzen-foW3wh2";
    public bool flatNormals = true;

    public bool tiles3d = false;
    public string diffuseProviderUrl = "http://a.tile.openstreetmap.org/{z}/{x}/{y}.png";//"http://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}";
    public string elevationProviderUrl = "https://tile.mapzen.com/mapzen/terrain/v1/terrarium/{z}/{x}/{y}.png?api_key=mapzen-foW3wh2";
    public string normalProviderUrl = "https://tile.mapzen.com/mapzen/terrain/v1/normal/{z}/{x}/{y}.png?api_key=mapzen-foW3wh2";

    private Map map;
    public Camera orthographicCamera;
    private RenderTexture RT;
    
    void Awake()
    {
        //tiles container
        tiles = new GameObject();
        tiles.transform.parent = gameObject.transform;
        tiles.name = "tiles";

        if (renderToTexture)
        {

            RT = new RenderTexture(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            RT.name = "camRtt";

            orthographicCamera = gameObject.AddComponent<Camera>() as Camera;
            orthographicCamera.orthographic = true;
            orthographicCamera.orthographicSize = Mathf.Max(width, height) / 2;
            orthographicCamera.targetTexture = RT;
            orthographicCamera.nearClipPlane = 0;
            orthographicCamera.farClipPlane = 2;
            orthographicCamera.depth = -1;

            orthographicCamera.cullingMask = (1 << LayerMask.NameToLayer("map"));
            orthographicCamera.transform.parent = tiles.transform;
            orthographicCamera.transform.position = new Vector3(0, 1, 0);
            orthographicCamera.transform.LookAt(new Vector3());
            
            rttMesh = GameObject.CreatePrimitive(PrimitiveType.Quad);
            rttMesh.name = "rttMesh";
            rttMesh.transform.parent = gameObject.transform.parent;
            rttMesh.transform.Rotate(new Vector3(90, 0, 0));
            rttMesh.transform.localScale = new Vector3(width, height, 1);

            Renderer renderer = rttMesh.GetComponent<Renderer>();
            renderer.material.mainTexture = RT;
            renderer.material.shader = Shader.Find("Unlit/Texture");

            gameObject.transform.position = new Vector3(0, -50000, 0);
            gameObject.hideFlags = HideFlags.HideInHierarchy;

            vectorTiles = false;

        }

        if( vectorTiles)
        {
            if (vectorTileUrl == "")
            {
                vectorTiles = false;
            }else
            {
                provider = vectorTileUrl;
            }
        }


        if (tiles3d)
        {
            provider = diffuseProviderUrl;
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
            RenderTexture.active = orthographicCamera.targetTexture;
            orthographicCamera.Render();
            RenderTexture.active = currentRT;
        }

        /*
        float[] delta = map.mercator.pixelsToLatLng(0, 250, zoom);
        delta[0] -= latitude;
        delta[1] -= longitude;

        if (Input.GetKey("up"))
        {
            latitude += delta[0];
        }
        if (Input.GetKey("down"))
        {
            latitude -= delta[0];
        }
        if (Input.GetKey("right"))
        {
            longitude += delta[1];
        }
        if (Input.GetKey("left"))
        {
            longitude -= delta[1];
        }
        //*/


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
        //map.setView(48.866667, 2.333333, map.zoom );// + Mathf.Round( ( .5f + Mathf.Sin( Time.time ) * .5f ) * 15 ) );
        
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
        if (rttMesh != null)
        {
            rttMesh.transform.localScale = new Vector3(width, height, 1);
            RT.width = width;
            RT.height = height;
            orthographicCamera.orthographicSize = Mathf.Max(width, height);
        }
    }
}
