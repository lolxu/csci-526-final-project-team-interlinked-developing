using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Utility;

[CustomEditor(typeof(DeathVisualizer))]
public class DeathHeatmapGenerator : Editor
{
    // Editor script stuff
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DeathVisualizer visualizer = (DeathVisualizer)target;
        
        GUILayout.Label("Death Heat Map Visualizer Editor Functions");
        GUILayout.Label("Read Metrics");
        if (GUILayout.Button("Read"))
        {
            visualizer.LoadDeathLocations();
        }
        
        GUILayout.Label("Create Visualizations");
        if (GUILayout.Button("Create"))
        {
            visualizer.ProcessDeathLocations();
        }
    }
    
}