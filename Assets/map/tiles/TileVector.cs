using System.Collections.Generic;
using UnityEngine;

namespace Assets.map.tiles
{
    class TileVector : MapTile
    {

        private List<POI> pois = new List<POI>();
        private List<TileExtrusion> blocks = new List<TileExtrusion>();
        private List<FlatTile> flatTiles = new List<FlatTile>();
        private List<Extrusion> buildings = new List<Extrusion>();
        private List<Lines> lines = new List<Lines>();

        public TileVector( Map map, string quadKey)
        {
            this.map = map;
            this.quadKey = quadKey;
        }

        public override void Update( bool active )
        {

            foreach (POI poi in pois)
            {
                poi.Update(active);
            }
            foreach (Extrusion building in buildings)
            {
                building.Update(active);
            }
            foreach (FlatTile flat in flatTiles)
            {
                flat.Update(active);
            }
        }

        public override void onDataLoaded(WWW www)
        {

            JSONObject obj = new JSONObject(www.text);

            /*
            Debug.Log(obj["pois"]);
            Debug.Log(obj["pois"]["features"]);
            Debug.Log(obj["pois"]["features"][0]["geometry"]);
            //*/

            //Debug.Log( lat +" "+ lng);
            JSONObject POIData = obj["pois"]["features"];
            for (int i = 0; i < POIData.Count; i++)
            {
                //pois.Add( new POI( this, POIData[i], parent ) );
            }

            //JSONObject BuildingData = obj["building"]["features"];
            //Debug.Log(BuildingData);

            JSONObject BuildingData = obj["buildings"]["features"];
            for (int i = 0; i < BuildingData.Count; i++)
            {
                if (BuildingData[i]["geometry"]["type"].str == "Polygon")
                {
                    float h = 3;
                    if (BuildingData[i]["properties"]["height"] != null)
                    {
                        h = BuildingData[i]["properties"]["height"].n;
                        //Debug.Log("height" + h);
                    }
                    if (BuildingData[i]["properties"]["min_height"] != null)
                    {
                        h = BuildingData[i]["properties"]["min_height"].n;
                        //Debug.Log("min_height" + h);
                    }
                    //buildings.Add(new Extrusion(this, BuildingData[i]["geometry"], parent, h ));
                    //lines.Add(new Lines(this, BuildingData[i]["geometry"], parent));
                }
            }



            JSONObject BlocksData = obj["buildings"]["features"];
            blocks.Add(new TileExtrusion(this, BlocksData, map.parent.tiles, new Color(.8f, .8f, .8f)));
            
            //flatTiles.Add(new FlatTile(this, BlocksData, parent, new Color( .8f,.8f,.8f ), 10, false ));

            JSONObject EarthData = obj["earth"]["features"];
            flatTiles.Add(new FlatTile(this, EarthData, map.parent.tiles, new Color(.0f, .8f, .1f), 0, false));

            JSONObject WaterData = obj["water"]["features"];
            flatTiles.Add(new FlatTile(this, WaterData, map.parent.tiles, new Color(.1f, .6f, .95f), -1, false));
            

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
}
