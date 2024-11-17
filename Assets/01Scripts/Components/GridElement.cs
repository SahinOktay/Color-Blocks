using System;
using UnityEngine;

namespace FOMO
{
    public abstract class GridElement : MonoBehaviour
    {
        [NonSerialized] public Vector2Int coordinates;
    }
}