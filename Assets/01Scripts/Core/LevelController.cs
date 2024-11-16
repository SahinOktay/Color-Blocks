using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace FOMO
{
    public class LevelController : MonoBehaviour
    {
        public struct ExitKey
        {
            public Dimention dimention;
            public Vector2Int coordinates;
        }

        [SerializeField] private Camera mainCamera;
        [SerializeField] 
        private GameObject exitPrefab, 
            gridTile, 
            singleBlock, 
            doubleBlock;
        [SerializeField] private PlayerInput playerInput;

        private Dictionary<ExitKey, Exit> _exits = new();
        private float _movePixelThreshold;
        private GridCell[][] _grid;
        private InputAction _touchStartAction, _touchMoveAction;
        private Movable _movable;
        private Vector2 _touchStartPos;
        private Vector2Int _gridSize;

        [ContextMenu("Generate Level")]
        public void Start()
        {
            _movePixelThreshold = Screen.width * .1f;
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

            _gridSize = new Vector2Int(levelData.ColCount, levelData.RowCount);
            List<List<GridCell>> gridList = new List<List<GridCell>>();

            Vector3 firstTilePosition = new Vector3(
                - _gridSize.x * .5f + .5f,
                0,
                _gridSize.y * .5f - .5f
            );

            GameObject spawnedTile;
            for (int y = 0; y < _gridSize.y; y++)
            {
                gridList.Add(new List<GridCell>());
                for (int x = 0; x < _gridSize.x; x++)
                {
                    spawnedTile = Instantiate(
                        gridTile, 
                        firstTilePosition + 
                            Vector3.right * (x * Constants.Numbers.CELL_SIZE) +
                            Vector3.back * (y * Constants.Numbers.CELL_SIZE), 
                        gridTile.transform.rotation,
                        tileParent
                    );
                    gridList[y].Add(new GridCell() { tile = spawnedTile });
                }
            }

            _grid = gridList.Select(item => item.ToArray()).ToArray();

            Exit spawnedExit;
            for (int i = 0; i < levelData.ExitInfo.Length; i++)
            {
                spawnedExit = Instantiate(
                    exitPrefab,
                    firstTilePosition +
                        Vector3.right * (levelData.ExitInfo[i].Col * Constants.Numbers.CELL_SIZE) +
                        Vector3.back * (levelData.ExitInfo[i].Row * Constants.Numbers.CELL_SIZE),
                    Quaternion.Euler(0, 90 * levelData.ExitInfo[i].Direction, 0),
                    exitParent
                ).GetComponent<Exit>();
                spawnedExit.Initialize(new BlockColor[] { (BlockColor)levelData.ExitInfo[i].Colors });
                _exits.Add(
                    new ExitKey()
                    {
                        coordinates = new Vector2Int(levelData.ExitInfo[i].Col, levelData.ExitInfo[i].Row),
                        dimention = Constants.Arrays.HORI_DIRECTIONS.Contains(levelData.ExitInfo[i].Direction) ? Dimention.Horizontal : Dimention.Vertical
                    }
                    ,
                    spawnedExit
                );
            }

            Block spawnedBlock;
            Vector2Int blockCoordinates;
            for (int i = 0; i < levelData.MovableInfo.Length; i++)
            {
                Dimention dimention = Constants.Arrays.HORI_DIRECTIONS.Contains(levelData.MovableInfo[i].Direction[0]) ? 
                    Dimention.Horizontal : Dimention.Vertical;

                blockCoordinates = new Vector2Int(levelData.MovableInfo[i].Col, levelData.MovableInfo[i].Row);

                spawnedBlock = Instantiate(
                    levelData.MovableInfo[i].Length == 1 ? singleBlock : doubleBlock,
                    firstTilePosition +
                        Vector3.right * (blockCoordinates.x * Constants.Numbers.CELL_SIZE) +
                        Vector3.back * (blockCoordinates.y * Constants.Numbers.CELL_SIZE),
                    Quaternion.Euler(
                        0,
                        dimention == Dimention.Horizontal ? 0 : 90,
                        0
                    ),
                    movableParent
                ).GetComponent<Block>();

                spawnedBlock.Initialize(
                    (BlockColor)levelData.MovableInfo[i].Colors,
                    dimention
                );
                spawnedBlock.coordinates = blockCoordinates;

                for (int j = 0; j < levelData.MovableInfo[i].Length; j++)
                {
                    if (spawnedBlock.Dimention == Dimention.Horizontal)
                        _grid[blockCoordinates.y][blockCoordinates.x + j].occupyingElement = spawnedBlock;
                    else
                        _grid[blockCoordinates.y + j][blockCoordinates.x].occupyingElement = spawnedBlock;
                }
            }

            // Handle camera view
            Vector3 camCenterLeftRay = mainCamera.ScreenPointToRay(Vector3.zero).direction;
            camCenterLeftRay = new Vector3(camCenterLeftRay.x, camCenterLeftRay.y, 0);

            // +2 for exits and +1 for margin
            float gridHalfWidth = (_gridSize.x + 2 * Constants.Numbers.CELL_SIZE + 1) * .5f;

            // Handle camera position for covering all grid vertically
            mainCamera.transform.position = new Vector3(-gridHalfWidth, 0, 0) +
                (gridHalfWidth / (camCenterLeftRay.x)) * camCenterLeftRay;

            // Handle camera position for looking at the center of the grid
            mainCamera.transform.position = mainCamera.transform.forward * (mainCamera.transform.position.y / mainCamera.transform.forward.y);

            _touchStartAction = playerInput.currentActionMap.FindAction(Constants.Actions.TOUCH_START);
            _touchMoveAction = playerInput.currentActionMap.FindAction(Constants.Actions.TOUCH_MOVE);
            
            _touchStartAction.performed += OnTouch;
        }

        private void OnTouch(InputAction.CallbackContext context)
        {
            if (
                Physics.Raycast(
                    Camera.main.ScreenPointToRay(context.ReadValue<Vector2>()),
                    out RaycastHit hit,
                    Mathf.Infinity,
                    1 << LayerMask.NameToLayer(Constants.Layers.INTERACTABLE)
                )
            )
            {
                _movable = hit.collider.GetComponent<Movable>();

                _touchStartAction.performed -= OnTouch;
                _touchStartPos = context.ReadValue<Vector2>();

                _touchMoveAction.performed += OnTouchMove;
            }
        }

        private void OnTouchMove(InputAction.CallbackContext context)
        {
            Vector2 newTouchPos = context.ReadValue<Vector2>();
            Vector2 differenceVector = newTouchPos - _touchStartPos;

            if (differenceVector.magnitude < _movePixelThreshold) return;

            // if absolute x value is greater than y, player made a horizontal move else its a vertical move
            Dimention movementDimention = Mathf.Abs(differenceVector.x) > Mathf.Abs(differenceVector.y) ? 
                Dimention.Horizontal : Dimention.Vertical;

            if (_movable.Dimention != movementDimention) 
            {
                _touchMoveAction.performed -= OnTouchMove;
                _touchStartAction.performed += OnTouch;
                return; 
            }

            Vector2Int directionVector = new Vector2Int(
                movementDimention == Dimention.Horizontal ? (differenceVector.x < 0 ? -1 : 1) : 0,
                movementDimention == Dimention.Vertical ? (differenceVector.y < 0 ? 1 : -1) : 0
            );

            // This variable is used to calculate which cells to check
            // If direction is positive we need to consider length of the block
            // If it is negative length is not included in the calculation because pivots of the blocks are at the start of the block
            bool isPositiveDirection = directionVector.x > 0 || directionVector.y > 0;

            Vector2Int currentCell = _movable.coordinates + 
                (isPositiveDirection ? (directionVector * (_movable.Length - 1)) : Vector2Int.zero);
            Vector2Int nextCellToCheck = _movable.coordinates + 
                (isPositiveDirection ? (directionVector * _movable.Length) : directionVector);

            int movementCount = 0;
            // Check for available cells
            while (
                (
                    nextCellToCheck.x >= 0 &&
                    nextCellToCheck.x < _gridSize.x &&
                    nextCellToCheck.y >= 0 &&
                    nextCellToCheck.y < _gridSize.y
                ) ?
                _grid[nextCellToCheck.y][nextCellToCheck.x].occupyingElement == null : false
            )
            {
                movementCount++;
                nextCellToCheck += directionVector;
            }

            ExitKey exitKey = new ExitKey() {
                dimention = movementDimention,
                coordinates = nextCellToCheck - directionVector
            };
            bool isExiting = _exits.ContainsKey(exitKey);

            if (movementCount == 0 && !isExiting) 
            {
                _touchMoveAction.performed -= OnTouchMove;
                _touchStartAction.performed += OnTouch;
                return; 
            }

            Vector2Int targetCell = _movable.coordinates + directionVector * movementCount;

            EmptyCells();

            if (!isExiting)
            {
                _movable.Move(_grid[targetCell.y][targetCell.x].tile.transform.position);
                _movable.coordinates = targetCell;

                FillCells();
            }
            else
            {
                _movable.MoveAndExit(_grid[targetCell.y][targetCell.x].tile.transform.position, _exits[exitKey]);
            }

            _touchMoveAction.performed -= OnTouchMove;
            _touchStartAction.performed += OnTouch;
        }

        private void EmptyCells()
        {
            for (int i = 0; i < _movable.Length; i++)
            {
                _grid[_movable.coordinates.y + (_movable.Dimention == Dimention.Vertical ? i : 0)]
                    [_movable.coordinates.x + (_movable.Dimention == Dimention.Horizontal ? i : 0)]
                    .occupyingElement = null;
            }
        }

        private void FillCells()
        {
            for (int i = 0; i < _movable.Length; i++)
            {
                _grid[_movable.coordinates.y + (_movable.Dimention == Dimention.Vertical ? i : 0)]
                    [_movable.coordinates.x + (_movable.Dimention == Dimention.Horizontal ? i : 0)]
                    .occupyingElement = _movable;
            }
        }
    }
}    
