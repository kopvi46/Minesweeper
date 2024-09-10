using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyGrid : MonoBehaviour
{
    public event EventHandler<OnGridCellOpenEventArgs> OnGridCellOpen;
    public class OnGridCellOpenEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private int _cellSize;
    [SerializeField] private int _fontSize;
    [SerializeField] private int _maxMineAmount;
    [SerializeField] private Transform _gridCellPrefab;
    [SerializeField] private TextMeshProUGUI _mineLeftAmountVisual;
    [SerializeField] private Button _restartButton;
    [SerializeField] private TextMeshProUGUI _gameTimerVisual;

    private GridCell[,] _gridCellArray;
    private int _mineLeftAmount;
    private int _closedCellCount;
    private bool _isGameOver = false;
    private ColorBlock _colorBlock;
    private float _gameTimer;

    private void Start()
    {
        transform.position -= new Vector3(_width / 2, _height / 2, 0);
        _colorBlock = _restartButton.colors;

        CreateNewGrid();

        OnGridCellOpen += MyGrid_OnGridCellOpen;
    }

    private void MyGrid_OnGridCellOpen(object sender, OnGridCellOpenEventArgs e)
    {
        //Lose game
        if (GetGridCell(e.x, e.y).isMined)
        {
            _isGameOver = true;
            _colorBlock.normalColor = Color.red;
            _restartButton.colors = _colorBlock;
            GetGridCell(e.x, e.y).background.color = Color.red;

            foreach (GridCell gridCell in _gridCellArray)
            {
                if (gridCell.isMined && !gridCell.isOpen && !gridCell.isMarked)
                {
                    gridCell.overlay.gameObject.SetActive(false);
                }
            }

            SoundManager.Instance.soundsSource.PlayOneShot(SoundManager.Instance.gameOver);
            Debug.Log("You have lost!");
        }

        //Win game
        if (_closedCellCount == _maxMineAmount)
        {
            _isGameOver = true;
            _colorBlock.normalColor = Color.green;
            _restartButton.colors = _colorBlock;

            SoundManager.Instance.soundsSource.PlayOneShot(SoundManager.Instance.gameWin);
            Debug.Log("You have won!");
        }
    }

    private void Update()
    {
        //Open GridCell
        if (Input.GetMouseButtonDown(0) && !_isGameOver)
        {
            Vector3 mouseWorldPosition = MyUtils.GetMouse2DWorldPosition();
            GetXY(mouseWorldPosition, out int x, out int y);

            if (GetGridCell(x, y) != null && !GetGridCell(x, y).isMarked && !GetGridCell(x, y).isOpen)
            {
                SoundManager.Instance.soundsSource.PlayOneShot(SoundManager.Instance.openCell);
            }

            OpenGridCell(x, y);
        }

        //Mark GridCell
        if (Input.GetMouseButtonDown(1) && !_isGameOver)
        {
            Vector3 mouseWorldPosition = MyUtils.GetMouse2DWorldPosition();
            GetXY(mouseWorldPosition, out int x, out int y);

            GridCell currentGridCell = GetGridCell(x, y);

            if (currentGridCell != null && !currentGridCell.isOpen)
            {
                currentGridCell.isMarked = !currentGridCell.isMarked;

                foreach (Transform child in currentGridCell.mark.transform)
                {
                    child.gameObject.SetActive(currentGridCell.isMarked);
                }

                if (currentGridCell.isMarked)
                {
                    _mineLeftAmount--;
                } else
                {
                    _mineLeftAmount++;
                }

                _mineLeftAmountVisual.text = _mineLeftAmount.ToString();

                SoundManager.Instance.soundsSource.PlayOneShot(SoundManager.Instance.markCell);
            }
        }

        //Update game timer
        if (!_isGameOver && _closedCellCount < (_width * _height))
        {
            _gameTimer += Time.deltaTime;

            _gameTimerVisual.text = ((int)_gameTimer).ToString();
        }
    }

    private void CreateNewGrid()
    {
        _isGameOver = false;
        _colorBlock.normalColor = Color.yellow;
        _restartButton.colors = _colorBlock;
        _gameTimer = 0;
        _gameTimerVisual.text = "0";
        _closedCellCount = _width * _height;

        _gridCellArray = new GridCell[_width, _height];

        for (int x = 0; x < _gridCellArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridCellArray.GetLength(1); y++)
            {
                Transform gridCellTransform = Instantiate(_gridCellPrefab);

                gridCellTransform.position = GetWorldPosition(x, y) + new Vector3(_cellSize * .5f, _cellSize * .5f, 0);
                gridCellTransform.SetParent(transform);

                GridCell gridCell = gridCellTransform.GetComponent<GridCell>();

                if (gridCell != null)
                {
                    _gridCellArray[x, y] = gridCell;
                    gridCell.x = x;
                    gridCell.y = y;
                }
            }
        }

        //Mining random GridCells
        for (int i = 0; i < _maxMineAmount; i++)
        {
            GetRandomUnminedGridCell(out int x, out int y);

            _gridCellArray[x, y].isMined = true;
        }

        //Count mines around unmined GridCell
        foreach (GridCell gridCell in _gridCellArray)
        {
            if (!gridCell.isMined)
            {
                gridCell.mineAround = GetMineAroundAmount(gridCell.x, gridCell.y);
            }
        }

        //Update mines count
        _mineLeftAmount = _maxMineAmount;
        _mineLeftAmountVisual.text = _mineLeftAmount.ToString();

        SoundManager.Instance.soundsSource.PlayOneShot(SoundManager.Instance.gameStart);
    }

    public void RestartGame()
    {
        _gridCellArray = null;
        
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        CreateNewGrid();
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y, 0) + transform.position;
    }

    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - transform.position).x / _cellSize);
        y = Mathf.FloorToInt((worldPosition - transform.position).y / _cellSize);
    }

    private GridCell GetGridCell(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
        {
            return _gridCellArray[x, y];
        } else
        {
            return default;
        }
    }

    private void GetRandomUnminedGridCell(out int x, out int y)
    {
        System.Random random = new System.Random();

        x = random.Next(_gridCellArray.GetLength(0));
        y = random.Next(_gridCellArray.GetLength(1));

        if (_gridCellArray[x, y].isMined)
        {
            GetRandomUnminedGridCell(out x, out y);
        }
    }

    private int GetMineAroundAmount(int x, int y)
    {
        int amount = 0;

        foreach (GridCell gridCell in GetNeighboursGridCellList(x, y))
        {
            if (gridCell.isMined) amount++;
        }

        return amount;
    }

    private List<GridCell> GetNeighboursGridCellList(int x, int y)
    {
        List<GridCell> gridCellList = new List<GridCell>();

        //Get all neighbours GridCell
        if (GetGridCell(x, y) != null)
        {
            //Top left
            if (GetGridCell(x - 1, y + 1) != null) gridCellList.Add(GetGridCell(x - 1, y + 1));
            //Top
            if (GetGridCell(x, y + 1) != null) gridCellList.Add(GetGridCell(x, y + 1));
            //Top right
            if (GetGridCell(x + 1, y + 1) != null) gridCellList.Add(GetGridCell(x + 1, y + 1));
            //Right
            if (GetGridCell(x + 1, y) != null) gridCellList.Add(GetGridCell(x + 1, y));
            //Bottom right
            if (GetGridCell(x + 1, y - 1) != null) gridCellList.Add(GetGridCell(x + 1, y - 1));
            //Bottom
            if (GetGridCell(x, y - 1) != null) gridCellList.Add(GetGridCell(x, y - 1));
            //Bottom left
            if (GetGridCell(x - 1, y - 1) != null) gridCellList.Add(GetGridCell(x - 1, y - 1));
            //Left
            if (GetGridCell(x - 1, y) != null) gridCellList.Add(GetGridCell(x - 1, y));
        }

        return gridCellList;
    }

    private void OpenGridCell(int x, int y)
    {
        GridCell currentGridCell = GetGridCell(x, y);

        if (currentGridCell != null && !currentGridCell.isMarked && !currentGridCell.isOpen)
        {
            _closedCellCount--;

            currentGridCell.isOpen = true;

            currentGridCell.overlay.gameObject.SetActive(false);

            OnGridCellOpen?.Invoke(this, new OnGridCellOpenEventArgs { x = x, y = y });

            //Open all neighbour unmined GridCells
            if (!currentGridCell.isMined && currentGridCell.mineAround == 0)
            {
                foreach (GridCell neighbour in GetNeighboursGridCellList(x, y))
                {
                    if (neighbour != null && !neighbour.isOpen)
                    {
                        OpenGridCell(neighbour.x, neighbour.y);
                    }
                }
            }
        }
    }
}
