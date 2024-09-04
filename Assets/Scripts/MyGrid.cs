using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

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

    private GridCell[,] _gridArray;
    private int _closedCellCount;

    private void Start()
    {
        transform.position -= new Vector3(_width / 2, _height / 2, 0);
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
            GetRandomGridCell(out int x, out int y);

            _gridArray[x, y].isMined = true;
        }

        foreach (GridCell gridCell in _gridArray)
        {
            if (!gridCell.isMined)
            {
                gridCell.mineAround = GetMineAroundAmount(gridCell.x, gridCell.y);
            }
        }

        OnGridOpen += MyGrid_OnGridOpen;
    }

    //private void Start()
    //{
    //    _gridArray = new GridCell[_width, _height];

    //    //int mineAmount = 0;

    //    for (int x = 0;  x < _gridArray.GetLength(0); x++)
    //    {
    //        for (int y = 0; y < _gridArray.GetLength(1); y++)
    //        {
    //            GridCell newGridCell = new GridCell(x, y);

    //            //if (mineAmount <  _maxMineAmount)
    //            //{
    //            //    System.Random random = new System.Random();
    //            //    newGridCell.isMined = random.Next(0, 2) == 0;

    //            //    if (newGridCell.isMined) mineAmount++;
    //            //}


    //            _gridArray[x, y] = newGridCell;
    //        }
    //    }

    //    bool showDebug = true;
    //    if (showDebug)
    //    {
    //        TextMesh[,] debugTextArray = new TextMesh[_width, _height];

    //        for (int x = 0; x < _gridArray.GetLength(0); x++)
    //        {
    //            for (int y = 0; y < _gridArray.GetLength(1); y++)
    //            {
    //                debugTextArray[x, y] = MyUtils.CreateWorldText(_gridArray[x, y]?.ToString(), null, GetWorldPosition(x, y) + new Vector3(_cellSize, _cellSize, 0) * .5f, _fontSize, Color.white, TextAnchor.MiddleCenter);
    //                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
    //                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);

    //            }
    //        }
    //        Debug.DrawLine(GetWorldPosition(0, _height), GetWorldPosition(_width, _height), Color.white, 100f);
    //        Debug.DrawLine(GetWorldPosition(_width, 0), GetWorldPosition(_width, _height), Color.white, 100f);

    //        OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
    //        {
    //            debugTextArray[eventArgs.x, eventArgs.y].text = _gridArray[eventArgs.x, eventArgs.y]?.ToString();
    //        };
    //    }

    //    OnGridValueChanged += MyGrid_OnGridValueChanged;
    //}

    private void MyGrid_OnGridOpen(object sender, OnGridOpenEventArgs e)
    {
        if (GetGridCell(e.x, e.y).isMined)
        {
            Debug.Log("You have lost!");
        }

        if (_closedCellCount == _maxMineAmount)
        {
            Debug.Log("You have won!");
        }

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = MyUtils.GetMouse2DWorldPosition();
            GetXY(mouseWorldPosition, out int x, out int y);

            OpenGridCell(x, y);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MyUtils.GetMouse2DWorldPosition();
            GetXY(mouseWorldPosition, out int x, out int y);
            if (GetGridCell(x, y) != null && !GetGridCell(x, y).isOpen)
            {
                GetGridCell(x, y).isMarked = !GetGridCell(x, y).isMarked;

                GetGridCell(x, y).mark.gameObject.SetActive(GetGridCell(x, y).isMarked);
            }
        }
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

    private void GetRandomGridCell(out int x, out int y)
    {
        System.Random random = new System.Random();

        x = random.Next(_gridArray.GetLength(0));
        y = random.Next(_gridArray.GetLength(1));

        if (_gridArray[x, y].isMined)
        {
            GetRandomGridCell(out x, out y);
        }
    }

    private int GetMineAroundAmount(int x, int y)
    {
        int amount = 0;

        //if (GetGridCell(x, y) != null)
        //{
        //    //Top left
        //    if (GetGridCell(x - 1, y + 1) != null && GetGridCell(x - 1, y + 1).isMined) amount++;
        //    //Top
        //    if (GetGridCell(x, y + 1) != null && GetGridCell(x, y + 1).isMined) amount++;
        //    //Top right
        //    if (GetGridCell(x + 1, y + 1) != null && GetGridCell(x + 1, y + 1).isMined) amount++;
        //    //Right
        //    if (GetGridCell(x + 1, y) != null && GetGridCell(x + 1, y).isMined) amount++;
        //    //Bottom right
        //    if (GetGridCell(x + 1, y - 1) != null && GetGridCell(x + 1, y - 1).isMined) amount++;
        //    //Bottom
        //    if (GetGridCell(x, y - 1) != null && GetGridCell(x, y - 1).isMined) amount++;
        //    //Bottom left
        //    if (GetGridCell(x - 1, y - 1) != null && GetGridCell(x - 1, y - 1).isMined) amount++;
        //    //Left
        //    if (GetGridCell(x - 1, y) != null && GetGridCell(x - 1, y).isMined) amount++;
        //}

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

        if (currentGridCell != null && !currentGridCell.isMarked)
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

    //private void OpenGridCell(int x, int y)
    //{
    //    Stack<GridCell> cellsToOpen = new Stack<GridCell>();
    //    cellsToOpen.Push(GetGridCell(x, y));

    //    while (cellsToOpen.Count > 0)
    //    {
    //        GridCell currentGridCell = GetGridCell(x, y);

    //        if (currentGridCell != null && !currentGridCell.isMarked)
    //        {
    //            _closedCellCount--;

    //            currentGridCell.isOpen = true;

    //            currentGridCell.overlay.gameObject.SetActive(false);

    //            OnGridOpen?.Invoke(this, new OnGridOpenEventArgs { x = x, y = y });

    //            if (!currentGridCell.isMined && currentGridCell.mineAround == 0)
    //            {
    //                foreach (GridCell gridCell in GetNeighboursGridCellList(currentGridCell.x, currentGridCell.y))
    //                {
    //                    if (!gridCell.isOpen)
    //                    {
    //                        cellsToOpen.Push(gridCell);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}
