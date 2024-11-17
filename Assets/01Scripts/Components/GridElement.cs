using System;
using UnityEngine;

namespace FOMO
{
    public abstract class GridElement : MonoBehaviour
    {
        [SerializeField] private int length;

        [NonSerialized] public Vector2Int coordinates;

        public int Length => length;
        public Dimention Dimention { get; private set; }

        public virtual void Initialize(Dimention dimention)
        {
            Dimention = dimention;
        }

        public Vector2Int GetLastCellAtDirection(int directionNumber)
        {
            if (Length == 1 || (directionNumber is 0 or 3)) return coordinates;

            if (directionNumber == 1) return coordinates + (Length - 1) * Vector2Int.right;

            return coordinates + (Length - 1) * Vector2Int.up;
        }
    }
}