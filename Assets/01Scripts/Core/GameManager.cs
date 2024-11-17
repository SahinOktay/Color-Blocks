using FOMO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FOMO
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int levelCount = 4;
        [SerializeField] private LevelController levelController;
        [SerializeField] private PoolManager poolManager;

        private int _currentLevel;

        private void Start()
        {
            _currentLevel = PlayerPrefs.GetInt(Constants.Prefs.NEXT_LEVEL, 1);
            poolManager.Initialize();
            levelController.poolManager = poolManager;
            levelController.InitializeLevel(_currentLevel);
            levelController.LevelComplete += OnLevelComplete;
            levelController.LevelFail += OnLevelFail;
        }

        private void OnLevelComplete()
        {
            ++_currentLevel;
            if (
                PlayerPrefs.GetInt(Constants.Prefs.ALL_LEVELS_COMPLETE, 0) == 1
            )
                _currentLevel = Random.Range(1, levelCount + 1);
            else if (_currentLevel > levelCount)
            {
                PlayerPrefs.SetInt(Constants.Prefs.ALL_LEVELS_COMPLETE, 1);
                _currentLevel = Random.Range(1, levelCount + 1);
            }

            PlayerPrefs.SetInt(Constants.Prefs.NEXT_LEVEL, _currentLevel);
            PlayerPrefs.Save();

            levelController.ClearLevel();
            levelController.InitializeLevel(_currentLevel);
        }

        private void OnLevelFail()
        {
            levelController.ClearLevel();
            levelController.InitializeLevel(_currentLevel);
        }
    }
}
