using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelData")]
    public class LevelDataScriptable : ScriptableObject
    {
        public bool m_needsTutorial = false;
        public List<string> m_levelNames = new List<string>();
        public List<int> m_waveCount;
    }
}