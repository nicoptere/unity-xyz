using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class MapTile{

    public Map map;
    public string quadKey;

    public GameObject plane;
    public Texture2D texture;
    
    public bool loaded = false;
    
    public float lat;
    public float lng;
    public int tx = 0;
    public int ty = 0;
    public float zoom = 0;
    
    public string url;
    
    public MapTile( Map map, string quadKey = null ){

        this.map = map;
        this.quadKey = quadKey;
        initFromQuadKey(this.quadKey);
        
    }

    public MapTile( Map map, int x, int y, float zoom)
    {
        this.map = map;
        quadKey = map.mercator.tileXYToQuadKey(x, y, zoom);
        initFromQuadKey(quadKey);
    }

    public void initFromQuadKey(string quadKey)
    {

        int[] data = map.mercator.quadKeyToTileXY(quadKey);
        if (data == null)return;
        
        tx = data[0];
        ty = data[1];
        zoom = data[2];
        
        float[] lalo = map.mercator.tileLatLngBounds(tx, ty, zoom);
        lat = -lalo[0];
        lng = lalo[1];
       
        url = getMapUrl(tx, ty, zoom);

    }
    
    public string getMapUrl( int x, int y, float z)
    {
        return getMapUrl(map.provider, map.domains, x, y, z);
    }

    public string getMapUrl(string provider, string[] domains, int x, int y, float z )
    {
        string url = provider;
        string pattern = "{x}";
        string replacement = x.ToString();
        Regex rgx = new Regex(pattern);
        url = rgx.Replace(url, replacement);

        pattern = "{y}";
        replacement = y.ToString();
        rgx = new Regex(pattern);
        url = rgx.Replace(url, replacement);

        pattern = "{z}";
        replacement = z.ToString();
        rgx = new Regex(pattern);
        url = rgx.Replace(url, replacement);

        if ( domains != null && domains.Length > 0)
        {
            pattern = "{s}";
            replacement = map.domains[(int)(Random.value * map.domains.Length)];
            rgx = new Regex(pattern);
            url = rgx.Replace(url, replacement);
        }
        return url;
    }

    public void onTextureLoaded(WWW www)
    {
        texture = www.texture;
        Renderer renderer = plane.GetComponent<Renderer>();
        renderer.material.shader = Shader.Find("Unlit/Texture");
        renderer.material.mainTexture = texture;
        loaded = true;
    }

    public void onJSONLoaded(WWW www, GameObject parent )
    {
        JSONObject obj = new JSONObject( www.text );

        /*
        Debug.Log(obj["pois"]);
        Debug.Log(obj["pois"]["features"]);
        Debug.Log(obj["pois"]["features"][0]["geometry"]);
        //*/

        Debug.Log( lat +" "+ lng);
        JSONObject pois = obj["pois"]["features"];
        for ( int i = 0; i < pois.Count; i++)
        {
            //Debug.Log(obj["pois"]["features"][i]["geometry"]["coordinates"][0].n);
                
            float plat = pois[i]["geometry"]["coordinates"][1].n;
            float plng = pois[i]["geometry"]["coordinates"][0].n;

            var p = map.latLonToPixels( plat, plng );
            Debug.Log(plat+"  "+ plng + " " + p[0]+"  "+ p[1]);

            float upScale = 10;
            Vector3 scale = new Vector3( 1/parent.transform.localScale.x * upScale, upScale, 1 / parent.transform.localScale.y * upScale);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            sphere.transform.parent = parent.transform;// map.parent.transform.parent;
            sphere.transform.position = new Vector3( p[0], 0, -p[1]);
            sphere.transform.localScale = scale;
            //sphere.layer = 8;

            Renderer renderer = sphere.GetComponent<Renderer>();
            renderer.material.color = new Color(1, 0, 0);

        }

        /*
        for (int i = 0; i < obj.list.Count; i++)
        {
            string key = (string)obj.keys[i];
            if (key == "pois")
            {
                JSONObject j = (JSONObject)obj.list[i];
                accessData(j);
            }
        }
        //*/

        /*
        string provider = "https://s3.amazonaws.com/elevation-tiles-prod/normal/{z}/{x}/{y}.png?api_key=mapzen-foW3wh2";
        string url = tile.getMapUrl( provider, null, tile.tx, tile.ty, map.zoom);
        www = new WWW(url);
        yield return www;
        renderer.material.SetTexture("_BumpMap", www.texture );
        //*/
    }

    //access data (and print it)
    void accessData(JSONObject obj)
    {
        switch (obj.type)
        {
            case JSONObject.Type.OBJECT:
                for (int i = 0; i < obj.list.Count; i++)
                {
                    string key = (string)obj.keys[i];
                    JSONObject j = (JSONObject)obj.list[i];
                    Debug.Log(key);
                    accessData(j);
                    
                }
                break;
            case JSONObject.Type.ARRAY:
                foreach (JSONObject j in obj.list)
                {
                    accessData(j);
                }
                break;
            case JSONObject.Type.STRING:
                Debug.Log(obj.str);
                break;
            case JSONObject.Type.NUMBER:
                Debug.Log(obj.n);
                break;
            case JSONObject.Type.BOOL:
                Debug.Log(obj.b);
                break;
            case JSONObject.Type.NULL:
                Debug.Log("NULL");
                break;

        }
    }
}
