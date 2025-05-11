using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject player, gameOverPanel, startPanel, value;
    public Camera mainCamera;

    AudioSource audioSource;
    public AudioClip EnemyRemoved, GameOverMusic, ExtendTime, Damage;

    public Canvas canvas;
    public Image circleSlider;
    public Text scoreBoad, resultScoreBoad, limit, conbo;
    public Text[] rankingText = new Text[3];
    public Slider limitslider;

    public List<GameObject> enemies;

    public float pauseDuration = 2.0f;
    public float cameraMoveSpeed = 5.0f;
    int previousScore;
    string[] ranking = { "score1", "score2", "score3" };
    int[] topScores = new int[3];
    int score = 0;
    public int Score
    {
        set
        {
            score = Mathf.Clamp(value, 0, 9999999);
            scoreBoad.text = score.ToString();
        }
        get
        {
            return score;
        }

    }

    public float timeLimit = 60.0f;  // 制限時間
    public Animator time, UI;
    public float currentTime
    {
        set
        {
            timeLimit = Mathf.Clamp(value, 0, 60);
        }
        get
        {
            return timeLimit;
        }
    }

    public Vector2 spawnArea = new Vector2(10f, 5f);
    public Vector3 spawnCenter = Vector3.zero; // 範囲の中心

    bool gameover = false;
    bool once;
    bool isRestart;

    public float chainTimeLimit = 0.2f; // 連鎖の制限時間
    public float chainScoreMultiplier = 0.1f; // 連鎖時のスコア倍率

    private float chainTimer; // 連鎖の制限時間のカウントダウン用
    private int currentChainCount; // 現在の連鎖数
    private float scoreMultiplier; // スコア倍率

    private void Start()
    {
        Time.timeScale = 0f;

        // パネルを表示
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        // 数秒後にパネルを非表示にしてゲーム再生
        StartCoroutine(StartGameAfterDelay());
        ResetChain(); // ゲーム開始時に初期化

        EnemyManager enemyManager = FindObjectOfType<EnemyManager>();
        audioSource = GetComponent<AudioSource>();
        gameOverPanel.gameObject.SetActive(false);

        enemies = enemyManager.ENEMIES;
        Score = 0;

        currentTime = timeLimit;

        gameover = false;
        once = true;
        isRestart = false;

        PlayerManager.OnPlayerGrounded += HandlePlayerGrounded;

    }

    private void HandlePlayerGrounded()
    {
        // プレイヤーが地面に接触した時の処理
        Debug.Log("Player grounded!");
        Score -= 30;
        audioSource.PlayOneShot(Damage);
        SpawnAnyText("-30", new Vector3(0, -160, 0), Color.red);
    }

    private void OnDestroy()
    {
        // ゲームオブジェクトが破棄される時に登録したイベントを解除
        PlayerManager.OnPlayerGrounded -= HandlePlayerGrounded;
    }

    private void Update()
    {


        // 制限時間内にスコアが一定以上に達しなかった場合

        if (currentTime < 10)
        {
            time.SetBool("limit", true);
        }
        else
        {
            time.SetBool("limit", false);
        }
        if (currentTime <= 0.0f)
        {


            gameover = true;

        }

        //カメラのチェイス
        if (player == null || mainCamera == null)
        {
            return;
        }

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(player.transform.position);
        if (viewportPos.x < 0.2f || viewportPos.x > 0.8f || viewportPos.y < 0.2f || viewportPos.y > 0.8f)
        {
            Vector3 targetPosition = new Vector3(player.transform.position.x, player.transform.position.y, mainCamera.transform.position.z);
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * cameraMoveSpeed);
        }

        //敵を倒した時の処理（省略版）
        if (enemies.Count < 30)
        {
            Shake(mainCamera.transform, 0.25f, 0.1f);
            audioSource.PlayOneShot(EnemyRemoved);
        }

        //ゲームオーバー処理

        if (gameover)
        {
            gameOverPanel.gameObject.SetActive(true);
            resultScoreBoad.text = Score.ToString();
            Time.timeScale = 0.0f;
            Shake(mainCamera.transform, 0.25f, 1f);


            if (once)
            {
                once = false;

                audioSource.Stop();
                audioSource.clip = GameOverMusic;
                audioSource.PlayOneShot(GameOverMusic);

                //ランキングの表示
                GetRanking();
                SetRanking(Score);

                Text[] texts = new Text[3];
                for (int i = 0; i < ranking.Length; i++)
                {
                    texts[i] = GameObject.Find(ranking[i]).GetComponent<Text>();
                    rankingText[i].text = topScores[i].ToString();
                }

                StartCoroutine(ActivateGameOverPanel());

            }

            if (isRestart)
            {
                if (Input.GetButton("joystick button 1"))
                {

                    SceneManager.LoadScene("title");
                    Time.timeScale = 1.0f;
                }
            }

        }



        IEnumerator ActivateGameOverPanel()
        {
            // 指定の秒数待機
            yield return new WaitForSecondsRealtime(3.0f);

            isRestart = true;
        }

        //スコアが更新された時の処理
        int currentScore = Score;

        if (previousScore < currentScore)
        {
            scoreBoad.rectTransform.rotation = Quaternion.Euler(0, 0, Random.Range(-10, 10));
            ShakeText(conbo, 0.25f, 10f);

        }
        previousScore = currentScore;

        if (Input.GetKeyDown(KeyCode.X))
        {
            PlayerPrefs.DeleteAll();
        }

        //コンボ
        if (chainTimer > 0f)
        {
            chainTimer -= Time.deltaTime;
            if (chainTimer <= 0f)
            {
                ResetChain(); // 制限時間が経過したら連鎖をリセット
            }
        }

        //UI処理
        float holdValue = FindObjectOfType<PlayerManager>().normalizedValue;

        circleSlider.material.SetFloat("_value", Utility.CustomFunctions.Map(holdValue, 0, 3f, 0, 1.2f));

        currentTime -= Time.deltaTime;

        float totalMultiplier = 0.1f * Mathf.Round(10 + (10 * scoreMultiplier));
        conbo.text = "x" + totalMultiplier.ToString("F1");

        if (totalMultiplier != 1)
        {
            UI.SetBool("conbo", true);
        }
        limit.text = currentTime.ToString();
        limitslider.value = currentTime;
    }

    private int CalculateScore(float score)
    {
        // スコア計算のロジック（例: 敵ごとに固定のスコア）

        return Mathf.RoundToInt(score * (1.0f + scoreMultiplier));
    }

    private void ResetChain()
    {
        chainTimer = 0f;
        currentChainCount = 0;
        scoreMultiplier = 0f;
        UI.SetBool("conbo", false);
    }

    //イベントリスナー
    void OnEnable()
    {
        FollowPlayer.OnDestroyed += OnEnemyDestroyed;
    }

    void OnDisable()
    {
        FollowPlayer.OnDestroyed -= OnEnemyDestroyed;
    }

    void OnEnemyDestroyed(FollowPlayer.EnemyType enemyType)
    {
        float scoreToAdd = 0;
        switch (enemyType)
        {
            case FollowPlayer.EnemyType.normal:
                scoreToAdd = 22;
                break;

            case FollowPlayer.EnemyType.timeExtender:
                scoreToAdd = 22;
                currentTime += 5;
                Color color;
                ColorUtility.TryParseHtmlString("#EEB927", out color);
                audioSource.PlayOneShot(ExtendTime);
                SpawnAnyText("+5", new Vector3(0, 157, 0), color);
                break;

            case FollowPlayer.EnemyType.highScore:
                scoreToAdd = 100;
                break;

            case FollowPlayer.EnemyType.punk:
                scoreToAdd = 22;
                break;


        }

        // オブジェクトAが消滅したときに実行する処理
        Score += CalculateScore(scoreToAdd);
        SpawnText(CalculateScore(scoreToAdd));
        if (currentChainCount > 1)
        {
            chainTimer = chainTimeLimit;
        }

        currentChainCount++; // 連鎖数を増加
        scoreMultiplier += chainScoreMultiplier; // スコア倍率を増加
    }

    public void Shake(Transform transform, float duration, float magnitude)
    {
        StartCoroutine(Utility.CustomFunctions.DoShake(transform, duration, magnitude));
    }

    public void ShakeText(Text text, float duration, float magnitude)
    {
        RectTransform parentRectTransform = text.GetComponent<RectTransform>();
        StartCoroutine(Utility.CustomFunctions.DoShakeRect(parentRectTransform, duration, magnitude));
    }

    void GetRanking()
    {
        for (int i = 0; i < ranking.Length; i++)
        {
            topScores[i] = PlayerPrefs.GetInt(ranking[i]);
        }
    }

    void SetRanking(int _value)
    {
        for (int i = 0; i < ranking.Length; i++)
        {
            if (_value > topScores[i])
            {
                var change = topScores[i];
                topScores[i] = _value;
                _value = change;
            }
        }

        for (int i = 0; i < topScores.Length; i++)
        {
            PlayerPrefs.SetInt(ranking[i], topScores[i]);
        }
    }

    void SpawnAnyText(string hoge, Vector3 position, Color color)
    {
        GameObject newText = Instantiate(value, position, Quaternion.identity);
        newText.transform.SetParent(canvas.transform, false);

        Text textComponent = newText.transform.GetChild(0).gameObject.GetComponent<Text>();

        textComponent.fontSize = 30;
        textComponent.text = hoge;
        textComponent.color = color;


        // インスタンス化されたテキストを数秒で消滅
        Destroy(newText, 1);
    }

    void SpawnText(int score)
    {
        // プレハブからテキストをインスタンス化
        GameObject newText = Instantiate(value, GetRandomSpawnPosition(), Quaternion.identity);
        newText.transform.SetParent(canvas.transform, false);

        // テキストにランダムな数値を代入
        Text textComponent = newText.transform.GetChild(0).gameObject.GetComponent<Text>();


        textComponent.text = "+" + score.ToString();


        // インスタンス化されたテキストを数秒で消滅
        Destroy(newText, 1);
    }

    Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(spawnCenter.x - spawnArea.x / 2f, spawnCenter.x + spawnArea.x / 2f);
        float y = Random.Range(spawnCenter.y - spawnArea.y / 2f, spawnCenter.y + spawnArea.y / 2f);
        return new Vector3(x, y, 0f);
    }

    IEnumerator StartGameAfterDelay()
    {
        yield return new WaitForSecondsRealtime(pauseDuration);

        // パネルを非表示に
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        // ゲーム再生
        Time.timeScale = 1f;
    }
}
