using System.Linq;
using UnityEngine;

namespace FOMO
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] 
        private GameObject exitPrefab, 
            gridTile, 
            singleBlock, 
            doubleBlock;

        private Vector2Int gridSize;

        [ContextMenu("Generate Level")]
        public void Start()
        {
            GameObject levelParent = new GameObject("Level Parent");
            Transform tileParent = new GameObject("Tile Parent").transform;
            Transform exitParent = new GameObject("Exit Parent").transform;
            Transform movableParent = new GameObject("Movable Parent").transform;

            tileParent.transform.SetParent(levelParent.transform);
            exitParent.transform.SetParent(levelParent.transform);
            movableParent.transform.SetParent(levelParent.transform);

            LevelData levelData = JsonUtility.FromJson<LevelData>(
                Resources.Load<TextAsset>("Levels/Level4").text
            );

            gridSize = new Vector2Int(levelData.ColCount, levelData.RowCount);

            Vector3 firstTilePosition = new Vector3(
                - gridSize.x * .5f + .5f,
                0,
                gridSize.y * .5f - .5f
            );

            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    Instantiate(
                        gridTile, 
                        firstTilePosition + 
                            Vector3.right * (x * Constants.Numbers.CELL_SIZE) +
                            Vector3.back * (y * Constants.Numbers.CELL_SIZE), 
                        gridTile.transform.rotation,
                        tileParent
                    );
                }
            }

            for (int i = 0; i < levelData.ExitInfo.Length; i++)
            {
                Instantiate(
                    exitPrefab,
                    firstTilePosition +
                        Vector3.right * (levelData.ExitInfo[i].Col * Constants.Numbers.CELL_SIZE) +
                        Vector3.back * (levelData.ExitInfo[i].Row * Constants.Numbers.CELL_SIZE),
                    Quaternion.Euler(0, 90 * levelData.ExitInfo[i].Direction, 0),
                    exitParent
                ).GetComponent<Exit>().Initialize(new BlockColor[] { (BlockColor)levelData.ExitInfo[i].Colors });
            }

            for (int i = 0; i < levelData.MovableInfo.Length; i++)
            {
                Dimention dimention = Constants.Arrays.HORI_DIRECTIONS.Contains(levelData.MovableInfo[i].Direction[0]) ? 
                    Dimention.Horizontal : Dimention.Vertical;
                Instantiate(
                    levelData.MovableInfo[i].Length == 1 ? singleBlock : doubleBlock,
                    firstTilePosition +
                        Vector3.right * (levelData.MovableInfo[i].Col * Constants.Numbers.CELL_SIZE) +
                        Vector3.back * (levelData.MovableInfo[i].Row * Constants.Numbers.CELL_SIZE),
                    Quaternion.Euler(
                        0, 
                        dimention == Dimention.Horizontal ? 0 : 90, 
                        0
                    ),
                    movableParent
                ).GetComponent<Block>().Initialize(
                    (BlockColor)levelData.MovableInfo[i].Colors,
                    dimention
                );
            }

            // Handle camera view
            Vector3 camCenterLeftRay = mainCamera.ScreenPointToRay(Vector3.zero).direction;
            camCenterLeftRay = new Vector3(camCenterLeftRay.x, camCenterLeftRay.y, 0);

            // +2 for exits and +1 for margin
            float gridHalfWidth = (gridSize.x + 2 * Constants.Numbers.CELL_SIZE + 1) * .5f;

            // Handle camera position for covering all grid vertically
            mainCamera.transform.position = new Vector3(-gridHalfWidth, 0, 0) +
                (gridHalfWidth / (camCenterLeftRay.x)) * camCenterLeftRay;

            // Handle camera position for looking at the center of the grid
            mainCamera.transform.position = mainCamera.transform.forward * (mainCamera.transform.position.y / mainCamera.transform.forward.y);
        }
    }
}    
