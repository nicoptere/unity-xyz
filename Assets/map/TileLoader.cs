using XYZMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XYZMap
{
    class TileLoader : MonoBehaviour
    {
        private int maxLoad = 4;
        private int loading;

        private Map map;
        private MapObject parent;
        private List<MapTile> queue;

        public TileLoader() {}

        public void init(Map map, MapObject parent )
        {
            this.map = map;
            this.parent = parent;
            queue = new List<MapTile>();
        }

        public void addTile(MapTile tile)
        {
            
            if( loading >= maxLoad)
            {
                //Debug.Log("queueing: " + tile.url);
                queue.Add(tile);
                return;
            }
            
            StartCoroutine(loadData(tile));
            
        }

        public IEnumerator loadData(MapTile tile)
        {
            loading++;

            WWW www = new WWW(tile.url);
            yield return www;
            tile.onDataLoaded(www);
            
            loading--;
            if (tile.loaded)
            {
                if( queue.Count > 0)
                {
                    MapTile next = queue[0];
                    addTile(next);
                    queue.RemoveAt(0);
                }

            }else{

                addTile(tile);
            }

        }

    }
}
