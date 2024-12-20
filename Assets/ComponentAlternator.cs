using UnityEngine;
using System.Collections.Generic;

public class ComponentAlternator : MonoBehaviour
{
    // Static members for synchronization
    private static List<ComponentAlternator> activeAlternators = new List<ComponentAlternator>();
    private static float globalTimer;
    private static bool globalShowingArrow = true;

    // Serialized fields
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject arrowShifted;

    [Tooltip("Time in seconds between switches")]
    [SerializeField] private float switchInterval = 0.5f;

    // Internal variables
    private float minInterval;
    private bool isInitialized;

    #region Unity Lifecycle Methods

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (!isInitialized) Initialize();
        activeAlternators.Add(this);
        UpdateVisibility(globalShowingArrow);
    }

    private void OnDisable()
    {
        activeAlternators.Remove(this);
    }

    private void OnDestroy()
    {
        activeAlternators.Remove(this);
    }

    private void Update()
    {
        if (activeAlternators.Count == 0 || activeAlternators[0] != this) return;

        globalTimer += Time.deltaTime;
        if (globalTimer >= switchInterval)
        {
            globalTimer = 0f;
            globalShowingArrow = !globalShowingArrow;

            // Update all active alternators
            for (int i = activeAlternators.Count - 1; i >= 0; i--)
            {
                if (activeAlternators[i] != null)
                {
                    activeAlternators[i].UpdateVisibility(globalShowingArrow);
                }
                else
                {
                    activeAlternators.RemoveAt(i);
                }
            }
        }
    }

    #endregion

    #region Private Methods

    private void Initialize()
    {
        if (isInitialized) return;

        // Calculate minimum interval based on target frame rate
        float targetFrameRate = Application.targetFrameRate;
        if (targetFrameRate <= 0)
        {
            targetFrameRate = Screen.currentResolution.refreshRate;
        }
        minInterval = 1f / targetFrameRate;

        // Validate initial switch interval
        switchInterval = Mathf.Max(switchInterval, minInterval);

        // Validate references
        if (arrow == null || arrowShifted == null)
        {
            Debug.LogError($"Missing arrow references on {gameObject.name}", this);
            enabled = false;
            return;
        }

        isInitialized = true;
    }

    private void UpdateVisibility(bool showArrow)
    {
        if (!arrow || !arrowShifted) return;

        arrow.SetActive(showArrow);
        arrowShifted.SetActive(!showArrow);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the interval between arrow switches
    /// </summary>
    /// <param name="newInterval">New interval in seconds (will be clamped to minimum frame time)</param>
    public void SetSwitchInterval(float newInterval)
    {
        switchInterval = Mathf.Max(newInterval, minInterval);
        Debug.Log($"Switch interval set to: {switchInterval}s (Minimum allowed: {minInterval}s)");
    }

    /// <summary>
    /// Gets the current minimum allowed interval based on frame rate
    /// </summary>
    public float GetMinimumInterval()
    {
        return minInterval;
    }

    /// <summary>
    /// Gets the current switch interval
    /// </summary>
    public float GetSwitchInterval()
    {
        return switchInterval;
    }

    #endregion
}
