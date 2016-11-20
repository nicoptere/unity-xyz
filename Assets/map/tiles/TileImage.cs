using System.Collections.Generic;
using UnityEngine;

namespace XYZMap
{
    class TileImage : MapTile
    {
        public TileImage(Map map, string quadKey)
        {
            this.map = map;
            this.quadKey = quadKey;
            initFromQuadKey(quadKey);
        }

        public override void Update(bool active)
        {
            if (!loaded) return;
            Vector3 pos = map.parent.gameObject.transform.position;
            
            if (map.parent.renderToTexture)
            {
                gameObject.hideFlags = HideFlags.HideInHierarchy;
                pos.y = map.parent.orthographicCamera.transform.position.y;
            }

            gameObject.SetActive(active);
            if( active)
            {
                float[] p = map.latLonToPixels(lat, lng);
                gameObject.transform.position = new Vector3(p[0], -1, -p[1]) + pos;
            }
        }

        public override void onDataLoaded(WWW www)
        {
            texture = www.texture;
            Renderer renderer = gameObject.GetComponent<Renderer>();
            renderer.material.shader = Shader.Find("Unlit/Texture");
            renderer.material.mainTexture = texture;
            loaded = true;
            Update(true);
        }
    }
}
