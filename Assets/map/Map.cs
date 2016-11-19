using UnityEngine;
using System.Collections.Generic;
using Assets.map;
using Assets.map.tiles;

public class Map : MonoBehaviour{

    public MapObject parent;

    public Mercator mercator;
    public string provider;
    public string[] domains;

    public int tileSize, width, height;
    public float latitude, longitude, zoom;

    List<MapTile> tiles;
    List<string> keys;
    private TileLoader tileLoader;

    public Map(MapObject parent, string provider = null, string[] domains = null, int width = 512, int height = 512 )
    {
        this.parent = parent;
        tileSize = 256;

        mercator = new Mercator(256);
        this.provider = provider != null ? provider : "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
        //this.provider = provider != null ? provider : "http://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}";
        this.domains = domains != null ? domains : new string[] { "a", "b", "c" };

        this.width = width;
        this.height = height;
        
        //map center
        latitude = 0;
        longitude = 0;
        //map zoom level
        zoom = 0;

        //loading
        tileLoader = new GameObject().AddComponent<TileLoader>();
        tileLoader.init( this, parent );
        tiles = new List<MapTile>();
        keys = new List<string>();
        setSize(width, height);


    }
    
    /**
     * sets the view rect size
     * @param w
     * @param h
     * @param apply boolean to apply the transform only
     */
    public void setSize( int w=256, int h=256 )
    {
        width = w;
        height = h;
    }

    public void hideTiles()
    {
        foreach (MapTile tile in tiles)
        {
            tile.Update(false);
        }
    }

    public void showTiles()
    {
        hideTiles();
        List<MapTile> visible = getVisibleTiles();
        foreach (MapTile tile in visible)
        {
            if (tile.loaded && tile.zoom == zoom)
            {
                tile.Update(true);
            }
        }
    }

    /**
     * sets the map view
     * @param lat
     * @param lng
     * @param zoom
     */
    public void setView(float lat, float lng, float zoom )
    {
        latitude = lat;
        longitude = lng;
        this.zoom = Mathf.Max( 1, Mathf.Min( 21, zoom ) );
        getVisibleTiles();
        showTiles();
    }

    private List<MapTile> getVisibleTiles()
    {
        float[] bounds = viewRectToLatLng(latitude, longitude, zoom);
        int[] tl = mercator.latLonToTile(-bounds[0], bounds[1], zoom);
        int[] br = mercator.latLonToTile(-bounds[2], bounds[3], zoom);
        List<MapTile> tmp = new List<MapTile>();
        for (int i = tl[0]; i <= br[0]; i++)
        {
            for (int j = tl[1]; j <= br[1]; j++)
            {

                string key = mercator.tileXYToQuadKey(i, j, zoom);
                
                bool exist = false;
                foreach (MapTile tile in tiles){
                    if (key == tile.quadKey){
                        tmp.Add(tile);
                        exist = true;
                    }
                }

                if (!exist){

                    MapTile tile = new TileImage(this, key);

                    tileLoader.addTile(tile);
                    tiles.Add(tile);
                    keys.Add(key);
                }
            }
        }
        return tmp;
    }

    /**
     * adds a loaded tile to the pool
     * @param tile
     */
    public void onTextureloaded( MapTile tile )
    {
        showTiles();
    }
    public float resolution( float zoom )
    {
        return mercator.resolution(zoom);
    }
    


    public float[] viewRectToLatLng(float lat, float lng, float zoom)
    {
        float[] c = mercator.latLngToPixels(-lat, lng, zoom);
        float[] tl = mercator.pixelsToLatLng(c[0] - width / 2, c[1] - height / 2, zoom);
        float[] br = mercator.pixelsToLatLng(c[0] + width / 2, c[1] + height / 2, zoom);
        return new float[] { -tl[0], tl[1], -br[0], br[1] };
    }
    
    public float[] pixelsToLatLon(float x, float y)
    {
        float[] c = mercator.latLngToPixels(-latitude, longitude, zoom);
        float[] pos = mercator.pixelsToLatLng(c[0] - width / 2 + x, c[1] - height / 2 + y, zoom);
        pos[0] *= -1;
        return pos;
    }

    public float[] latLonToPixels(float lat, float lon)
    {
        float[] c = mercator.latLngToPixels(-latitude, longitude, zoom);
        float[] p = mercator.latLngToPixels(-lat, lon, zoom);
        return new float[] {
            Mathf.Floor( ( p[0] - c[0]) ) + tileSize / 2,
            Mathf.Floor( ( p[1] - c[1]) ) + tileSize / 2
        };
    }
    

}
