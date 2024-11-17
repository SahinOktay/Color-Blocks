using DG.Tweening;
using FOMO;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Exit : MonoBehaviour
{
    [Serializable] 
    public class ColoredElements
    {
        public MeshRenderer[] meshRenderers;
    }

    [SerializeField] private BlockTextureMap blockTextureMap;
    [SerializeField] private ColoredElements[] coloredElements;
    [SerializeField] private GameObject blockingQuad;
    [SerializeField] private ParticleSystem grindParticle;
    [SerializeField] private Transform gateParent, grindingPart;

    private BlockColor[] _colors;
    private static float _grindDuration = .75f;

    public int Direction {  get; private set; }

    public void Initialize(BlockColor[] colors, int direction)
    {
        Direction = direction;
        _colors = colors;

        for (int i = 0; i < colors.Length; i++)
        {
            for (int j = 0; j < coloredElements[i].meshRenderers.Length; j++)
                coloredElements[i].meshRenderers[j].material.SetColor("_Color", blockTextureMap.GetGateColor(colors[i]));
        }

        Color lastColor = blockTextureMap.GetGateColor(colors[^1]);
        for (int i = colors.Length; i < coloredElements.Length; i++)
        {
            for (int j = 0; j < coloredElements[i].meshRenderers.Length; j++)
                coloredElements[i].meshRenderers[j].material.SetColor("_Color", lastColor);
        }
    }

    public bool WaitForGrindable(IGrindable grindable)
    {
        grindable.ReachedGrinder += OnBlockReach;

        return _colors.Contains(grindable.Color);
    }

    public void OnBlockReach(IGrindable grindable, bool isPositiveDirection, int length)
    {
        grindable.ReachedGrinder -= OnBlockReach;

        if (!_colors.Contains(grindable.Color))
        {
            if (grindable is Movable movable) movable.GetBumped(Direction, 1f);
            gateParent.DORotate(new Vector3(-30, 0, 0), .15f, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                gateParent.DORotate(new Vector3(30, 0, 0), .15f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutCubic);
            });
            return;
        }

        grindable.GetGrinded(
            transform.forward *
            (Constants.Numbers.CELL_SIZE * (length + .5f)),
            _grindDuration
        );
        blockingQuad.SetActive(true);

        gateParent.localPosition += Vector3.down;
        grindingPart.DOMoveY(1, .1f);

        grindingPart.DOMoveY(0, .25f).SetDelay(_grindDuration);
        gateParent.DOMoveY(0, .25f).SetDelay(_grindDuration).OnComplete(() => blockingQuad.SetActive(false));

        grindParticle.Play();
        ParticleSystemRenderer particleRenderer = grindParticle.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material.SetColor("_Color", blockTextureMap.GetGateColor(grindable.Color));
        particleRenderer.material.SetColor("_EmissionColor", blockTextureMap.GetGateColor(grindable.Color) * .3f);
    }
}
