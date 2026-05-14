using UnityEngine;

public class SpawnGroup : MonoBehaviour
{
    [SerializeField] private string _groupID;
    public string GroupID => _groupID;

    [SerializeField] private Transform[] _spawnPoints;
    public Transform[] SpawnPoints => _spawnPoints;
}
