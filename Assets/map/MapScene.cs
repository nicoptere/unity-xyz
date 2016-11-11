using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class MapScene : MonoBehaviour
{
    public string provider;
    public int width        = 512;
    public int height       = 512;
    public float latitude   = 48.85f;
    public float longitude  = 2.32f;
    public float zoom = 1;

    void Start(){


        /*
        string key = "mapzen-foW3wh2";
        //https://mapzen.com/blog/terrain-tile-service/
        //string provider = "https://tile.mapzen.com/mapzen/terrain/v1/terrarium/{z}/{x}/{y}.png?api_key=" + key;
        //provider = "https://s3.amazonaws.com/elevation-tiles-prod/normal/{z}/{x}/{y}.png?api_key=mapzen-foW3wh2" + key;
        //*/

        Scene rttScene = SceneManager.CreateScene("MapScene");

        GameObject go = new GameObject();
        go.name = "pop";
        go.layer = 8;
        SceneManager.MoveGameObjectToScene(go, rttScene);

        MapObject app = go.AddComponent<MapObject>();
        app.provider = provider == null ? "http://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}" : provider;
        app.width   = width == 0 ? 512 : width;
        app.height  = height == 0 ? 256 : height;
        
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log("Active scene is '" + scene.name + "'.");



    }


}