using UnityEngine;

namespace FOMO
{
    public static class Constants
    {

        public static class Actions
        {
            public const string TOUCH_START = "TouchStart";
            public const string TOUCH_MOVE = "TouchMove";
        }
        public static class Arrays
        {
            public static int[] HORI_DIRECTIONS = new int[] { 1, 3 };
            public static int[] VER_DIRECTIONS = new int[] { 0, 2 };
        }

        public static class Calculators
        {
            public static Vector2Int GetDirectionVector(int direction)
            {
                return direction switch
                {
                    0 => Vector2Int.down,
                    1 => Vector2Int.right,
                    2 => Vector2Int.up,
                    3 => Vector2Int.left,
                    _ => Vector2Int.zero,
                };
            }
        }

        public static class Layers
        {
            public const string INTERACTABLE = "Interactable";
        }

        public static class Numbers
        {
            public const float CELL_SIZE = 1f;
            public const float BLOCK_SPEED = 10f;
        }

        public static class  Prefs
        {
            public const string NEXT_LEVEL = "NextLevel";
            public const string ALL_LEVELS_COMPLETE = "AllLevelsComplete";
        }
    }
}