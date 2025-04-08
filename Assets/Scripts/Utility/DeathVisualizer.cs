using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Utility
{
    public class DeathVisualizer : MonoBehaviour
    {
        [Header("Grid Settings")]
        public Vector2 gridOrigin = new Vector2(-50, -50);  // Bottom-left of the level
        public Vector2 gridSize = new Vector2(100, 100);    // Width & height of the level
        public float cellSize = 5f;                             // Size of each block

        [Header("Data Input")]
        public TextAsset jsonFile;
        public string m_currentLevelName;
        
        private int[,] heatmapGrid;
        private int rows, cols;

        [System.Serializable]
        public class DeathLocation
        {
            public float x;
            public float y;
        
            public override string ToString()
            {
                return $"({x}, {y})";
            }
        }

        private List<DeathLocation> m_deathLocations;

        private List<DeathLocation> ExtractDeathLocations(string jsonText)
        {
            List<DeathLocation> allDeathLocations = new List<DeathLocation>();
            
            // Parse the JSON
            JObject metricsData = JObject.Parse(jsonText);
            
            foreach (var sessionProperty in metricsData.Properties())
            {
                string sessionID = sessionProperty.Name;
                JObject sessionData = (JObject)sessionProperty.Value;
                
                // Check if this session has level metrics data
                if (sessionData["m_levelMetricsData"] != null)
                {
                    JArray levelMetrics = (JArray)sessionData["m_levelMetricsData"];
                    foreach (JObject levelData in levelMetrics)
                    {
                        string levelName = levelData["m_levelName"]?.ToString();
                        
                        // Check if this level has death locations
                        if (levelData["m_deathLocations"] != null && levelName == m_currentLevelName)
                        {
                            JArray deathLocations = (JArray)levelData["m_deathLocations"];
                            
                            // Iterate through each death location
                            foreach (JObject location in deathLocations)
                            {
                                DeathLocation deathLoc = new DeathLocation
                                {
                                    x = float.Parse(location["x"].ToString()),
                                    y = float.Parse(location["y"].ToString())
                                };
                                
                                allDeathLocations.Add(deathLoc);
                            }
                        }
                    }
                }
            }
            
            return allDeathLocations;
        }
        
        // Load death locations
        public void LoadDeathLocations()
        {
            if (jsonFile != null)
            {
                m_deathLocations = ExtractDeathLocations(jsonFile.text);
                Debug.Log($"Found {m_deathLocations.Count} death locations");
            }
        }

        public void ProcessDeathLocations()
        {
            rows = Mathf.CeilToInt(gridSize.y / cellSize);
            cols = Mathf.CeilToInt(gridSize.x / cellSize);
            heatmapGrid = new int[cols, rows];
                
            // Do something with the death locations
            foreach (DeathLocation location in m_deathLocations)
            {
                Debug.Log($"Death at {location}");
                    
                int col = Mathf.FloorToInt((location.x - gridOrigin.x) / cellSize);
                int row = Mathf.FloorToInt((location.y - gridOrigin.y) / cellSize);
            
                if (col >= 0 && col < cols && row >= 0 && row < rows)
                {
                    heatmapGrid[col, row]++;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (heatmapGrid == null) return;

            int maxDeaths = 1;
            foreach (int count in heatmapGrid)
            {
                if (count > maxDeaths) maxDeaths = count;
            }

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    int deathCount = heatmapGrid[x, y];
                    if (deathCount > 0)
                    {
                        float alpha = (float)deathCount / maxDeaths;
                        Color color = new Color(1f, 0f, 0f, alpha); // red with intensity
                        Gizmos.color = color;

                        Vector3 center = new Vector3(
                            gridOrigin.x + x * cellSize + cellSize / 2,
                            gridOrigin.y + y * cellSize + cellSize / 2,
                            0f
                        );

                        Gizmos.DrawCube(center, new Vector3(cellSize, cellSize, 0.1f));
                    }
                }
            }
        }
    }
}