using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Colors", menuName = "ScriptableObjects/Game Colors")]
public class GameColorsScriptableObject : ScriptableObject
{
    [Header("Player Stuff")]
    public Color m_playerColor;
    public Color m_playerRopeColor;
    public Color m_connectionHighlight;
    public Color m_disconnectionHighlight;

    [Header("Enemy Stuff")]
    public Color m_enemyColor;
    public Color m_enemyRopeColor;

    [Header("Ability Stuff")]
    public Color m_abilityBaseColor;
    //sjdahsaj
}
