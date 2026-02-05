using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private PlayerStats player;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI hpText;

    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button btnA;
    [SerializeField] private Button btnB;
    [SerializeField] private TextMeshProUGUI btnAText;
    [SerializeField] private TextMeshProUGUI btnBText;

    [Header("Progression")]
    [SerializeField] private float expPerKill = 1f;

    public EnemySpawner Spawner => spawner;
    public PlayerStats Player => player;

    public int CurrentWave => wave;
    public bool IsPaused { get; private set; }

    private int wave;
    private int pendingUpgradePicks;
    private bool isManualPause;

    private UpgradeChoice choiceA;
    private UpgradeChoice choiceB;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
        SetCursorForUI(false);

        if (spawner == null) Debug.LogWarning("[GameManager] EnemySpawner is not assigned.");
        if (player == null) Debug.LogWarning("[GameManager] PlayerStats is not assigned.");
        if (waveText == null) Debug.LogWarning("[GameManager] waveText is not assigned.");
        if (hpText == null) Debug.LogWarning("[GameManager] hpText is not assigned.");

        if (player != null)
            player.OnLevelUp += HandlePlayerLevelUp;

        NextWave();
    }

    private void OnDestroy()
    {
        if (player != null)
            player.OnLevelUp -= HandlePlayerLevelUp;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleManualPause();
        }

        UpdateHUD();

        if (!IsPaused && spawner != null && spawner.IsWaveCleared())
        {
            NextWave();
        }
    }

    private void UpdateHUD()
    {
        if (player != null && hpText != null)
        {
            hpText.text = $"HP: {Mathf.CeilToInt(player.Hp)}/{Mathf.CeilToInt(player.MaxHp)}";
        }

        if (waveText != null)
        {
            waveText.text = $"Wave {wave}";
        }
    }

    public void EnemyKilled(Enemy enemy)
    {
        if (player == null || enemy == null) return;
        player.AddExperience(enemy.ExperienceDrop);
    }

    private void NextWave()
    {
        wave++;

        if (waveText != null)
            waveText.text = $"Wave {wave}";

        if (spawner != null)
            spawner.SpawnWave(wave);
    }

    private void HandlePlayerLevelUp(int newLevel)
    {
        pendingUpgradePicks++;

        if (upgradePanel != null && !upgradePanel.activeSelf)
            ShowUpgrades();
    }

    private void ShowUpgrades()
    {
        if (upgradePanel == null) return;
        if (upgradePanel.activeSelf) return;

        upgradePanel.SetActive(true);
        RefreshPauseState();

        choiceA = UpgradeSystem.RandomChoice(player);
        do { choiceB = UpgradeSystem.RandomChoice(player); }
        while (choiceA != null && choiceB != null && choiceB.id == choiceA.id);

        if (choiceA == null || choiceB == null)
        {
            upgradePanel.SetActive(false);
            RefreshPauseState();
            return;
        }

        if (btnAText != null) btnAText.text = choiceA.title;
        if (btnBText != null) btnBText.text = choiceB.title;

        if (btnA != null)
        {
            btnA.onClick.RemoveAllListeners();
            btnA.onClick.AddListener(() => Pick(choiceA));
        }

        if (btnB != null)
        {
            btnB.onClick.RemoveAllListeners();
            btnB.onClick.AddListener(() => Pick(choiceB));
        }
    }

    private void Pick(UpgradeChoice choice)
    {
        if (choice.oneTime && !player.TryTakeUpgrade(choice.id))
            return;

        if (player != null)
            choice.apply(player);

        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        pendingUpgradePicks = Mathf.Max(0, pendingUpgradePicks - 1);

        if (pendingUpgradePicks > 0)
        {
            ShowUpgrades();
            return;
        }

        RefreshPauseState();
    }

    public void PlayerDied()
    {
        SetCursorForUI(true);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        SetCursorForUI(paused);
    }

    private void ToggleManualPause()
    {
        if (upgradePanel != null && upgradePanel.activeSelf)
            return;

        isManualPause = !isManualPause;
        RefreshPauseState();
    }

    private void RefreshPauseState()
    {
        bool isUpgradeOpen = upgradePanel != null && upgradePanel.activeSelf;
        SetPaused(isManualPause || isUpgradeOpen);
    }

    private static void SetCursorForUI(bool uiOpen)
    {
        if (uiOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        expPerKill = Mathf.Max(0.1f, expPerKill);

        if (upgradePanel != null && (btnA == null || btnB == null || btnAText == null || btnBText == null))
        {

        }
    }
#endif
}
