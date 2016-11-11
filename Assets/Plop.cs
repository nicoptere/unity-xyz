using UnityEngine;
using System.Collections;

public class Plop : ExposableMonobehavior
{
    [HideInInspector, SerializeField]
    int m_SomeInt;
    [HideInInspector, SerializeField]
    float m_SomeFloat;
    [HideInInspector, SerializeField]
    bool m_SomeBool;
    [HideInInspector, SerializeField]
    string m_Etc;
    [HideInInspector, SerializeField]
    MonoBehaviour m_Obj;
    [HideInInspector, SerializeField]
    RenderTexture m_Tex;

    [ExposeProperty]
    public int SomeInt
    {
        get { return m_SomeInt; }
        set { m_SomeInt = value; }
    }

    [ExposeProperty]
    public float SomeFloat
    {
        get { return m_SomeFloat; }
        set { m_SomeFloat = value; }
    }

    [ExposeProperty]
    public bool SomeBool
    {
        get { return m_SomeBool; }
        set { m_SomeBool = value; }
    }

    [ExposeProperty]
    public string SomeString
    {
        get { return m_Etc; }
        set { m_Etc = value; }
    }

    [ExposeProperty]
    public MonoBehaviour SomeScript
    {
        get { return m_Obj; }
        set { m_Obj = value; }
    }

    [ExposeProperty]
    public RenderTexture SomeTexture
    {
        get { return m_Tex; }
//        set { m_Tex = value; }
    }
}