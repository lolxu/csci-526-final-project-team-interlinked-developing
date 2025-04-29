using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelData")]
    public class LevelDataScriptable : ScriptableObject
    {
        public bool m_needsTutorial = false;
        public bool m_needsLevelName = false;
        public bool m_needsRopeRangeIndicator = false;
        public List<string> m_levelNames = new List<string>();
        public List<int> m_waveCount = new List<int>();
        public List<bool> m_levelUnlocked = new List<bool>();

        public void SetLevelUnlocked(string levelName)
        {
            bool found = false;
            int ind = -1;
            for (int i = 0; i < m_levelNames.Count; i++)
            {
                if (m_levelNames[i] == levelName)
                {
                    ind = i;
                    found = true;
                }
            }

            if (found)
            {
                m_levelUnlocked[ind] = true;
            }
        }

        public int FindLevelIndex(string levelName)
        {
            int ind = -1;
            for (int i = 0; i < m_levelNames.Count; i++)
            {
                if (m_levelNames[i] == levelName)
                {
                    ind = i;
                }
            }

            return ind;
        }
    }
}