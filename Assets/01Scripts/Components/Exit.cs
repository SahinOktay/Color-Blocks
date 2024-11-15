using FOMO;
using UnityEngine;

public class Exit : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] gates;
    [SerializeField] private BlockTextureMap blockTextureMap;

    private BlockColor[] _colors;

    public void Initialize(BlockColor[] colors)
    {
        _colors = colors;
        for (int i = 0; i < colors.Length; i++)
        {
            gates[i].material.SetColor("_Color", blockTextureMap.GetGateColor(colors[i]));
        }

        Color lastColor = blockTextureMap.GetGateColor(colors[^1]);
        for (int i = colors.Length; i < gates.Length; i++)
        {
            gates[i].material.SetColor("_Color", lastColor);
        }
    }
}
