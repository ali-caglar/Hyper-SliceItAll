using System.Collections;
using UnityEngine;
using TMPro;

public class CurrencyController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _totalCurrencyTMP;
    [SerializeField] private TextMeshProUGUI _earnedCurrencyTMP;
    
    private int _totalCurrency;
    private int _earnedCurrencyOnThisLevel;

    private void OnEnable()
    {
        GameManager.OnStateChanged += CheckGameState;
        Sliceable.OnObjectSliced += UpdateCurrency;
        
        UpdateUI();
    }

    private void OnDisable()
    {
        GameManager.OnStateChanged -= CheckGameState;
        Sliceable.OnObjectSliced -= UpdateCurrency;
    }

    public void ApplyBonus(int multiplier)
    {
        StartCoroutine(Bonus(multiplier));
    }

    private IEnumerator Bonus(int multiplier)
    {
        yield return new WaitForSeconds(2f);

        _earnedCurrencyOnThisLevel *= multiplier;
        _earnedCurrencyTMP.text = $"+ {_earnedCurrencyOnThisLevel}";
        
        yield return new WaitForSeconds(0.5f);

        _totalCurrency += _earnedCurrencyOnThisLevel;
        _totalCurrencyTMP.text = $"$ {_totalCurrency}";
    }

    private void CheckGameState()
    {
        GameState state = GameManager.Instance.CharacterState;

        if (state == GameState.Start)
        {
            _earnedCurrencyOnThisLevel = 0;
        }
        else if (state == GameState.Win)
        {
            _earnedCurrencyTMP.text = $"+ {_earnedCurrencyOnThisLevel}";
        }
    }

    private void UpdateCurrency(int score)
    {
        _totalCurrency += score;
        _earnedCurrencyOnThisLevel += score;
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        _totalCurrencyTMP.text = $"$ {_totalCurrency}";
    }
}