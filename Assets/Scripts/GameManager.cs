using System.Collections;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Dongle lastDongle;

    [Header("# Start Group")]
    [SerializeField] private GameObject startGroup;

    [Header("# Background")]
    [SerializeField] private GameObject line;
    [SerializeField] private GameObject bottom;

    [Header("# Game Score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI maxScoreText;
    [SerializeField] private int score = 0;
    public int Score => score;
    private int maxScore;

    [Header("# Result Group")]
    [SerializeField] private GameObject resultGroup;
    [SerializeField] private TextMeshProUGUI resultScoreText;

    private bool isLive = true;
    public bool IsLive => isLive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        Application.targetFrameRate = 60;
        maxScore = PlayerPrefs.GetInt("MaxScore", 0);
        maxScoreText.SetText($"{maxScore}");
    }

    private void LateUpdate()
    {
        scoreText.SetText($"{score}");
    }

    private Dongle GetDongle()
    {
        // Object Pool에서 하나씩 Get
        GameObject dongleObj = ObjectPoolManager.Instance.Get(ObjectType.Dongle);
        Dongle dongle = dongleObj.GetComponent<Dongle>();
        dongle.Init(Random.Range(0, 4));

        return dongle;
    }

    private void NextDongle()
    {
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.gameObject.SetActive(true);
        SoundManager.Instance.PlaySfx(Sfx.Next);

        StartCoroutine(WaitNextDongle());
    }

    IEnumerator WaitNextDongle()
    {
        while (lastDongle != null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        NextDongle();
    }

    public void TouchDown()
    {
        if (lastDongle != null && isLive)
        {
            lastDongle.Drag();
        }
    }

    public void TouchUp()
    {
        if (lastDongle != null && isLive)
        {
            lastDongle.Drop();
            lastDongle = null;
        }
    }

    public void AddScore(int value)
    {
        score += value;
    }

    public void GameOver()
    {
        if (isLive)
        {
            isLive = false;
            StopAllCoroutines();

            StartCoroutine(GameOverRoutine());
        }
    }

    IEnumerator GameOverRoutine()
    {
        Dongle[] dongles = GameObject.FindObjectsByType<Dongle>(FindObjectsSortMode.None);

        foreach (Dongle dongle in dongles)
        {
            dongle.Hide(dongle.transform.position, true);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        maxScore = Mathf.Max(score, maxScore);
        PlayerPrefs.SetInt("MaxScore", maxScore);

        resultScoreText.SetText($"점수: {score}");
        resultGroup.SetActive(true);

        SoundManager.Instance.PlayBgm(false);
        SoundManager.Instance.PlaySfx(Sfx.GameOver);
    }

    public void StartGame()
    {
        // Object 활성화
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        // 사운드 Play
        SoundManager.Instance.PlaySfx(Sfx.Button);
        SoundManager.Instance.PlayBgm(true);

        Invoke("NextDongle", 1f);
    }

    public void ResetGame()
    {
        SoundManager.Instance.PlaySfx(Sfx.Button);

        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
