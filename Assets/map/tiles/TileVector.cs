using System.Collections.Generic;
using UnityEngine;

namespace Assets.map
{

    class TileVector : MapTile
    {

        private List<TilePOI> pois = new List<TilePOI>();
        private List<TileExtrusion> blocks = new List<TileExtrusion>();
        private List<TileFlat> flatTiles = new List<TileFlat>();
        private List<Extrusion> buildings = new List<Extrusion>();

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
            foreach (Extrusion building in buildings)
            {
                building.Update(active);
            }
            foreach (TileFlat flat in flatTiles)
            {
                flat.Update(active);
            }
        }

        public override void onDataLoaded(WWW www)
        {

            JSONObject obj = new JSONObject(www.text);

            /*
            JSONObject POIData = obj["pois"]["features"];
            for (int i = 0; i < POIData.Count; i++)
            {
                Debug.Log(POIData[i]);
                pois.Add( new TilePOI( this, POIData[i] ) );
            }
            //*/
            

            JSONObject BlocksData = obj["buildings"]["features"];
            blocks.Add(new TileExtrusion(this, BlocksData, map.parent.tiles, new Color(.1f, .1f, .1f)));
            
            JSONObject EarthData = obj["earth"]["features"];
            flatTiles.Add(new TileFlat(this, EarthData, map.parent.tiles, new Color(.0f, .8f, .1f), 0, false));

            JSONObject WaterData = obj["water"]["features"];
            flatTiles.Add(new TileFlat(this, WaterData, map.parent.tiles, new Color(.1f, .6f, .95f), 0, false));
            
        }
        
    }
}
