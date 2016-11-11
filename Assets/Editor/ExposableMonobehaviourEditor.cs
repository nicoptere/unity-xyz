﻿using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(ExposableMonobehavior), true)]
public class ExposableMonobehaviourEditor : Editor
{
    ExposableMonobehavior m_Instance;
    PropertyField[] m_fields;

    public virtual void OnEnable()
    {
        m_Instance = target as ExposableMonobehavior;
        m_fields = ExposeProperties.GetProperties(m_Instance);
    }

    public override void OnInspectorGUI()
    {
        if (m_Instance == null)
            return;
        this.DrawDefaultInspector();
        ExposeProperties.Expose(m_fields);
    }
}