using PoolSystem;
using System;
using UnityEngine;

namespace FOMO
{
    public class GridTile : MonoBehaviour, IPoolable
    {
        [NonSerialized] public GridElement occupyingElement;

        public void Reset()
        {
            transform.eulerAngles = Vector3.right * 90;
            occupyingElement = null;
        }
    }
}