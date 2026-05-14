using UnityEngine;
using UnityEngine.AI;

public class TankController : MonoBehaviour, IDamagable, ITargetable
{
    public static int ActiveTanksCount { get; private set; } = 0;

    [Header("Tipo de objetivo")]
    [SerializeField] private TargetType _targetType = TargetType.Enemy;
    public TargetType TargetType => _targetType;

    [Header("Configuracion deteccion torretas")]
    [SerializeField] private float _centryDetectionRadius = 10f;
    [SerializeField] private float _centryDetectionAngle = 60f;

    [Header("Variables de ataque")]
    [SerializeField] private float _attackDistance = 4f;
    [SerializeField] private float _cooldown = 2;
    [SerializeField] private int _attackDamage = 1;

    private float _timeSinceLastAttack = 0;

    [Header("Vida")]
    [SerializeField] private int _maxHealth;
    private int _health;

    [Header("Inteligencia de Destruccion")]
    [SerializeField] private float _maxDetourMultiplier = 0.25f;


    [Header("Botin del Enemigo")]
    [SerializeField] private LootObject[] _standardDrops;
    [SerializeField] private LootObject[] _exclusiveDrops;

    private BoxCollider _spaceshipCollider;
    private Transform _spaceshipTarget;
    private Transform _currentTarget;
    private Transform _centryTarget;
    private NavMeshAgent _navAgent;

    private bool isJumping = false;

    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();

