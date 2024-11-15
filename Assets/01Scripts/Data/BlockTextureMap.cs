using System;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace FOMO
{
    [CreateAssetMenu(fileName = "BlockTextureMap", menuName = "ScriptableObjects/BlockTextureMap", order = 1)]
    public class BlockTextureMap : ScriptableObject
    {
        public TextureConfig[] textureConfigs;

        public UnityEngine.Color GetGateColor(BlockColor blockColor) => textureConfigs.First(item => item.blockColor == blockColor).gateColor;

        public Texture GetTexture(int length, BlockColor color, Dimention direction) =>
            textureConfigs.First(item => item.blockColor == color).GetTexture(length, direction);
    }

    [Serializable]
    public class TextureConfig
    {
        public BlockColor blockColor;
        public UnityEngine.Color gateColor;
        public Texture horizontalShort, horizontalLong;
        public Texture verticalShort, verticalLong;

        public Texture GetTexture(int length, Dimention direction) =>
            length == 1 ? (direction == Dimention.Horizontal ? horizontalShort : verticalShort) :
                (direction == Dimention.Horizontal ? horizontalLong : verticalLong);
    }
}