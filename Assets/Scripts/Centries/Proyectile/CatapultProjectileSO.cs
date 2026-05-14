using UnityEngine;

[CreateAssetMenu(fileName = "CatapultProjectile", menuName = "Inventory System/Catapult Projectile")]
public class CatapultProjectileSO : ScriptableObject
{
    [Header("Proyectil")]
    public GameObject projectilePrefab;
    public int projectileCount = 1;

    [Header("Disparo")]
    public float heightOffset = 5f;    

    [Header("Explosion")]
    public float explosionRadius = 3f;
    public int explosionDamage = 10;
}
