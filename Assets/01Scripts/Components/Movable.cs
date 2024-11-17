using DG.Tweening;
using System.Linq;
using UnityEngine;
using System;

namespace FOMO
{
    public delegate void VoidDelegate();

    public abstract class Movable : GridElement
    {
        private int _movementDirection;

        public Action<Movable, int, int> MovementEnded;

        private float MovementDuration(Vector3 targetPos) => Vector3.Distance(transform.position, targetPos) / Constants.Numbers.BLOCK_SPEED;

        protected void Move(Vector3 pos, int directionNumber, VoidDelegate onComplete)
        {
            _movementDirection = directionNumber;
            transform.DOMove(pos, MovementDuration(pos)).SetEase(Ease.Linear).OnComplete(() => {
                MovementEnded?.Invoke(this, _movementDirection, 2);
                onComplete();
            });
        }

        public abstract void GetBumped(int direction, float strength = .5f);

        public void Move(Vector3 pos, int directionNumber)
        {
            Move(pos, directionNumber, () => { });
        }
    }
}