//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.map.extra
{
    public class Lines
    {
        MapTile tile;
        JSONObject data;
        GameObject parent;
        GameObject geom;
        Vector2 center;
        float lat, lng;
        public Lines(MapTile tile, JSONObject data, GameObject parent)
        {

            this.tile = tile;
            this.data = data;
            this.parent = parent;
            
            int count = data["coordinates"][0].Count;

            float h = 250 * Random.value;
            Vector3[] vertices = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float[] pos = tile.map.latLonToPixels( data["coordinates"][0][i][1].n, data["coordinates"][0][i][0].n );
                float x = pos[0] - tile.map.tileSize / 2;
                float y = -pos[1] + tile.map.tileSize / 2;

                float d = Mathf.Max( 1 - Mathf.Sqrt(-x * -x + -y * -y) / ( tile.map.width / 2 ), 0 );
                Vector3 v = new Vector3( x, h * d, y );
                vertices[ i ] = v;
            }

            Color c =new Color( 0,.2f,.8f );
            Color c1 = Color.yellow;
            geom = new GameObject();
            geom.transform.parent = parent.transform;

            LineRenderer lineRenderer = geom.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(  Shader.Find("Particles/Additive"));//Shader.Find( "Unlit/Color" ) );//
            lineRenderer.material.color = c;
            //lineRenderer.SetColors(c1, c);
            lineRenderer.SetWidth(1.0F, 1.0F);
            lineRenderer.SetVertexCount(count);
            lineRenderer.SetPositions(vertices);
            
        }


        public void Update(bool active)
        {
            return;
            geom.SetActive(active);
            if (active)
            {
                float[] p = tile.map.latLonToPixels(lat, lng);
                geom.transform.position = new Vector3(p[0], 2, -p[1]);

                if (p[0] < -tile.map.width / 2 || p[0] > tile.map.width / 2) active = false;
                if (-p[1] < -tile.map.height / 2 || -p[1] > tile.map.height / 2) active = false;

            }
            Renderer renderer = geom.GetComponent<Renderer>();
            renderer.material.color = active == true ? new Color(1, 1, 0) : new Color(0, .3f, .6f);


        }

    }
}