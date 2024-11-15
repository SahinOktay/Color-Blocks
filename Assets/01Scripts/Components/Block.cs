using UnityEngine;

namespace FOMO
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private int length;
        [SerializeField] private MeshRenderer blockRenderer;
        [SerializeField] private BlockTextureMap blockTextureMap;

        private BlockColor _color;

        public void Initialize(BlockColor color, Dimention dimension)
        {
            _color = color;
            blockRenderer.material.SetTexture("_MainTex", blockTextureMap.GetTexture(length, _color, dimension));
        }
    }
}
