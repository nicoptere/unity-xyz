using UnityEngine;

public class MapRect
{

    public float x;
    public float y;
    public float w;
    public float h;

    public MapRect(int _x, int _y, int _w, int _h)
    {
        x = (float)_x;
        y = (float)_y;
        w = (float)_w;
        h = (float)_h;
    }

    public MapRect(float _x, float _y, float _w, float _h)
    {
        x = _x;
        y = _y;
        w = _w;
        h = _h;
    }

    public bool containsPoint(float _x, float _y)
    {
        if (_x < x) return false;
        if (_y < y) return false;
        if (_x > x + w) return false;
        return _y <= y + h;
    }

    public bool isContained(float _x, float _y, float _w, float _h)
    {
        return (x >= _x
        && y >= _y
        && x + w <= _x + _w
        && y + h <= _y + _h);
    }

    public bool intersect(float _x, float _y, float _w, float _h)
    {
        return !(_x > x + w || _x + _w < x || _y > y + h || _y + _h < y);
    }

    public MapRect intersection(MapRect other)
    {

        if (intersect(other.x, other.y, other.x + other.w, other.y + other.h))
        {
            var _x = Mathf.Max(x, other.x);
            var _y = Mathf.Max(y, other.y);
            var _w = Mathf.Min(x + w, other.x + other.w) - _x;
            var _h = Mathf.Min(y + h, other.y + other.h) - _y;
            return new MapRect(_x, _y, _w, _h);
        }
        return null;
    }

}