        _navAgent.stoppingDistance = _attackDistance - 0.5f;
    }

    private void Start()
    {
        SpaceshipIdentificator _spaceship = FindFirstObjectByType<SpaceshipIdentificator>();

        ActiveTanksCount++;

        if (_spaceship != null)
        {
            _spaceshipTarget = _spaceship.transform;
            _spaceshipCollider = _spaceship.GetComponent<BoxCollider>();
        }

        _currentTarget = _spaceshipTarget.transform;
        UpdateDestination();
        
        _health = _maxHealth;
    }

    private void OnDestroy()
    {
        ActiveTanksCount--;
    }

    private void Update()
    {
        Debug.Log(_currentTarget);
        if (_spaceshipTarget == null) return;

        if (_centryTarget != null && _centryTarget.gameObject == null)
            _centryTarget = null;

        if (_centryTarget == null)
            DetectCentries();

        if (_centryTarget != null)
            HandleCentryTarget();
        else
            HandleSpaceshipTarget();

        if (_navAgent.isOnOffMeshLink && !isJumping)
            StartCoroutine(SmoothJump());
    }

    private void DetectCentries()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _centryDetectionRadius);

        float closestDistance = Mathf.Infinity;
        Transform closestCentry = null;

        foreach (Collider col in hits)
        {
            CentryIdentificator centry = col.GetComponent<CentryIdentificator>();
            if (centry == null) continue;

            Vector3 dir = col.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > _centryDetectionAngle) continue;

            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCentry = col.transform;
            }
        }

        if (closestCentry != null)
        {
            _centryTarget = closestCentry;
            _currentTarget = _centryTarget;
        }
    }

    private void HandleCentryTarget()
    {
        if (_centryTarget == null)
        {
            _currentTarget = _spaceshipTarget;
            UpdateDestination();
            return;
        }

        Collider turretCollider = _centryTarget.GetComponent<Collider>();
        Vector3 targetPos = turretCollider != null
            ? turretCollider.ClosestPoint(transform.position)
            : _centryTarget.position;

        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance <= _attackDistance)
        {
            _navAgent.isStopped = true;
            AttackCentry();
        }
        else
        {
            _navAgent.isStopped = false;
            _navAgent.SetDestination(targetPos);
        }
    }

    private void AttackCentry()
    {
        _timeSinceLastAttack += Time.deltaTime;

        if (_timeSinceLastAttack >= _cooldown)
        {
            if (_centryTarget == null)
            {
                _currentTarget = _spaceshipTarget;
                UpdateDestination();
                return;
            }

            IDamagable centryDamagable = _centryTarget.GetComponent<IDamagable>();
            if (centryDamagable != null)
            {
                centryDamagable.DamageRecived(_attackDamage);
                _timeSinceLastAttack = 0;
            }
            else
            {
                _centryTarget = null;
                _currentTarget = _spaceshipTarget;
                UpdateDestination();
            }
        }
    }

    private void HandleSpaceshipTarget()
    {
        _currentTarget = _spaceshipTarget;

        Vector3 targetPos = _spaceshipTarget.position;
        if (_spaceshipCollider != null)
            targetPos = _spaceshipCollider.ClosestPoint(transform.position);

        NavMeshPath virtualPath = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, virtualPath);

        float straightDistance = Vector3.Distance(transform.position, targetPos);
        float walkingDistance = CalculatePathLength(virtualPath);

        bool isPathBlocked = virtualPath.status == NavMeshPathStatus.PathPartial;
        bool isDetourTooLong = walkingDistance > (straightDistance * _maxDetourMultiplier);

        Vector3 dirToSpaceship = (targetPos - transform.position).normalized;
        dirToSpaceship.y = 0;

        Vector3 rayOrigin = transform.position + Vector3.up * 1f;
        bool isWallInFront = Physics.Raycast(rayOrigin, dirToSpaceship, _attackDistance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        if (isPathBlocked || isDetourTooLong)
        {
            NavMeshHit navHit;
            if (NavMesh.Raycast(transform.position, targetPos, out navHit, NavMesh.AllAreas))
                _navAgent.SetDestination(navHit.position);

            if (dirToSpaceship != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(dirToSpaceship), Time.deltaTime * 5f);

            if (isWallInFront)
            {
                _navAgent.isStopped = true;
                TryAttackObstacle();
            }
            else
            {
                _navAgent.isStopped = false;
            }
        }
        else
        {
            _navAgent.SetDestination(targetPos);

            if (straightDistance <= _attackDistance)
            {
                _navAgent.isStopped = true;
                TryAttackObstacle();
            }
            else
            {
                _navAgent.isStopped = false;
            }
        }
    }

    private void TryAttackObstacle()
    {
        _timeSinceLastAttack += Time.deltaTime;

        if (_timeSinceLastAttack >= _cooldown)
        {
            Vector3 targetPos = _spaceshipTarget.position;
            if (_spaceshipCollider != null)
                targetPos = _spaceshipCollider.ClosestPoint(transform.position);

            Vector3 origin = transform.position + Vector3.up * 1f;
            Vector3 dirToTarget = (targetPos - transform.position).normalized;
            dirToTarget.y = 0;

            if (Physics.Raycast(origin, dirToTarget, out RaycastHit hit, _attackDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                IDamagable obstacle = hit.collider.GetComponentInParent<IDamagable>();
                if (obstacle != null)
                {
                    obstacle.DamageRecived(_attackDamage);
                    _timeSinceLastAttack = 0;
                }
            }
        }
    }

    private void UpdateDestination()
    {
        if (_currentTarget == null) return;

        Vector3 targetPos = _currentTarget.position;
        if (_currentTarget == _spaceshipTarget && _spaceshipCollider != null)
            targetPos = _spaceshipCollider.ClosestPoint(transform.position);

        _navAgent.SetDestination(targetPos);
    }

    public void DamageRecived(int damage)
    {
        _health -= damage;

        if (_health <= 0)
        {
            GiveLoot();
            Destroy(gameObject);
        }
    }

    private void GiveLoot()
    {
        PlayerInventoryHolder playerInventory = FindFirstObjectByType<PlayerInventoryHolder>();
        DayNightSpawnManager timeManager = FindFirstObjectByType<DayNightSpawnManager>();

        if (playerInventory == null) return;

        foreach (LootObject item in _standardDrops)
            TryDropItem(item, playerInventory, timeManager);

        foreach (LootObject item in _exclusiveDrops)
        {
            if (TryDropItem(item, playerInventory, timeManager))
                break;
        }
    }

    private bool TryDropItem(LootObject item, PlayerInventoryHolder inventory, DayNightSpawnManager timeManager)
    {
        if (timeManager != null)
        {
            bool isDay = timeManager.CurrentState == DayNightSpawnManager.DayCycleState.Day;
            if (item.timeCondition == TimeCondition.OnlyNight && isDay) return false;
            if (item.timeCondition == TimeCondition.OnlyDay && !isDay) return false;
        }

        int die = Random.Range(0, 100);
        if (die <= item.probability)
        {
            int dropAmount = Random.Range(item.minAmount, item.maxAmount + 1);
            if (item.itemData != null && dropAmount > 0)
            {
                bool addedToPrimary = inventory.PrimaryInventorySystem.AddToInventory(item.itemData, dropAmount);
                if (!addedToPrimary)
                    inventory.SecondaryInventorySystem.AddToInventory(item.itemData, dropAmount);
                return true;
            }
        }
        return false;
    }

    private float CalculatePathLength(NavMeshPath path)
    {
        if (path == null || path.corners.Length < 2) return 0f;

        float totalDistance = 0f;
        for (int i = 0; i < path.corners.Length - 1; i++)
            totalDistance += Vector3.Distance(path.corners[i], path.corners[i + 1]);

        return totalDistance;
    }

    private System.Collections.IEnumerator SmoothJump()
    {
        isJumping = true;

        OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
        Vector3 startPos = _navAgent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * _navAgent.baseOffset;

        float duration = 0.6f;
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime / duration;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, time);
            currentPos.y += 1f * Mathf.Sin(time * Mathf.PI);
            _navAgent.transform.position = currentPos;
            yield return null;
        }

        _navAgent.CompleteOffMeshLink();
        isJumping = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _centryDetectionRadius);

        Gizmos.color = Color.magenta;
        Vector3 leftBoundary = Quaternion.Euler(0, -_centryDetectionAngle, 0) * transform.forward * _centryDetectionRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, _centryDetectionAngle, 0) * transform.forward * _centryDetectionRadius;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}

