using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CatapultProjectile : MonoBehaviour
{
    private float _explosionRadius;
    private int _explosionDamage;
    private GameObject _target;


    public void Init(float explosionRadius, int explosionDamage, GameObject target)
    {
        _explosionRadius = explosionRadius;
        _explosionDamage = explosionDamage;
        _target = target;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _explosionRadius);

        foreach (Collider col in hitColliders)
        {
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.DamageRecived(_explosionDamage);
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        if (_target != null)
        {
            transform.position = new Vector3(
                _target.transform.position.x,
                transform.position.y,
                _target.transform.position.z
            );
        }
    }
}
