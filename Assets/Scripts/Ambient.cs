using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Ambient : MonoBehaviour
{

    public AudioClip[] audioClips;
    public Vector2 timeRange;
    public Image button;
    public Sprite spriteOn;
    public Sprite spriteOff;
    AudioSource audioSource;
    Tween tween;
    bool music = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayClip(0);
        if (PlayerPrefs.GetInt("Music", 0) == 1) Toggle();
    }

    void PlayClip(int n)
    {
        if (n == audioClips.Length) n = 0;

        audioSource.clip = audioClips[n];
        audioSource.Play();
        tween = DOTween.Sequence().SetDelay(audioClips[n].length + Random.Range(timeRange.x, timeRange.y)).OnComplete(() => PlayClip(n + 1));
    }

    public void Pause()
    {
        if (!music) return;
        tween.Pause();
        audioSource.Pause();
    }

    public void UnPause()
    {
        if (!music) return;
        tween.Play();
        audioSource.UnPause();
    }

    public void Toggle() // ON/OFF + Save
    {
        tween.TogglePause();
        music = tween.IsPlaying();
        if (music)
        {
            audioSource.UnPause();
            button.sprite = spriteOn;
            PlayerPrefs.SetInt("Music", 0);
        }
        else
        {
            audioSource.Pause();
            button.sprite = spriteOff;
            PlayerPrefs.SetInt("Music", 1);
        }
    }
}
