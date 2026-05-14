using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DayNightSpawnManager : MonoBehaviour
{
    public enum DayCycleState
    {
        Day,
        Night
    }

    public DayCycleState CurrentState => _currentState;

    [Header("Control iluminacion del sol")]
    [SerializeField] private Transform _sunTransform;
    [SerializeField] private WeatherProfileSO _normalWeather;
    [SerializeField] private WeatherProfileSO _bloodMoonWeather;
    [SerializeField] private Light _sunLight;

    private WeatherProfileSO _currentWeather;

    [Header("Duracion del dia y la noche")]
    [SerializeField, Range(0f, 1f)] private float _nightActiveSpawnPercentage = 0.2f;
    [SerializeField] private float _dayDuration;
    [SerializeField] private float _nightDuration;

    [Header("Rondas")]
    [SerializeField] private int _nightsToIncreaseSpeed = 3;
    [SerializeField] private NightRoundDataSO[] _nightRounds;
    [SerializeField] private NightRoundDataSO _defaultRound;
    [SerializeField] private int _maxNightsSurvive;

    private NightRoundDataSO _currentRound;

    private DayCycleState _currentState;
    private int _nightsCount = -1;
    private float _timer;

    [Header("Variables de spawn y enemigos")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _tankPrefab;
    private List<GameObject> _enemyInScene = new List<GameObject>();
    private List<Transform> _spawnPoints = new List<Transform>();

    [Header("Riesgo durnate la exploracion")]
    [SerializeField, Range(0f, 1f)] private float _extraWaveDurationPercentage = 0.15f;
    [SerializeField, Range(0, 100)] private int _randomWaveChance = 20;
    [SerializeField] private float _checkIntertval = 10;

    private float _randomWaveTimer = 0;
    private bool _isExtraWaveActive = false; 
    private float _currentExtraWaveTimer = 0;

    private float _spawnTimer = 0;
    private void Start()
    {
        if (_nightRounds != null && _nightRounds.Length > 0)
        {
            _currentRound = _nightRounds[0];
        }
        else
        {
            _currentRound = _defaultRound;
        }
        StartDay();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        UpdateVisuals();

        if (_currentState == DayCycleState.Night)
            HandleNightSpawning();

        if (_timer <= 0)
        {
            if (_currentState == DayCycleState.Day)
                StartNight();
            else if (_currentState == DayCycleState.Night)
            {
                if ((_nightsCount + 1) >= _maxNightsSurvive)
                {
                    GameStateManager.Instance.ChangeGameState(GameState.StateType.WIN);
                    return;
                }

                StartDay();
            }
        }
    }

    private void StartDay()
    {
        _isExtraWaveActive = false;
        _currentState = DayCycleState.Day;
        _timer = _dayDuration;
        _spawnTimer = 0;

        _currentWeather = _normalWeather;

        CleanRemainingEnemies();
    }

    private void StartNight()
    {
        _isExtraWaveActive = false;
        _currentState = DayCycleState.Night;
        _timer = _nightDuration;
        _nightsCount++;
        _spawnTimer = 0;

        _currentRound = GetCurrentRound();
        LoadSpawnPointsForRound(_currentRound);


        if ((_nightsCount + 1) % _nightsToIncreaseSpeed == 0)
        {
            _currentWeather = _bloodMoonWeather;
            Debug.Log("Aumento de nivel");
        }
        else
        {
            _currentWeather = _normalWeather;
            Debug.Log("A mimir");
        }
    }

    private NightRoundDataSO GetCurrentRound()
    {
        if (_nightRounds != null && _nightsCount < _nightRounds.Length)
            return _nightRounds[_nightsCount];
        return _defaultRound;
    }

    private void LoadSpawnPointsForRound(NightRoundDataSO round)
    {
        _spawnPoints.Clear();

        SpawnGroup[] allGroups = FindObjectsByType<SpawnGroup>(FindObjectsSortMode.None);

        foreach (SpawnGroup group in allGroups)
        {
            bool groupIsActive = round.activeSpawnsID.Contains(group.GroupID);

            if (groupIsActive)
            {
                foreach (Transform point in group.SpawnPoints)
                    _spawnPoints.Add(point);
            }
        }
    }

    private void HandleNightSpawning()
    {
        CleanDeadEnemies();

        float nightProgress = 1f - (_timer / _nightDuration);

        if (nightProgress <= _nightActiveSpawnPercentage)
            RunStandardHordeLogic();
        else
            HandleExploratioRisk();
    }

    private void RunStandardHordeLogic()
    {
        _spawnTimer += Time.deltaTime;

        if (_spawnTimer >= _currentRound.spawnRate && _enemyInScene.Count < _currentRound.maxEnemies)
        {
            _spawnTimer = 0;
            SpawnEnemy();
        }
    }

    private void HandleExploratioRisk()
    {
        if (_isExtraWaveActive)
        {
            RunStandardHordeLogic();

            _currentExtraWaveTimer -=Time.deltaTime;
            if (_currentExtraWaveTimer <= 0)
                _isExtraWaveActive = false;
        }
        else
        {
            if (_enemyInScene.Count == 0)
            {
                _randomWaveTimer += Time.deltaTime;

                if (_randomWaveTimer >= _checkIntertval)
                {
                    _randomWaveTimer = 0;

                    if (Random.Range(0,100) <= _randomWaveChance)
                    {
                        _isExtraWaveActive = true;
                        _currentExtraWaveTimer = _nightDuration * _extraWaveDurationPercentage;
                    }
                }
            }
        }
    }

    private void SpawnEnemy()
    {
        if (_spawnPoints.Count == 0) return;

        int randomIndex = Random.Range(0, _spawnPoints.Count);
        Transform selectedPoint = _spawnPoints[randomIndex];

        bool spawnTank = _tankPrefab != null &&
                TankController.ActiveTanksCount < _currentRound.maxTanks &&
                Random.Range(0, 100) < _currentRound.tankSpawnChance;

        GameObject prefabToSpawn = spawnTank ? _tankPrefab : _enemyPrefab;
        GameObject newEnemy = Instantiate(prefabToSpawn, selectedPoint.position, selectedPoint.rotation);

        _enemyInScene.Add(newEnemy);
    }

    private void CleanDeadEnemies()
    {
        for (int i = _enemyInScene.Count - 1; i >= 0; i--)
        {
            if ( _enemyInScene[i] == null )
                _enemyInScene.RemoveAt(i);
        }
    }

    private void CleanRemainingEnemies()
    {
        CleanDeadEnemies();

        for (int i = 0; i < _enemyInScene.Count; i++)
        {
            if (_enemyInScene[i] != null)
                Destroy(_enemyInScene[i]);
        }

        CleanDeadEnemies();
    }

    private void UpdateVisuals()
    {
        if (_sunTransform == null || _sunLight == null || _currentWeather == null) return;

        float progress;
        float currentAngle;
        float transitionThreshold = 0.8f; 

        Color sunColor, ambientColor, fogColor, skyTop, skyBottom, horizon;

        if (_currentState == DayCycleState.Day)
        {
            progress = 1f - (_timer / _dayDuration);
            currentAngle = Mathf.Lerp(0f, 180f, progress);

            sunColor = _currentWeather.dayDirectionalColor.Evaluate(progress);
            ambientColor = _currentWeather.dayAmbientColor.Evaluate(progress);
            fogColor = _currentWeather.dayFogColor.Evaluate(progress);
            skyTop = _currentWeather.daySkyTop.Evaluate(progress);
            skyBottom = _currentWeather.daySkyBottom.Evaluate(progress);
            horizon = _currentWeather.dayHorizon.Evaluate(progress);

            if (progress >= transitionThreshold)
            {
                float blend = (progress - transitionThreshold) / (1f - transitionThreshold); 

                int nextNightCount = _nightsCount + 1;
                WeatherProfileSO nextWeather = ((nextNightCount + 1) % _nightsToIncreaseSpeed == 0) ? _bloodMoonWeather : _normalWeather;

                sunColor = Color.Lerp(sunColor, nextWeather.nightDirectionalColor.Evaluate(0f), blend);
                ambientColor = Color.Lerp(ambientColor, nextWeather.nightAmbientColor.Evaluate(0f), blend);
                fogColor = Color.Lerp(fogColor, nextWeather.nightFogColor.Evaluate(0f), blend);
                skyTop = Color.Lerp(skyTop, nextWeather.nightSkyTop.Evaluate(0f), blend);
                skyBottom = Color.Lerp(skyBottom, nextWeather.nightSkyBottom.Evaluate(0f), blend);
                horizon = Color.Lerp(horizon, nextWeather.nightHorizon.Evaluate(0f), blend);
            }
        }
        else 
        {
            progress = 1f - (_timer / _nightDuration);
            currentAngle = Mathf.Lerp(180f, 360f, progress);

            sunColor = _currentWeather.nightDirectionalColor.Evaluate(progress);
            ambientColor = _currentWeather.nightAmbientColor.Evaluate(progress);
            fogColor = _currentWeather.nightFogColor.Evaluate(progress);
            skyTop = _currentWeather.nightSkyTop.Evaluate(progress);
            skyBottom = _currentWeather.nightSkyBottom.Evaluate(progress);
            horizon = _currentWeather.nightHorizon.Evaluate(progress);

            if (progress >= transitionThreshold)
            {
                float blend = (progress - transitionThreshold) / (1f - transitionThreshold);

                WeatherProfileSO nextWeather = _normalWeather;

                sunColor = Color.Lerp(sunColor, nextWeather.dayDirectionalColor.Evaluate(0f), blend);
                ambientColor = Color.Lerp(ambientColor, nextWeather.dayAmbientColor.Evaluate(0f), blend);
                fogColor = Color.Lerp(fogColor, nextWeather.dayFogColor.Evaluate(0f), blend);
                skyTop = Color.Lerp(skyTop, nextWeather.daySkyTop.Evaluate(0f), blend);
                skyBottom = Color.Lerp(skyBottom, nextWeather.daySkyBottom.Evaluate(0f), blend);
                horizon = Color.Lerp(horizon, nextWeather.dayHorizon.Evaluate(0f), blend);
            }
        }

        _sunLight.color = sunColor;
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.fogColor = fogColor;

        RenderSettings.skybox.SetColor("_SkyGradientTop", skyTop);
        RenderSettings.skybox.SetColor("_SkyGradientBottom", skyBottom);
        RenderSettings.skybox.SetColor("_HorizonLineColor", horizon);

        _sunTransform.rotation = Quaternion.Euler(currentAngle, -30f, 0f);
    }

    public void SkipToNextDay()
    {
        _timer = 0f;
        _isExtraWaveActive = false;
    }

    public bool CanPlayerSleep(out string reason)
    {
        if (_currentState == DayCycleState.Day)
        {
            reason = "No puedes dormir durante el día.";
            return false;
        }

        float nightProgress = 1f - (_timer / _nightDuration);
        if (nightProgress <= _nightActiveSpawnPercentage)
        {
            reason = "Aún es pronto, la horda obligatoria no ha terminado.";
            return false;
        }

        if (_isExtraWaveActive)
        {
            reason = "No puedes dormir, estás sufriendo un ataque sorpresa.";
            return false;
        }

        if (_enemyInScene.Count > 0)
        {
            reason = "No puedes dormir, hay enemigos acechando en la zona.";
            return false;
        }

        reason = "";
        return true;
    }
}
