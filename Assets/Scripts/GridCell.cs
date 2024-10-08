using TMPro;
using UnityEngine;
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
    public Image background;
    public Image overlay;
    public Transform mark;
    public Transform mine;

    private void Start()
    {
        foreach (Transform child in mine.transform)
        {
            child.gameObject.SetActive(isMined);
        }

        foreach (Transform child in mark.transform)
        {
            child.gameObject.SetActive(false);
        }

        //Change color for mineAroundVisual or hide it
        if (!isMined && mineAround > 0)
        {
            mineAroundVisual.text = mineAround.ToString();
            switch (mineAround)
            {
                case 1:
                    mineAroundVisual.color = Color.blue;
                    break;
                case 2:
                    mineAroundVisual.color = Color.green;
                    break;
                case 3:
                    mineAroundVisual.color = Color.red;
                    break;
                case 4:
                    mineAroundVisual.color = Color.black;
                    break;
                case 5:
                    mineAroundVisual.color = Color.yellow;
                    break;
                case 6:
                    mineAroundVisual.color = Color.cyan;
                    break;
                case 7:
                    mineAroundVisual.color = Color.grey;
                    break;
                case 8:
                    mineAroundVisual.color = Color.magenta;
                    break;
            }
        } else
        {
            mineAroundVisual.gameObject.SetActive(false);
        }
    }
}
