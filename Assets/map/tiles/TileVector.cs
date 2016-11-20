using System.Collections.Generic;
using UnityEngine;

namespace XYZMap
{

    class TileVector : MapTile
    {

        private List<TilePOI> pois = new List<TilePOI>();
        private List<TileExtrusion> extrusions = new List<TileExtrusion>();
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

            //buildings
            extrusions.Add(new TileExtrusion(this, obj["buildings"]["features"], map.parent.tiles, new Color(.9f, .9f, .9f)));
            
            //landuse
            flatTiles.Add(new TileFlat(this, obj["landuse"]["features"], map.parent.tiles, new Color(1, .6f, .0f), -5 ));
            
            //earth 
            flatTiles.Add(new TileFlat(this, obj["earth"]["features"], map.parent.tiles, new Color(.0f, .8f, .3f), -10 ));
            
            //water
            flatTiles.Add(new TileFlat(this, obj["water"]["features"], map.parent.tiles, new Color(.1f, .6f, .95f),-15 ));
            //*/

        }
        
    }
}
