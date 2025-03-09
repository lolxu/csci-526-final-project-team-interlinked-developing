using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelData")]
    public class LevelDataScriptable : ScriptableObject
    {
        public List<string> m_levelNames = new List<string>();
    }
}