#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeScaleAndLockFPS : MonoBehaviour
{

    public float timeScale;
    public int FPS;
    float oldTimeScale;
    int oldFPS;

    [Space(10)]
    [Header("Debug")]
    public GameObject FPSDebug;
    public TextMeshProUGUI FPSCount;
    public Color[] colors = new Color[3]{ new Color(1f, 0.2431373f, 0.1686275f, 1f), new Color(1f, 0.9490196f, 0.3764706f, 1f), new Color(0.5803922f, 1f, 0.2431373f, 1f) };

    void Start()
    {
        StartCoroutine(ShowFPS());
#if DEVELOPMENT_BUILD
        FPSDebug.SetActive(true);
#endif
    }

    void Update()
    {
        if (oldFPS != FPS)
        {
            Application.targetFrameRate = FPS;
            oldFPS = FPS;
        }

        if (oldTimeScale != timeScale)
        {
            Time.timeScale = timeScale;
            oldTimeScale = timeScale;
        }
    }

    IEnumerator ShowFPS()
    {
        float startTime = Time.time;
        float sumFPS = 0;
        int min = int.MaxValue, max = 0;
        for (; ; )
        {
            float tFPS = 0;
            float fps = 1f / Time.deltaTime, fpsCount = 0;
            float ms = 0;
            while (tFPS < 1)
            {
                fps += (1f / Time.deltaTime);
                ms += Time.deltaTime;
                fpsCount++;
                tFPS += Time.deltaTime;
                yield return null;
            }

            fps /= fpsCount;
            sumFPS += fps;
            int avg = Mathf.Clamp((int)(sumFPS / (int)(Time.time - startTime)), 0, 300);
            min = Mathf.Max(Mathf.Min(min, (int)fps), 0);
            max = Mathf.Max(max, (int)fps);

            if (avg < 2f || avg > 300)
            {
                sumFPS = 0;
                startTime = Time.time;
            }
            if (min < 2) min = int.MaxValue;
            if (max > 300) max = 0;

            string s = "";
            s += (int)fps + " fps" + "\n";
            s += "<size=60%>" + ((ms / fpsCount) * 1000).ToString("0.0") + "ms" + "\n\n";
            s += "<size=70%>" + "<#" + ColorUtility.ToHtmlStringRGB(colors[(int)Mathf.Clamp(avg / 27.5f, 0, 2)]) + ">" + avg + " <#FFFFFF>avg" + "\n";
            s += "<#" + ColorUtility.ToHtmlStringRGB(colors[(int)Mathf.Clamp(min / 27.5f, 0, 2)]) + ">" + min + " <#FFFFFF>min" + "\n";
            s += "<#" + ColorUtility.ToHtmlStringRGB(colors[(int)Mathf.Clamp(max / 27.5f, 0, 2)]) + ">" + max + " <#FFFFFF>max" + "\n";
            FPSCount.text = s;
        }
    }
}
#endif