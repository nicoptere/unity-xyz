using System.Collections.Generic;
using UnityEngine;

namespace XYZMap
{

    class TileVector : MapTile
    {

        private List<TilePOI> pois = new List<TilePOI>();
        private List<TileExtrusion> extrusions = new List<TileExtrusion>();
        private List<TileFlat> flats = new List<TileFlat>();
        private List<TileLine> lines = new List<TileLine>();

        public TileVector( Map map, string quadKey)
        {
            this.map = map;
            this.quadKey = quadKey;
            initFromQuadKey(quadKey);
        }

        public override void Update( bool active )
        {
            if (!loaded) return;

            foreach (TilePOI poi in pois)
            {
                poi.Update(active);
            }
            foreach (TileExtrusion extrusion in extrusions)
            {
                extrusion.Update(active);
            }
            foreach (TileFlat flat in flats)
            {
                flat.Update(active);
            }
        }

        public override void onDataLoaded(WWW www)
        {

            if (gameObject == null)
            {
                gameObject = new GameObject();
                gameObject.transform.parent = map.parent.tiles.transform;
            }

            JSONObject obj = new JSONObject(www.text);
            /*
            JSONObject POIData = obj["pois"]["features"];
            for (int i = 0; i < POIData.Count; i++)
            {
                Debug.Log(POIData[i]);
                pois.Add( new TilePOI( this, POIData[i] ) );
            }
            //*/

            Color color = new Color(Random.value + .5f, Random.value + .5f, Random.value + .5f);
            
            //buildings
            extrusions.Add(new TileExtrusion(this, obj["buildings"]["features"], map.parent.tiles, color ));
            
            //landuse
            flats.Add(new TileFlat(this, obj["landuse"]["features"], map.parent.tiles, color, -2 ));
            
            //earth 
            flats.Add(new TileFlat(this, obj["earth"]["features"], map.parent.tiles, color, -4 ));

            //water
            color = new Color(0, 0.3f, 0.8f);
            flats.Add(new TileFlat(this, obj["water"]["features"], map.parent.tiles, color,-3 ));


            //roads types:
            //highway, major_road, minor_road, path, aeroway, rail, ferry, piste, aerialway, racetrack, portage_way

            float scale = 1 / map.resolution(map.zoom);

            color = new Color( 1,0,0 );// Random.value + .5f, Random.value + .5f, Random.value + .5f);
            List<string> roads = new List<string>(new string[] { "highway", "major_road", "minor_road" });
            lines.Add(new TileLine(this, obj["roads"]["features"], map.parent.tiles, color, roads, 2 * scale ));
            /*
            color = new Color( 0,1,0 );
            List<string> rails = new List<string>(new string[] { "rail" });
            lines.Add(new TileLine(this, obj["roads"]["features"], map.parent.tiles, color, rails, 4 * scale));
            
            color = new Color( 0,0,1 );
            List<string> boats = new List<string>(new string[] { "ferry" });
            lines.Add(new TileLine(this, obj["roads"]["features"], map.parent.tiles, color, boats, 10 * scale));
            //*/
        }

    }
}
