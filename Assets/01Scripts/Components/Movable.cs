using DG.Tweening;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace FOMO
{
    public abstract class Movable : GridElement
    {
        [SerializeField] private int length;
        
        public int Length => length;
        public Dimention Dimention { get; private set; }

        public void Initialize(Dimention dimention)
        {
            Dimention = dimention;
        }

        private float MovementDuration(Vector3 targetPos) => Vector3.Distance(transform.position, targetPos) / Constants.Numbers.BLOCK_SPEED;

        public void Move(Vector3 pos)
        {
            transform.DOMove(pos, MovementDuration(pos)).SetEase(Ease.Linear);
        }

        public void MoveAndExit(Vector3 pos, Exit exit)
        {
            transform.DOMove(pos, MovementDuration(pos)).SetEase(Ease.Linear).OnComplete(() => { gameObject.SetActive(false); });
        }
    }
}