using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/Enemy")]
public class EnemyScriptable : ScriptableObject
{
    [Serializable]
    public class Enemy
    {
        public enum EnemyType
        {
            Basic,
            Brute,
            Stealer,
            Shooter,
            Shield,
            Spawner,
            Teleporter
        }

        public GameObject m_prefab;
        public EnemyType m_enemyType;

        [Range(0.0f, 1.0f)] 
        public float m_lootSpawnRate;
    }

    [Serializable]
    public struct RandomEnemy
    {
        public Enemy m_enemy;
        
        [Range(0.0f, 1.0f)]
        public float m_weightedValue;
    }

    public RandomEnemy enemy;
}
