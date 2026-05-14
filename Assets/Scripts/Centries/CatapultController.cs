using UnityEngine;
using UnityEngine.Events;

public class CatapultController : InventoryHolder, IDamagable, IInteractable, ITargetable
{
    [Header("Tipo de objetivo")]
    [SerializeField] private TargetType _targetType = TargetType.LooseObject;
    public TargetType TargetType => _targetType;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    [Header("Vida")]
    [SerializeField] private int _maxHealth;
    private int _currentHealth;

    [Header("Tipos de proyectil")]
    [SerializeField] private InventoryItemData _rockProjectile;
    [SerializeField] private CatapultProjectileSO _rockProjStats;

    [SerializeField] private InventoryItemData _coconutProyectile;
    [SerializeField] private CatapultProjectileSO _cocoProyStats;

    [Header("Configutacion de disparo")]
    [SerializeField] private float _detectionRadius = 15f;
    [SerializeField] private float _detectionAngle = 45f;
    [SerializeField] private float _fireRate = 3;
    private float _timer;

    private GameObject _currentTarget;

    protected override void Awake()
    {
        base.Awake();
        _currentHealth = _maxHealth;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _fireRate)
        {
            _timer = 0;
            FindTarget();

            if (_currentTarget != null)
                TryFire();
        }
    }

    private void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _detectionRadius);
        float closestDistance = Mathf.Infinity;
        _currentTarget = null;


        foreach (Collider col in hits)
        {   
            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemy == null) continue;

            Vector3 dir = col.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, dir);

            if (angle > _detectionAngle) continue;

            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                _currentTarget = col.gameObject;
            }
        }
    }

    private void TryFire()
    {
        CatapultProjectileSO projectileData = null;
        
        foreach (var slot in PrimaryInventorySystem.InventorySlots)
        {
            if (slot.ItemData == null || slot.StackSize <= 0) continue;

            if (slot.ItemData == _rockProjectile && _rockProjStats != null)
            {
                projectileData = _rockProjStats;
                slot.RemoveFromStack(projectileData.projectileCount);
                
                if (slot.StackSize <= 0) 
                    slot.ClearSlot();
                PrimaryInventorySystem.OnInventorySlotChanged?.Invoke(slot);
                break;
            }
            else if (slot.ItemData == _coconutProyectile && _cocoProyStats != null)
            {
                projectileData = _cocoProyStats;
                slot.RemoveFromStack(projectileData.projectileCount);
               
                if(slot.StackSize <= 0)
                    slot.ClearSlot();
                PrimaryInventorySystem.OnInventorySlotChanged?.Invoke(slot);
                break;
            }
        }

        if (projectileData == null) return;
        
        Fire(projectileData);
    }

    private void Fire(CatapultProjectileSO projectileData)
    {
        if (_currentTarget == null || projectileData.projectilePrefab == null) return;

        Vector3 targetPos = _currentTarget.transform.position;

        Vector3 spawnPos = targetPos + Vector3.up * projectileData.heightOffset;

        GameObject projectile = Instantiate(projectileData.projectilePrefab, spawnPos, Quaternion.identity);

        CatapultProjectile proj = projectile.GetComponent<CatapultProjectile>();
        if (proj != null)
            proj.Init(projectileData.explosionRadius, projectileData.explosionDamage, _currentTarget);
    }

    public void DamageRecived(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
            Destroy(gameObject);
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        OnDynamicInventoryDisplayRequested?.Invoke(PrimaryInventorySystem);
        interactSuccessful = true;
    }

    public void EndInteraction() { }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);

        Gizmos.color = Color.cyan;
        Vector3 leftBoundary = Quaternion.Euler(0, -_detectionAngle, 0) * transform.forward * _detectionRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, _detectionAngle, 0) * transform.forward * _detectionRadius;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
