using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.map.extra
{
    public class POI
    {
        MapTile tile;
        JSONObject data;
        GameObject parent;
        GameObject geom;

        float lat, lng;

        public POI( MapTile tile, JSONObject data, GameObject parent ){

            this.tile = tile;
            this.data = data;
            this.parent = parent;
            //Debug.Log(obj["pois"]["features"][i]["geometry"]["coordinates"][0].n);

            lat = data["geometry"]["coordinates"][1].n;
            lng = data["geometry"]["coordinates"][0].n;
            
            float upScale = 10;
            Vector3 scale = new Vector3(1 / parent.transform.localScale.x * upScale, upScale, 1 / parent.transform.localScale.y * upScale);
            geom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            geom.transform.parent = this.parent.transform;
            geom.transform.localScale = scale;

            Renderer renderer = geom.GetComponent<Renderer>();
            renderer.material.color = new Color( 0, 1, 0);

            //Update();

        }


        public void Update( bool active )
        {
                geom.SetActive( active );
            if (active)
            {
                var p = tile.map.latLonToPixels( lat, lng );
                geom.transform.position = new Vector3(p[0], 2, -p[1]);

                if (p[0] < -tile.map.width/2 || p[0] > tile.map.width/2) active = false;
                if (-p[1] < -tile.map.height/2 || -p[1] > tile.map.height/2) active = false;

            }
            Renderer renderer = geom.GetComponent<Renderer>();
            renderer.material.color = active == true ? new Color(0,1, 0) : new Color( 1,0, 0);


        }

    }
}
