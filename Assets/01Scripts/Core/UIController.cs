using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FOMO
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private Button nextButton, retryButton;
        [SerializeField] private Canvas gameplayCanvas, winCanvas, loseCanvas;
        [SerializeField] private TMP_Text levelText, moveCountText;

        public Action NextButtonClick, RetryButtonClick;

        public void Initialize(int level, int moveCount)
        {
            levelText.text = "Level " + level;
            winCanvas.enabled = false;
            loseCanvas.enabled = false;
            gameplayCanvas.enabled = true;

            if (moveCount > 0)
            {
                moveCountText.text = moveCount.ToString();
                moveCountText.gameObject.SetActive(true);
            }
            else
                moveCountText.gameObject.SetActive(false);
        }

        private void OnNextButtonClick()
        {
            nextButton.onClick.RemoveListener(OnNextButtonClick);
            NextButtonClick?.Invoke();
        }

        private void OnRetryButtonClick()
        {
            retryButton.onClick.RemoveListener(OnRetryButtonClick);
            RetryButtonClick?.Invoke();
        }

        public void SetMoveCount(int moveCount)
        {
            moveCountText.text = moveCount.ToString();
        }

        public void ShowFinalCanvas(bool isWin)
        {
            gameplayCanvas.enabled = false;
            loseCanvas.enabled = !isWin;
            winCanvas.enabled = isWin;

            if (isWin)
            {
                nextButton.onClick.AddListener(OnNextButtonClick);
            } else
            {
                retryButton.onClick.AddListener(OnRetryButtonClick);
            }
        }
    }
}