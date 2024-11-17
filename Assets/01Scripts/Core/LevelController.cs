using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FOMO
{
    public class LevelController : MonoBehaviour
    {
        public struct ExitKey
        {
            public int direction;
            public Vector2Int coordinates;
        }

        [SerializeField] private Camera mainCamera;
        [SerializeField] 
        private GameObject exitPrefab, 
            gridTile, 
            singleBlock, 
            doubleBlock;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private UIController uiController;
        
        private bool _canMakeAMove = true;
        private readonly Dictionary<ExitKey, Exit> _exits = new();
        private float _movePixelThreshold;
        private GridCell[][] _grid;
        private readonly HashSet<Block> _blocks = new();
        private int _currentLevel = 4, _remainingMoves;
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
                Resources.Load<TextAsset>("Levels/Level" + _currentLevel).text
            );

            _remainingMoves = levelData.MoveLimit;

            uiController.Initialize(_currentLevel, _remainingMoves);

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
                spawnedExit.Initialize(new BlockColor[] { (BlockColor)levelData.ExitInfo[i].Colors }, levelData.ExitInfo[i].Direction);
                _exits.Add(
                    new ExitKey()
                    {
                        coordinates = new Vector2Int(levelData.ExitInfo[i].Col, levelData.ExitInfo[i].Row),
                        direction = levelData.ExitInfo[i].Direction
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

                spawnedBlock.Destroyed += OnBlockDestroyed;

                spawnedBlock.Initialize(
                    (BlockColor)levelData.MovableInfo[i].Colors,
                    dimention
                );
                spawnedBlock.coordinates = blockCoordinates;
                _blocks.Add(spawnedBlock);

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

        private void Win()
        {
            Debug.Log("Win");
        }

        private void Lose()
        {
            Debug.Log("Lose");
        }

        private void OnBlockDestroyed(Block block)
        {
            block.Destroyed -= OnBlockDestroyed;
            _blocks.Remove(block);
            EmptyCells(block);

            _canMakeAMove = true;

            if (_blocks.Count == 0)
            {
                Win();
            }
            else if (_remainingMoves == 0)
            {
                Lose();
            }
        }

        private void OnTouch(InputAction.CallbackContext context)
        {
            if (!_canMakeAMove) return;

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

            int directionNumber;
            if (movementDimention == Dimention.Horizontal)
                directionNumber = differenceVector.x > 0 ? 1 : 3;
            else
                directionNumber = differenceVector.y > 0 ? 0 : 2;

            if (_movable.Dimention != movementDimention) 
            {
                _touchMoveAction.performed -= OnTouchMove;
                _touchStartAction.performed += OnTouch;
                return; 
            }

            Vector2Int directionVector = Constants.Calculators.GetDirectionVector(directionNumber);

            // This variable is used to calculate which cells to check
            // If direction is positive we need to consider length of the block
            // If it is negative length is not included in the calculation because pivots of the blocks are at the start of the block
            bool isPositiveDirection = directionNumber is 1 or 2;

            Vector2Int currentCell = _movable.GetLastCellAtDirection(directionNumber);

            Vector2Int nextCellToCheck = _movable.GetLastCellAtDirection(directionNumber) + directionVector;

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
                direction = directionNumber,
                coordinates = nextCellToCheck - directionVector
            };
            bool isExiting = _exits.ContainsKey(exitKey) && _movable is IGrindable;

            if (movementCount == 0 && !isExiting) 
            {
                _touchMoveAction.performed -= OnTouchMove;
                _touchStartAction.performed += OnTouch;
                PushOtherBlocks(_movable, directionNumber, 1);

                return; 
            }

            Vector2Int targetCell = _movable.coordinates + directionVector * movementCount;

            EmptyCells(_movable);

            _movable.coordinates = targetCell;
            if (!isExiting)
            {
                _canMakeAMove = false;
                _movable.MovementEnded += PushOtherBlocks;
                _movable.Move(_grid[targetCell.y][targetCell.x].tile.transform.position, directionNumber);

                FillCells();
            }
            else
            {
                _canMakeAMove = false;
                IGrindable grindable = _movable as IGrindable;
                grindable.MoveAndExit(_grid[targetCell.y][targetCell.x].tile.transform.position, isPositiveDirection, directionNumber);
                if (!_exits[exitKey].WaitForGrindable(grindable))
                {
                    _movable.MovementEnded += EnableMovement;
                }
            }

            FillCells();

            _touchMoveAction.performed -= OnTouchMove;

            if (--_remainingMoves > 0) _touchStartAction.performed += OnTouch;
            uiController.SetMoveCount(_remainingMoves);
        }

        private void EmptyCells(Movable movable)
        {
            for (int i = 0; i < movable.Length; i++)
            {
                _grid[movable.coordinates.y + (movable.Dimention == Dimention.Vertical ? i : 0)]
                    [movable.coordinates.x + ( movable.Dimention == Dimention.Horizontal ? i : 0)]
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

        private void EnableMovement(Movable movable, int directionNumber, int cellCount)
        {
            movable.MovementEnded -= PushOtherBlocks;
            _canMakeAMove = true;
        }

        private void PushOtherBlocks(Movable movable, int directionNumber, int cellCount)
        {
            movable.MovementEnded -= PushOtherBlocks;
            _canMakeAMove = true;

            Vector2Int directionVector = Constants.Calculators.GetDirectionVector(directionNumber);

            int stepCount = 0;
            Vector2Int bumpedCell = movable.GetLastCellAtDirection(directionNumber) + directionVector;

            // Handle blocks bumping each other
            while (
                bumpedCell.x >= 0 &&
                bumpedCell.x < _gridSize.x &&
                bumpedCell.y >= 0 &&
                bumpedCell.y < _gridSize.y
                && stepCount < cellCount
            )
            {
                if (_grid[bumpedCell.y][bumpedCell.x].occupyingElement is Movable nextMovable)
                {
                    nextMovable.GetBumped(directionNumber, Mathf.Lerp(1, .5f, stepCount / (float)cellCount));
                }
                stepCount++;
                bumpedCell += directionVector;
            }

            if (stepCount > 0)
            {
                movable.GetBumped(directionNumber, 1f);
            }

            if (_remainingMoves == 0) Lose();
        }
    }
}    
