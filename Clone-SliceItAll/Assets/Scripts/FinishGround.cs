using UnityEngine;
using TMPro;

public class FinishGround : MonoBehaviour, IKnifeHit
{
    [SerializeField] private int _bonusMultiplier = 1;
    
    private bool _isKnifeStuck;

    private TextMeshProUGUI _multiplierTMP;
    private CurrencyController _currencyController;

    private void Awake()
    {
        _multiplierTMP = GetComponentInChildren<TextMeshProUGUI>();
        _currencyController = FindObjectOfType<CurrencyController>();
    }

    private void Start()
    {
        if (_multiplierTMP != null)
        {
            _multiplierTMP.text = $"X{_bonusMultiplier}";
        }
    }

    public void OnSharpEdgeHit(PlayerController playerController)
    {
        if (_isKnifeStuck || GameManager.Instance.CharacterState == GameState.Win) return;
        _isKnifeStuck = true;
        
        playerController.Stuck();
        
        GameManager.Instance.SetGameState(GameState.Win);
        _currencyController.ApplyBonus(_bonusMultiplier);
    }

    public void OnKnifesBackHit(PlayerController playerController)
    {
        if (_isKnifeStuck) return;
        
        playerController.JumpBack();
    }
}