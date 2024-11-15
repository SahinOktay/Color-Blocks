using System;
using UnityEngine;

namespace FOMO
{
    [Serializable]
    public class LevelData
    {
        public int MoveLimit, RowCount, ColCount;
        public MovableInfo[] MovableInfo;
        public ExitInfo[] ExitInfo;
    }

    [Serializable]
    public class MovableInfo
    {
        public int Row, Col, Length, Colors;
        public int[] Direction;
    }

    [Serializable]
    public class ExitInfo
    {
        public int Row, Col, Direction, Colors;
    }
}
