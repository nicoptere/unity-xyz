using UnityEngine;
using System.Collections;

public class MapUtils{
    
    static public float RAD = Mathf.PI / 180F;
    static public float EARTH_RADIUS = 6378137F;

    static public bool isPowerOfTwo(float value) {
        return isPowerOfTwo( (int)value);
    }
    static public bool isPowerOfTwo(int value){
        return (( value & -value ) == value);
    }

    static public int powerTwo(float val){

        if (isPowerOfTwo(val)) return (int) val;
        int b = 1;
        while (b < Mathf.FloorToInt(val)) b = b << 1;
        return b;

    }

    //http://www.movable-type.co.uk/scripts/latlong.html
    static public float latLngDistance( float lat1, float lng1, float lat2, float lng2){
        float R = EARTH_RADIUS; // meters
        float p1 = lat1 * RAD;
        float p2 = lat2 * RAD;
        float tp = (lat2 - lat1) * RAD;
        float td = (lng2 - lng1) * RAD;

        float a = Mathf.Sin(tp / 2) * Mathf.Sin(tp / 2) +
                Mathf.Cos(p1) * Mathf.Cos(p2) *
                Mathf.Sin(td / 2) * Mathf.Sin(td / 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return R * c;
    }

    static public float lerp(float t, float a, float b) { return a + t * (b - a); }
    static public float norm(float t, float a, float b) { return (t - a) / (b - a); }
    static public float map(float t, float a0, float b0, float a1, float b1) { return lerp(norm(t, a0, b0), a1, b1); }
    
}
