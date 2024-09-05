using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class MyGrid : MonoBehaviour
{
    public event EventHandler<OnGridOpenEventArgs> OnGridOpen;
    public class OnGridOpenEventArgs : EventArgs
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

    private GridCell[,] _gridArray;
    private int _mineLeftAmount;
    private int _closedCellCount;
    private bool _isGameOver = false;
    private ColorBlock colorBlock;

    private void Start()
    {
        transform.position -= new Vector3(_width / 2, _height / 2, 0);

        colorBlock = _restartButton.colors;

        StartNewGame();

        OnGridOpen += MyGrid_OnGridOpen;
    }

    private void MyGrid_OnGridOpen(object sender, OnGridOpenEventArgs e)
    {
        if (GetGridCell(e.x, e.y).isMined)
        {
            Debug.Log("You have lost!");

            _isGameOver = true;

            colorBlock.normalColor = Color.red;
            _restartButton.colors = colorBlock;
        }

        if (_closedCellCount == _maxMineAmount)
        {
            Debug.Log("You have won!");
            
            _isGameOver = true;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_isGameOver)
        {
            Vector3 mouseWorldPosition = MyUtils.GetMouse2DWorldPosition();
            GetXY(mouseWorldPosition, out int x, out int y);

            OpenGridCell(x, y);
        }

        if (Input.GetMouseButtonDown(1) && !_isGameOver)
        {
            Vector3 mouseWorldPosition = MyUtils.GetMouse2DWorldPosition();
            GetXY(mouseWorldPosition, out int x, out int y);

            GridCell currentGridCell = GetGridCell(x, y);

            if (currentGridCell != null && !currentGridCell.isOpen)
            {
                currentGridCell.isMarked = !currentGridCell.isMarked;

                currentGridCell.mark.gameObject.SetActive(currentGridCell.isMarked);

                if (currentGridCell.isMarked)
                {
                    _mineLeftAmount--;
                } else
                {
                    _mineLeftAmount++;
                }

                _mineLeftAmountVisual.text = _mineLeftAmount.ToString();
            }
        }
    }

    private void StartNewGame()
    {
        colorBlock.normalColor = Color.green;
        _restartButton.colors = colorBlock;
        _isGameOver = false;

        _closedCellCount = _width * _height;

        _gridArray = new GridCell[_width, _height];

        for (int x = 0; x < _gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridArray.GetLength(1); y++)
            {
                Transform cellTransform = Instantiate(_gridCellPrefab);

                cellTransform.position = GetWorldPosition(x, y) + new Vector3(_cellSize * .5f, _cellSize * .5f, 0);
                cellTransform.SetParent(transform);

                GridCell gridCell = cellTransform.GetComponent<GridCell>();

                if (gridCell != null)
                {
                    _gridArray[x, y] = gridCell;
                    gridCell.x = x;
                    gridCell.y = y;
                }
            }
        }

        for (int i = 0; i < _maxMineAmount; i++)
        {
            GetRandomUnminedGridCell(out int x, out int y);

            _gridArray[x, y].isMined = true;
        }

        foreach (GridCell gridCell in _gridArray)
        {
            if (!gridCell.isMined)
            {
                gridCell.mineAround = GetMineAroundAmount(gridCell.x, gridCell.y);
            }
        }

        _mineLeftAmount = _maxMineAmount;
        _mineLeftAmountVisual.text = _mineLeftAmount.ToString();
    }

    public void RestartGame()
    {
        _gridArray = null;
        
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        StartNewGame();
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
            return _gridArray[x, y];
        } else
        {
            return default(GridCell);
        }
    }

    private void GetRandomUnminedGridCell(out int x, out int y)
    {
        System.Random random = new System.Random();

        x = random.Next(_gridArray.GetLength(0));
        y = random.Next(_gridArray.GetLength(1));

        if (_gridArray[x, y].isMined)
        {
            GetRandomUnminedGridCell(out x, out y);
        }
    }

    private int GetMineAroundAmount(int x, int y)
    {
        int amount = 0;

        foreach(GridCell gridCell in GetNeighboursGridCellList(x, y))
        {
            if (gridCell.isMined) amount++;
        }

        return amount;
    }

    private List<GridCell> GetNeighboursGridCellList(int x, int y)
    {
        List<GridCell> gridCellList = new List<GridCell>();

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

            OnGridOpen?.Invoke(this, new OnGridOpenEventArgs { x = x, y = y });

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
