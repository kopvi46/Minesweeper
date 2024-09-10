using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource soundsSource;

    public AudioClip openCell;
    public AudioClip markCell;
    public AudioClip gameStart;
    public AudioClip gameWin;
    public AudioClip gameOver;

    private void Awake()
    {
        Instance = this;
    }
}
