using PoolSystem;
using System.Collections.Generic;
using Unity.Android.Gradle;
using UnityEngine;

namespace FOMO
{
    public class PoolManager : MonoBehaviour
    {
        [SerializeField] private Block[] blocks;
        [SerializeField] private GameObject exitPrefab, tilePrefab;

        private Pool<Exit> _exitPool;
        private Dictionary<int, Pool<Block>> _blockPools = new Dictionary<int, Pool<Block>>();
        private Pool<GridTile> _tilePool;


        public void Initialize()
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                _blockPools.Add(
                    blocks[i].Length,
                    new Pool<Block>(
                        new PrefabFactory<Block>(blocks[i].gameObject),
                        15
                    )
                );
            }

            _exitPool = new Pool<Exit>(
                new PrefabFactory<Exit>(exitPrefab),
                15
            );
            _tilePool = new Pool<GridTile>(
                new PrefabFactory<GridTile>(tilePrefab),
                30
            );
        }

        public Block GetBlock(int length) => _blockPools[length].GetItem();
        public void RecycleBlock(Block block) 
        {
            block.transform.SetParent(null);
            _blockPools[block.Length].Recycle(block); 
        }

        public Exit GetExit() => _exitPool.GetItem();
        public void RecycleExit(Exit exit) 
        { 
            exit.transform.SetParent(null);
            _exitPool.Recycle(exit); 
        }

        public GridTile GetGridTile() => _tilePool.GetItem();
        public void RecycleGridTile(GridTile tile)
        {
            tile.transform.SetParent(null);
            _tilePool.Recycle(tile);
        }
    }
}