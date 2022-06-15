using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Canvasses")]
    [SerializeField] private GameObject _startCanvas;
    [SerializeField] private GameObject _inGameCanvas;
    [SerializeField] private GameObject _winCanvas;
    [SerializeField] private GameObject _loseCanvas;

    [Header("Buttons")]
    [SerializeField] private Button[] _restartButtons;
    [SerializeField] private Button _nextLevelButton;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _feedbackTMP;
    [SerializeField] private TextMeshProUGUI _currentLevelTMP;

    private LevelManager _levelManager;

    private void Awake()
    {
        _levelManager = FindObjectOfType<LevelManager>();

        _startCanvas.SetActive(CloseOtherCanvases());
        _currentLevelTMP.text = $"LEVEL {_levelManager.CurrentLevel + 1}";
    }

    private void OnEnable()
    {
        GameManager.OnStateChanged += UpdateUI;
        _nextLevelButton.onClick.AddListener(NextLevel);
        _restartButtons.ToList().ForEach(x => x.onClick.AddListener(RestartLevel));
    }

    private void OnDisable()
    {
        GameManager.OnStateChanged -= UpdateUI;
        _nextLevelButton.onClick.RemoveAllListeners();
        _restartButtons.ToList().ForEach(x => x.onClick.RemoveAllListeners());
    }

    private void UpdateUI()
    {
        GameState state = GameManager.Instance.CharacterState;

        switch (state)
        {
            case GameState.Start:
                _startCanvas.SetActive(CloseOtherCanvases());
                _currentLevelTMP.text = $"LEVEL {_levelManager.CurrentLevel + 1}";
                break;
            case GameState.InGame:
                _inGameCanvas.SetActive(CloseOtherCanvases());
                break;
            case GameState.Win:
                _winCanvas.SetActive(CloseOtherCanvases());
                break;
            case GameState.Lose:
                _loseCanvas.SetActive(CloseOtherCanvases());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void NextLevel()
    {
        _levelManager.NextLevel();
    }
    
    private void RestartLevel()
    {
        _levelManager.Restart();
    }

    private bool CloseOtherCanvases()
    {
        _startCanvas.SetActive(false);
        _inGameCanvas.SetActive(false);
        _winCanvas.SetActive(false);
        _loseCanvas.SetActive(false);

        return true;
    }
}