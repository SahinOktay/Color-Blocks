using UnityEngine;
using DG.Tweening;

namespace FOMO
{
    public class Block : Movable
    {
        [SerializeField] private MeshRenderer blockRenderer;
        [SerializeField] private BlockTextureMap blockTextureMap;

        private BlockColor _color;

        public void Initialize(BlockColor color, Dimention dimension)
        {
            base.Initialize(dimension);
            _color = color;
            blockRenderer.material.SetTexture("_MainTex", blockTextureMap.GetTexture(Length, _color, dimension));
        }
    }
}
