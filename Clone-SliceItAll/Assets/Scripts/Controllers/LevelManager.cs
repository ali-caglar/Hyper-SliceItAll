using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _levelPrefabs;

    private int _currentLevel;
    private GameObject _activeLevelPrefab;

    public int CurrentLevel => _currentLevel;

    private void Awake()
    {
        LoadLevel();
    }

    public void NextLevel()
    {
        _currentLevel++;
        LoadLevel();
    }

    public void Restart()
    {
        LoadLevel();
    }

    private void LoadLevel()
    {
        if (_activeLevelPrefab != null)
        {
            Destroy(_activeLevelPrefab);
        }

        int levelToLoad = Mathf.Clamp(_currentLevel, 0, _levelPrefabs.Length - 1);

        _activeLevelPrefab = Instantiate(_levelPrefabs[levelToLoad]);
        _activeLevelPrefab.gameObject.SetActive(true);
    }
}