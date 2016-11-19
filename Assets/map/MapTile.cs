using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Assets.map.tiles;

public class MapTile{

    public Map map;
    public string quadKey;

    public GameObject gameObject;
    public Texture2D texture;
    
    public bool loaded = false;
    
    public float lat;
    public float lng;
    public int tx = 0;
    public int ty = 0;
    public float zoom = 0;
    
    public string url;

    public MapTile() { }

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

    public string getMapUrl(int x, int y)
    {
        return getMapUrl(map.provider, map.domains, x, y, map.zoom);
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

    public virtual void Update(bool active)
    {
        /*
        gameObject.SetActive(active);
        if( active)
        {
            float[] p = map.latLonToPixels(lat, lng);
            gameObject.transform.position = new Vector3(p[0], -p[1], 0);
        }
        //*/
    }
    
    public virtual void onDataLoaded(WWW www)
    {
        /*
        texture = www.texture;
        Renderer renderer = gameObject.GetComponent<Renderer>();
        renderer.material.shader = Shader.Find("Unlit/Texture");
        renderer.material.mainTexture = texture;
        loaded = true;
        //*/
    }

}
