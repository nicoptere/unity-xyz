//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.map.tiles
{
    public class POI
    {
        MapTile tile;
        JSONObject data;
        GameObject parent;
        GameObject geom;

        float lat, lng;

        public POI( MapTile tile, JSONObject data ){

            this.tile = tile;
            this.data = data;
            this.parent = tile.gameObject;
            //Debug.Log(obj["pois"]["features"][i]["geometry"]["coordinates"][0].n);

            lat = data["geometry"]["coordinates"][1].n;
            lng = data["geometry"]["coordinates"][0].n;
            
            float upScale = 2 + Random.value * 5;
            Vector3 scale = new Vector3(1 / parent.transform.localScale.x * upScale, upScale, 1 / parent.transform.localScale.y * upScale);
            geom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            geom.transform.parent = parent.transform;
            geom.transform.localScale = scale;

            Renderer renderer = geom.GetComponent<Renderer>();
            renderer.material.color = new Color( 0, 0, 0 );
            renderer.material.SetFloat("_Metallic",  .5f);
            renderer.material.SetFloat("_Glossiness", .8f );
            renderer.material.SetColor("_EmissionColor", new Color(1, 1, .75f));
            Update(true);

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
            renderer.material.color = active == true ? new Color(1, 1, .75f) : new Color(0, 0, 0);
            renderer.material.SetColor( "_EmissionColor", active == true ? new Color(1, 1, .75f) : new Color(0, 0, 0) );
            //*/

        }

    }
}
