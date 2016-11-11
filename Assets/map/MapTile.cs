using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net;

public class MapTile{

    public Map map;
    public string quadKey;

    public GameObject plane;
    public Texture2D texture;
    
    public bool loaded = false;
    
    public int tx = 0;
    public int ty = 0;
    public float zoom = 0;
    
    public float lat;
    public float lng;
    
    public string url;
    
    /**
     * @param map the map this tile is bound to
     * @param quadKey the QuadKey of this Tile
     * @constructor
     */
    public MapTile( Map map, string quadKey = null ){

        this.map = map;
        this.quadKey = quadKey;
        initFromQuadKey(this.quadKey);
        
    }
    /**
     * @param map the map this tile is bound to
     * @param quadKey the QuadKey of this Tile
     * @constructor
     */
    public MapTile( Map map, int x, int y, float zoom)
    {
        this.map = map;
        quadKey = map.mercator.tileXYToQuadKey(x, y, zoom);
        initFromQuadKey(quadKey);
    }

    public void initFromQuadKey(string quadKey)
    {

        int[] tile = map.mercator.quadKeyToTileXY(quadKey);
        if (tile == null)return;
        
        tx = tile[0];
        ty = tile[1];
        zoom = tile[2];
        
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
    
    public Color[] GetPixels()
    {
        return texture.GetPixels(0, 0, map.tileSize, map.tileSize);
    }
    
}
