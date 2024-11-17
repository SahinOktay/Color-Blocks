using UnityEngine;

namespace FOMO
{
    public delegate void GrindEvent(IGrindable grindable, bool isPositiveDirection, int Length);
    public interface IGrindable
    {
        public event GrindEvent ReachedGrinder;

        public BlockColor Color { get; }
        public void GetGrinded(Vector3 pos, float duration);
        public void MoveAndExit(Vector3 pos, bool isPositiveDirection, int directionNumber);
    }
}