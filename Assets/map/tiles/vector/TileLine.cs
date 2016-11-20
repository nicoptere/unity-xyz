using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace XYZMap
{
    class TileLine
    {
        //LineString, MultiLineString
        MapTile tile;
        JSONObject data;
        GameObject parent;
        GameObject gameObject;
        Vector2 center;
        static private List<int> built = new List<int>();

        float lat, lng;
        public TileLine(MapTile tile, JSONObject data, GameObject parent, Color color, float height = 1, bool checkId = false)
        {

            this.tile = tile;
            this.data = data;
            this.parent = parent;

            //Debug.Log("> " + LineBuffer.points );
            //LineBuffer.addPoint(new Vector3(-100, 100, 0));
            return;
            lat = tile.lat;
            lng = tile.lng;
            float[] tileCenter = tile.map.latLonToPixels(lat, lng);

            for (int i = 0; i < data.Count; i++)
            {

                if( checkId && data[i]["properties"]["id"] != null)
                {
                    int id = (int)data[i]["properties"]["id"].n;
                    if (checkId && built.IndexOf(id) != -1) continue;
                    built.Add(id);
                }

                JSONObject geometry = data[i]["geometry"];

                if ( geometry["type"].str == "LineString" )
                {
                    processLine(tileCenter, geometry["coordinates"]);
                }
                /*
                if ( geometry["type"].str == "MultiLineString")
                {
                    for (int j = 0; j < geometry["coordinates"].Count; j++)
                    {
                        JSONObject poly = geometry["coordinates"][j];
                        processLine(tileCenter, poly);
                    }
                }
                //*/
            }
        }

        public void processLine(float[] center, JSONObject polygon)
        {
            //Debug.Log("c " + polygon.Count );
            for (int k = 0; k < polygon.Count; k++)
            {
                JSONObject poly = polygon[k];
                /*
                int count = poly.Count;
                Debug.Log("  > " + count );
                for (int j = 0; j < count; j++)
                {
                Debug.Log("  > " + polygon[j][0].n + " " + polygon[j][1].n);
                //*/
                    float[] pos = tile.map.latLonToPixels(polygon[1].n, polygon[0].n);
                    Vector3 v = new Vector3(pos[0] - center[0] - tile.map.tileSize / 2, 5, -pos[1] + center[1] + tile.map.tileSize / 2);
                    LineBuffer.addPoint(v);
                //}
            }
            /*
            try{
                //Debug.Log("ok");// geometry["type"].str + " " + geometry["coordinates"].Count);
            }
            catch(Exception e)
            {
                //Debug.Log("no " + e.ToString() );// geometry["type"].str + " " + geometry["coordinates"].Count);

            }
            //*/
        }

        public void Update(bool active)
        {
            gameObject.SetActive(active);

            Vector3 pos = tile.map.parent.gameObject.transform.position;

            if (tile.map.parent.renderToTexture)
            {
                gameObject.hideFlags = HideFlags.HideInHierarchy;
                pos.y = tile.map.parent.orthographicCamera.transform.position.y;
            }
            if (active)
            {
                float[] p = tile.map.latLonToPixels(lat, lng);
                gameObject.transform.position = new Vector3(p[0], 0, -p[1]);
            }
        }
    }
}
