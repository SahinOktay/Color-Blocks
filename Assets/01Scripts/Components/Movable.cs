using DG.Tweening;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace FOMO
{
    public delegate void VoidDelegate();

    public abstract class Movable : GridElement
    {
        [SerializeField] private int length;

        public int direction;
        
        public int Length => length;
        public Dimention Dimention { get; private set; }

        public void Initialize(Dimention dimention)
        {
            Dimention = dimention;
        }

        protected void Move(Vector3 pos, VoidDelegate onComplete)
        {
            transform.DOMove(pos, MovementDuration(pos)).SetEase(Ease.Linear).OnComplete(() => onComplete());
        }

        public abstract void GetBumped(int direction, float strength = .5f);

        private float MovementDuration(Vector3 targetPos) => Vector3.Distance(transform.position, targetPos) / Constants.Numbers.BLOCK_SPEED;

        public void Move(Vector3 pos)
        {
            transform.DOMove(pos, MovementDuration(pos)).SetEase(Ease.Linear);
        }
    }
}