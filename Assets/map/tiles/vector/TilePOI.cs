//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XYZMap
{
    public class TilePOI
    {
        MapTile tile;
        GameObject geom;
        float lat, lng;

        private Color inside = new Color(1, 1, .75f);
        private Color outside = new Color(0,0,0);

        public TilePOI( MapTile tile, JSONObject data ){

            this.tile = tile;
            lat = data["geometry"]["coordinates"][1].n;
            lng = data["geometry"]["coordinates"][0].n;
            
            float upScale = 2 + Random.value * 5;
            Vector3 scale = new Vector3(1 / tile.gameObject.transform.localScale.x * upScale, upScale, 1 / tile.gameObject.transform.localScale.y * upScale);
            geom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            geom.transform.parent = tile.gameObject.transform;
            geom.transform.localScale = scale;
            geom.hideFlags = HideFlags.HideInHierarchy;

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
                p[0] -= tile.map.tileSize / 2;
                p[1] -= tile.map.tileSize / 2;
                geom.transform.position = new Vector3(p[0], 2, -p[1]);

                if (p[0] < -tile.map.width/2 || p[0] > tile.map.width/2) active = false;
                if (-p[1] < -tile.map.height/2 || -p[1] > tile.map.height/2) active = false;

            }
            Renderer renderer = geom.GetComponent<Renderer>();
            renderer.material.color = active == true ? inside : outside;
            renderer.material.SetColor( "_EmissionColor", active == true ? inside : outside);

        }

    }
}
