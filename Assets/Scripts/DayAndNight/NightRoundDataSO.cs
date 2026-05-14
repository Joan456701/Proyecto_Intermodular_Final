using UnityEngine;

[CreateAssetMenu(fileName = "NightRound", menuName = "Day Night/Night Round Data")]
public class NightRoundDataSO : ScriptableObject
{
    [Header("Enemigos")]
    public int maxEnemies = 5;
    public float spawnRate = 6f;

    [Header("Tanques")]
    public int maxTanks = 0;
    [Range(0, 100)]
    public int tankSpawnChance = 0;

    [Header("Spawns activos")]
    public string[] activeSpawnsID;
}
