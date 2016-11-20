﻿using System.Collections.Generic;
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

            JSONObject obj = new JSONObject(www.text);
            /*
            JSONObject POIData = obj["pois"]["features"];
            for (int i = 0; i < POIData.Count; i++)
            {
                Debug.Log(POIData[i]);
                pois.Add( new TilePOI( this, POIData[i] ) );
            }
            //*/

            Color color = new Color(.9f, .9f, .9f);
            //buildings
            extrusions.Add(new TileExtrusion(this, obj["buildings"]["features"], map.parent.tiles, color ));
            
            //landuse
            flats.Add(new TileFlat(this, obj["landuse"]["features"], map.parent.tiles, color, -5 ));
            
            //earth 
            flats.Add(new TileFlat(this, obj["earth"]["features"], map.parent.tiles, color, -10 ));
            
            //water
            flats.Add(new TileFlat(this, obj["water"]["features"], map.parent.tiles, color,-15 ));
            //*/


            //roads
            /*
            Color color = new Color(1, 0, 0);
            lines.Add(new TileLine(this, obj["roads"]["features"], map.parent.tiles, color));
            //*/
        }

    }
}
