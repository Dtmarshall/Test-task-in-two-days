using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class HealthPoints : MonoBehaviour
{

    bool mob;

    [Header("HP Bar")]
    public Transform healthStrip;
    public Transform oldHealthStrip;
    public GameObject frame;
    public Sprite redStrip;
    public TextMeshPro text;
    public GameObject bossBar;
    Slider bossSlider;
    TextMeshProUGUI bossText;

    [Header("Damage Text")]
    public GameObject damageTxt;
    public Vector2 txtTime;
    public AnimationCurve curve;
    public float speed;
    public Color critColor;
    public float critSize;
    List<GameObject> damageNumbers = new List<GameObject>();

    Transform thisTransform;

    void Start()
    {
        thisTransform = transform;
    }

    public void Mob()
    {
        healthStrip.GetComponent<SpriteRenderer>().sprite = redStrip;
        Destroy(text.gameObject);
        mob = true;
    }

    public void Boss(Transform canvas)
    {
        bossSlider = Instantiate(bossBar, canvas).GetComponent<Slider>();
        Transform bossSliderTransform = bossSlider.transform;
        bossSliderTransform.DOLocalMoveY(bossSliderTransform.localPosition.y - 300, 2);

        bossText = bossSlider.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        GetComponent<SpriteRenderer>().enabled = false;
        healthStrip.gameObject.SetActive(false);
        oldHealthStrip.gameObject.SetActive(false);
        frame.SetActive(false);
    }

    public void SetHP(int maxHP, int curHP)
    {
        Vector3 newPos = Vector3.right * Mathf.Lerp(2.05f, 0, (float)curHP / maxHP);
        healthStrip.localPosition = newPos;
        oldHealthStrip.DOLocalMove(newPos, 0.75f).SetEase(Ease.Linear);
        if (!mob) text.text = curHP.ToString();
    }

    public void SetHPBoss(int maxHP, int curHP)
    {
        string curhp = curHP.ToString();
        if (curHP < 1000) bossText.text = curhp;
        else bossText.text = curHP / 1000 + "," + curhp.Substring(curhp.Length - 3);
        bossText.text += "/" + maxHP / 1000 + "," + maxHP % 1000;
        bossSlider.value = (float)curHP / maxHP;
    }

    public void DestroyBossBar()
    {
        Destroy(bossSlider.gameObject);
    }

    public IEnumerator Damage(int damage, bool crit)
    {
        GameObject txtObject = Instantiate(damageTxt, thisTransform.position, Quaternion.identity);
        TextMeshPro txt = txtObject.GetComponent<TextMeshPro>();
        damageNumbers.Add(txtObject);
        Transform txtTransform = txtObject.transform;
        Vector3 startPos = txtTransform.position;
        float x = Random.Range(-1f, 1f);
        Vector3 size = txtTransform.localScale* (crit ? critSize : 1);
        txt.text = damage.ToString();
        Color color = crit ? critColor : txt.color;

        float time = Random.Range(txtTime.x, txtTime.y);
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            float tCurve = curve.Evaluate(t / time);
            txtTransform.position += thisTransform.rotation * new Vector3(x, 1, 0) * Time.deltaTime * speed * tCurve;
            txtTransform.localScale = Vector3.Scale(Vector3.one, size) * tCurve;
            txt.color = color * new Color(1, 1, 1, tCurve);
            yield return null;
        }

        damageNumbers.Remove(txtObject);
        Destroy(txtObject);
    }

    void OnDestroy()
    {
        for (byte i = 0; i < damageNumbers.Count; i++) Destroy(damageNumbers[i]);
    }
}