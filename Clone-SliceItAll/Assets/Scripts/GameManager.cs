using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static event Action OnStateChanged;
    public static GameManager Instance { get; private set; }
    public GameState CharacterState { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void SetGameState(GameState state)
    {
        if (state == CharacterState) return;

        CharacterState = state;
        OnStateChanged?.Invoke();
    }
}