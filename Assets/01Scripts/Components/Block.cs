using UnityEngine;
using DG.Tweening;
using System;
using PoolSystem;

namespace FOMO
{
    public class Block : Movable, IGrindable, IPoolable
    {
        [SerializeField] private AnimationCurve bounceBackEase;
        [SerializeField] private BlockTextureMap blockTextureMap;
        [SerializeField] private Renderer blockRenderer;
        [SerializeField] private Transform bumpBone;

        public event GrindEvent ReachedGrinder;
        public Action<Block> Destroyed;

        public BlockColor Color { get; private set; }

        public void Initialize(BlockColor color, Dimention dimension)
        {
            base.Initialize(dimension);
            Color = color;
            blockRenderer.material.SetTexture("_MainTex", blockTextureMap.GetTexture(Length, Color, dimension));
        }

        public override void GetBumped(int direction, float strength = .5f)
        {
            bumpBone.DORotate(
                (Quaternion.Euler(0, 90 * direction, 0)) * new Vector3(30 * strength, 0, 0),
                .15f,
                RotateMode.WorldAxisAdd
            ).SetEase(Ease.OutCubic).OnComplete(() => { 
                bumpBone.transform.DORotate(transform.eulerAngles, .3f).SetEase(bounceBackEase); 
            });
        }

        public void MoveAndExit(Vector3 pos, bool isPositiveDirection, int directionNumber)
        {
            Move(
                pos, 
                directionNumber , 
                () => ReachedGrinder?.Invoke(this, isPositiveDirection, Length)
            );
        }

        public void GetGrinded(Vector3 pos, float duration)
        {
            transform.DOMove(transform.position + pos, duration);
            transform.DOShakeRotation(duration, 5, 20, fadeOut: false).OnComplete(() =>
            {
                Destroyed?.Invoke(this);
            });
        }

        public void Reset()
        {
            bumpBone.transform.localEulerAngles = Vector3.zero;
        }
    }
}
