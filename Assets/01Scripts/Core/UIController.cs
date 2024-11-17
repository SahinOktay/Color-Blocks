using TMPro;
using UnityEngine;

namespace FOMO
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText, moveCountText;

        public void Initialize(int level, int moveCount)
        {
            levelText.text = "Level " + level;
            moveCountText.text = moveCount.ToString();
        }

        public void SetMoveCount(int moveCount)
        {
            moveCountText.text = moveCount.ToString();
        }
    }
}