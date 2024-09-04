using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    [HideInInspector] public int x;
    [HideInInspector] public int y;
    [HideInInspector] public int mineAround;
    [HideInInspector] public bool isMined;
    [HideInInspector] public bool isOpen;
    [HideInInspector] public bool isMarked;

    public TextMeshProUGUI mineAroundVisual;
    public Image overlay;
    public Image mark;
    public Image mine;

    private void Start()
    {
        //mineAroundVisual.gameObject.SetActive(!isMined);
        mine.gameObject.SetActive(isMined);

        //overlay.gameObject.SetActive(false);

        mark.gameObject.SetActive(false);

        if (!isMined && mineAround >= 1)
        {
            mineAroundVisual.text = mineAround.ToString();
        } else
        {
            mineAroundVisual.gameObject.SetActive(false);
        }
    }

    public override string ToString()
    {
        return isMarked.ToString();
    }
}
