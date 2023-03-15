using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameLoop : MonoBehaviour
{

    public Slime player;
    Transform playerTransform;
    public Enemy enemy;
    public HealthPoints hpBar;
    public Ambient ambient;

    [Header("Move")]
    public float distMove;
    public float distBetweenPlayerAndEnemy;

    [Header("Camera")]
    public Transform camPoint;
    public float camRangeX;
    Camera cam;
    Transform camTransform;

    [Header("Money")]
    public TextMeshProUGUI goldTxt;
    Transform goldTxtTransform;
    AudioSource goldAudioSource;
    Tween goldTween;
    int gold;
    public GameObject coin;
    public Vector2Int coinsSpawnCount;
    public float coinsRotateTime;
    public Vector2 coinsForce;
    public float coinsRotateSpeed;
    public float coinsCollectDelay;
    public float coinsCollectTime;
    public AnimationCurve coinsCollectCurve;
    public float giveGoldMulti;

    [System.Serializable]
    public class DigitUpgrades
    {
#if UNITY_EDITOR
        public string name;
#endif
        public int lvl = 1;
        public int cost;
        public int addCost;
        public float addDigit;
        public TextMeshProUGUI lvlTxt;
        public TextMeshProUGUI costTxt;
        public TextMeshProUGUI addDigitTxt;
    }
    [Header("Upgrades")]
    public DigitUpgrades[] digitUpgrades;
    public AudioSource upgradeAudioSource;
    public GameObject upgradeParticles;
    Tween playerUpgradeVFXTween;

    [Header("Fight")]
    Enemy curEnemies;
    bool fight;

    [Header("Lose")]
    public AudioClip loseSound;

    [Header("Stages")]
    public float increasDifficulty;
    public TextMeshProUGUI stageTxt;
    public Slider curEnemySlider;
    public Image[] curEnemyIcons;
    public TextMeshProUGUI[] curEnemyNumbers;
    public Sprite curEnemySilverIcon, curEnemyGoldIcon;
    float difficulty;
    int curStage;
    int curEnemy;

    [Header("Transition")]
    public Material transition;
    public float transitionTime;
    public AnimationCurve transitionCurve;

    void Awake()
    {
        goldTxtTransform = goldTxt.transform;
        goldAudioSource = goldTxt.GetComponent<AudioSource>();
        cam = Camera.main;
        camTransform = cam.transform;
        //Cache

        (player = Instantiate(player)).CustomStart(hpBar, this);
        playerTransform = player.transform;
        //Init

        gold = PlayerPrefs.GetInt("Gold", 0);
        UpdateGold();
        for (int i = 0; i < digitUpgrades.Length; i++)
        {
            digitUpgrades[i].lvl = PlayerPrefs.GetInt("LVLUpgrade" + i, 1);
            SetDigitUpgrade(i, digitUpgrades[i].lvl - 1);
        }
        curStage = PlayerPrefs.GetInt("Stage", 1);
        stageTxt.text = "Normal 1-" + curStage;
        difficulty = Mathf.Pow(curStage, increasDifficulty);
        //Saves

        playerTransform.position = new Vector3(-distMove * 5 * (curStage - 1), 0, 0);
        //Player offset

        transition.SetFloat("_Fill", 1);
        transition.DOFloat(0, "_Fill", transitionTime).SetEase(transitionCurve).OnComplete(() => NextWave());
        //Transition

        Application.targetFrameRate = 60;
    }

    void NextWave()
    {
        if (curEnemy == 5)
        {
            PlayerPrefs.SetInt("Stage", ++curStage);
            stageTxt.text = "Normal 1-" + curStage;
            curEnemy = 0;
            for (int i = 0; i < 4; i++)
            {
                curEnemyNumbers[i].color = Color.white;
                curEnemyIcons[i].sprite = curEnemySilverIcon;
            }
            player.RecoverHP();
        }
        difficulty = Mathf.Pow(curStage + curEnemy / 5f, increasDifficulty);
        curEnemy++;

        SpawnEnemy();

        player.Move(-distMove);
    }

    void SpawnEnemy()
    {
        float offset = playerTransform.position.x - distMove - distBetweenPlayerAndEnemy;
        (curEnemies = Instantiate(enemy, Vector3.right * offset, Quaternion.Euler(0, 80, 0))).CustomStart(hpBar, this, player, difficulty);
    }

    public void EndMove()
    {
        fight = true;
        player.StartFight(curEnemies);
        curEnemies.StartFight();
    }

    public void EndFight()
    {
        fight = false;
        player.EndFight();

        if (curEnemy < 5)
        {
            curEnemyIcons[curEnemy - 1].sprite = curEnemyGoldIcon;
            curEnemyNumbers[curEnemy - 1].color = new Color(0.753f, 0.333f, 0.235f);
        }
        
        DOTween.Sequence().SetDelay(1).OnComplete(() => NextWave());
    }

    public void Death()
    {
        curEnemies.EndFight();
        player.EndFight();
        if (curEnemy == 1) PlayerPrefs.SetInt("Stage", curStage - 1);

        ambient.Pause();
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = loseSound;
        audioSource.Play();
        //SFX

        transition.DOFloat(-1, "_Fill", transitionTime).SetDelay(2.66f).SetEase(transitionCurve).OnComplete(() => Restart());
    }

    void Restart()
    {
        DOTween.KillAll();
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    void Update()
    {
        camPoint.position = playerTransform.position + Vector3.right * camRangeX;
        curEnemySlider.value = (Mathf.Abs(playerTransform.position.x) - distMove * curStage) / (distMove * 4) - (curStage - 1);

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F5))
        {
            PlayerPrefs.DeleteAll();
            Restart();
        }
#endif
    }

    public void BuyUpgrade(int n)
    {
        switch (n)
        {
            case int c when (c < 3): //Digit Upgrades
                {
                    if (!Buy(digitUpgrades[n].cost)) return;
                    digitUpgrades[n].lvl++;
                    PlayerPrefs.SetInt("LVLUpgrade" + n, digitUpgrades[n].lvl);
                    SetDigitUpgrade(n, 1);

                    upgradeAudioSource.pitch = 1 + digitUpgrades[n].lvl * 0.05f;
                    upgradeAudioSource.Play();

                    playerUpgradeVFXTween.Kill(true);
                    playerUpgradeVFXTween = player.thisRenderer.material.DOColor(new Color(1f, 0.58f, 0.136f), 0.33f).From();

                    Instantiate(upgradeParticles, playerTransform.position, Quaternion.identity);
                }
                break;
            case 3:
                {
                    AddGold(10);
                }
                break;
            case 4:
                {
                    if (fight) curEnemies.Death();
                }
                break;
            case 5:
                {
                    Time.timeScale = Mathf.Min(Time.timeScale + 0.5f, 5);
                }
                break;
            case 6:
                {
                    Time.timeScale = Mathf.Max(Time.timeScale - 0.5f, 1);
                }
                break;
        }
    }

    void SetDigitUpgrade(int n, int lvl)
    {
        digitUpgrades[n].cost += digitUpgrades[n].addCost * lvl;
        digitUpgrades[n].costTxt.text = digitUpgrades[n].cost.ToString();
        digitUpgrades[n].lvlTxt.text = "Lv " + digitUpgrades[n].lvl;

        float digit = 0;
        switch (n)
        {
            case 0: // Damage
                {
                    player.damage += (int)(digitUpgrades[n].addDigit * lvl);
                    digit = player.damage;
                }
                break;
            case 1: // Attack Speed
                {
                    player.attackSpeed += digitUpgrades[n].addDigit * lvl;
                    digit = player.attackSpeed;
                }
                break;
            case 2: // HP
                {
                    player.AddHP((int)(digitUpgrades[n].addDigit * lvl));
                    digit = player.maxHP;
                }
                break;
        }
        digitUpgrades[n].addDigitTxt.text = digit.ToString();
    }

    void UpdateGold()
    {
        if (gold == 0) goldTxt.text = "0";
        else goldTxt.text = gold.ToString("# ### ### ###");
        PlayerPrefs.SetInt("Gold", gold);
    }

    public void AddGold(int count)
    {
        gold += count;
        UpdateGold();

        goldAudioSource.pitch += 0.033f;
        goldAudioSource.Play();

        float savePitch = goldAudioSource.pitch;
        goldTween.Kill(true);
        if (savePitch > 1) goldAudioSource.pitch = savePitch;
        goldTween = goldTxtTransform.DOScale(1.33f, 0.2f).From().OnComplete(() => goldAudioSource.pitch = 1);
    }

    public void SpendGold(int count)
    {
        gold -= count;
        UpdateGold();
    }

    bool Buy(int count)
    {
        if (count > gold) return false;
        SpendGold(count);
        return true;
    }

    public void SpawnCoins(Vector3 pos)
    {
        int count = Random.Range(coinsSpawnCount.x, coinsSpawnCount.y);
        for (int i = 0; i < count; i++) StartCoroutine(SpawnCoin(pos, count, i));
    }

    IEnumerator SpawnCoin(Vector3 pos, int count, int i)
    {
        GameObject coin = Instantiate(this.coin, pos, Quaternion.Euler(0, Random.Range(-180, 180), 90));
        Transform coinTransform = coin.transform;
        Rigidbody coinRigidbody = coin.GetComponent<Rigidbody>();
        coinRigidbody.AddForce(Quaternion.Euler(0, 360 * (i / (float)count), 0) * new Vector3(Random.Range(0.1f, 0.25f), 0.9f, 0) * Random.Range(coinsForce.x, coinsForce.y), ForceMode.Impulse);

        float rotateTime = coinsRotateTime + i * coinsCollectDelay;
        float t = 0;
        while (t < rotateTime)
        {
            t += Time.deltaTime;
            coinTransform.Rotate(new Vector3(0, coinsRotateSpeed, 0), Space.World);
            yield return null;
        }

        Vector3 startPos = coinTransform.position;
        Vector3 randomPointOnCircle = Random.insideUnitCircle.normalized * 25;
        coinRigidbody.isKinematic = true;
        float startScale = coinTransform.localScale.x;

        t = 0;
        while (t < coinsCollectTime)
        {
            t += Time.deltaTime;
            float tCurve = coinsCollectCurve.Evaluate(t / coinsCollectTime);

            Vector3 endPoint = cam.ScreenToWorldPoint(goldTxtTransform.position + Vector3.forward);
            Vector3 wayPoint = endPoint + Quaternion.Euler(randomPointOnCircle.x, 0, randomPointOnCircle.y) * ((startPos - endPoint) * 0.5f);
            coinTransform.position = Mathf.Pow(1 - tCurve, 2) * startPos + 2 * (1 - tCurve) * tCurve * wayPoint + Mathf.Pow(tCurve, 2) * endPoint;
            coinTransform.rotation = Quaternion.Slerp(coinTransform.rotation, camTransform.rotation * Quaternion.Euler(0, 90, 90), tCurve);
            coinTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 0.3f, tCurve);
            yield return null;
        }

        AddGold((int)((curStage + curEnemy / 5f) * giveGoldMulti));
        Destroy(coin);
    }
}
