using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class GameManager : MonoBehaviour
{
    static GameManager inst;
    public static GameManager Inst { get => inst; }

    [SerializeField] Board b1, b2;
    [SerializeField] TMP_Text p1NameText, p2NameText;
    Animator p1NameTextAnimator, p2NameTextAnimator;
    [SerializeField] GameObject winnerScreen;
    [SerializeField] TMP_Text winnerText;

    [SerializeField] int tokensStart = 4;
    int tokensTotal;
    int scoreMinWin;
    public int ScoreMinWin { get => scoreMinWin; }

    [SerializeField] int eatMin = 2;
    [SerializeField] int eatMax = 3;
    public int EatMin { get => eatMin; }
    public int EatMax { get => eatMax; }

    [SerializeField] int maxTurnWithoutEat = 100;
    int turnWithoutEat;

    [SerializeField] LayerMask holeLayer;
    public LayerMask HoleLayer { get => holeLayer; }

    [SerializeField, Range(0f, 100f)] float timeSpeed = 1f;
    [SerializeField] float dtInitBoardHole = 0.1f;
    [SerializeField] float dtPutToken = 0.1f;
    [SerializeField] float dtEatToken = 0.3f;
    [SerializeField] float botMinTime = 0.7f;
    [SerializeField] float botMaxTime = 3f;
    [SerializeField] float botMinTimeValue = 5f;
    [SerializeField] float botTimePow = 0.5f;
    public float BotMinTime { get => botMinTime; }
    public float BotReflectionTime(float value)
        => Mathf.Lerp(botMaxTime, botMinTime, Mathf.Pow(Mathf.InverseLerp(-botMinTimeValue, botMinTimeValue, value), botTimePow));

    [SerializeField] AudioClip takeTokensClip;
    [SerializeField] AudioClip dropTokenClip;
    [SerializeField] AudioClip eatTokensClip;
    [SerializeField] float dropTokenPlusMinusPitch = 0.05f;
    [SerializeField] float eatTokensDPitch = 0.03f;

    Animator animator;

    void OnValidate()
    {
        UpdateTimeScale();
    }

    void Awake()
    {
        Cache();
    }


    void UpdateTimeScale()
    {
        if (Time.timeScale == timeSpeed)
            return;

        Time.timeScale = timeSpeed;
        //Time.fixedDeltaTime = 0.02f * timeSpeed;
    }

    void Cache()
    {
        inst = this;
        animator = GetComponent<Animator>();
        p1NameTextAnimator = p1NameText.GetComponent<Animator>();
        p2NameTextAnimator = p2NameText.GetComponent<Animator>();
    }

    public void Open() => animator.SetBool("open", true);

    public void Close() => animator.SetBool("open", false);

    public void StartGame(PlayerData p1Data, PlayerData p2Data)
    {
        (InGamePlayer p1InGame, InGamePlayer p2InGame) = BuildInGamePlayers(p1Data, p2Data);

        p1InGame.nameText.text = p1InGame.data.name;
        p2InGame.nameText.text = p2InGame.data.name;

        p1InGame.nameTextAnimator.SetBool("playerTurn", false);
        p2InGame.nameTextAnimator.SetBool("playerTurn", false);

        StartCoroutine(GameLoop(p1InGame, p2InGame));
    }

    (InGamePlayer p1InGame, InGamePlayer p2InGame) BuildInGamePlayers(PlayerData p1Data, PlayerData p2Data)
    {
        InGamePlayer p1InGame = new InGamePlayer()
        {
            data = p1Data,
            board = b1,
            nameText = p1NameText,
            nameTextAnimator = p1NameTextAnimator,
         };

        InGamePlayer p2InGame = new InGamePlayer()
        {
            data = p2Data,
            board = b2,
            nameText = p2NameText,
            nameTextAnimator = p2NameTextAnimator,
        };

        p1InGame.opponent = p2InGame;
        p2InGame.opponent = p1InGame;

        return (p1InGame, p2InGame);
    }

    IEnumerator InitBoards(int holeTokensCount, float dtHole)
    {
        tokensTotal = 0;

        yield return StartCoroutine(InitBoard(b1, holeTokensCount, dtHole));
        yield return StartCoroutine(InitBoard(b2, holeTokensCount, dtHole));

        scoreMinWin = tokensTotal / 2 + 1;
    }

    IEnumerator InitBoard(Board board, int holeTokensCount, float dtHole)
    {
        board.Score.Value = 0;

        foreach (Counter hole in board.Holes)
            hole.Value = 0;

        foreach (Counter hole in board.Holes)
        {
            yield return new WaitForSeconds(dtHole);
            hole.Value = holeTokensCount;
            tokensTotal += holeTokensCount;
        }
    }

    public void CleanBoards()
    {
        CleanBoard(b1);
        CleanBoard(b2);

        tokensTotal = 0;
        scoreMinWin = 0;
    }

    void CleanBoard(Board board)
    {
        board.Score.Value = 0;

        foreach (Counter hole in board.Holes)
            hole.Value = 0;
    }

    IEnumerator GameLoop(InGamePlayer p1InGame, InGamePlayer p2InGame)
    {
        yield return StartCoroutine(InitBoards(tokensStart, dtInitBoardHole));

        InGamePlayer playerTurn = p1InGame;

        while (true)
        {
            // turn start
            playerTurn.nameTextAnimator.SetBool("playerTurn", true);

            // chose hole
            if (CanPlay(playerTurn))
            {
                // chose hole
                yield return StartCoroutine(playerTurn.ChoseHole());

                // play
                yield return StartCoroutine(Play(playerTurn, playerTurn.chosenHole));
            }
            
            // HUNGRYYY !
            else yield return StartCoroutine(Hungry(playerTurn));

            // check victory
            if (HasWin(playerTurn))
            {
                // set winner text
                winnerText.text = $"{playerTurn.data.name}  win !";

                // exit game loop
                break;
            }

            // check equality
            if (NoMoreTokens() ||
                turnWithoutEat > maxTurnWithoutEat)
            {
                // set winner text
                winnerText.text = "Equality";

                // exit game loop
                break;
            }

            // turn end
            playerTurn.nameTextAnimator.SetBool("playerTurn", false);

            // switch player turn
            playerTurn = playerTurn.opponent;
        }

        // turn end
        playerTurn.nameTextAnimator.SetBool("playerTurn", false);

        // active winner screen
        winnerScreen.SetActive(true);
    }

    IEnumerator Play(InGamePlayer inGamePlayer, Counter hole)
    {
        if (!inGamePlayer.board.Has(hole) ||
            hole.Value == 0)
            yield break;

        Counter startingHole = hole;

        // take token
        int tokens = startingHole.Value;
        startingHole.Value = 0;
        AudioSourceExtension.Play(takeTokensClip);

        // drop tokens
        while (tokens > 0)
        {
            yield return new WaitForSeconds(dtPutToken);

            hole = Next(hole);

            if (hole == startingHole)
                hole = Next(hole);

            tokens--;
            hole.Value++;

            AudioSourceExtension.Play(dropTokenClip, 1, 1 + Random.Range(-dropTokenPlusMinusPitch, dropTokenPlusMinusPitch));
        }

        // eat
        int eatCount = 0;

        while (!inGamePlayer.board.Has(hole) &&
            hole.Value >= eatMin &&
            hole.Value <= eatMax)
        {
            yield return new WaitForSeconds(dtEatToken);

            inGamePlayer.board.Score.Value += hole.Value;
            hole.Value = 0;
            eatCount++;

            AudioSourceExtension.Play(eatTokensClip, 1, 1 + eatCount * eatTokensDPitch);

            hole = Previous(hole);
        }

        if (eatCount > 0) turnWithoutEat = 0;
        else              turnWithoutEat++;
    }

    IEnumerator Hungry(InGamePlayer inGamePlayer)
    {
        int eatCount = 0;

        foreach (Counter hole in inGamePlayer.opponent.board.Holes)
        {
            if (hole.Value <= 0)
                continue;

            yield return new WaitForSeconds(dtEatToken);

            inGamePlayer.board.Score.Value += hole.Value;
            hole.Value = 0;
            eatCount++;

            AudioSourceExtension.Play(eatTokensClip, 1, 1 + eatCount * eatTokensDPitch);
        }
    }

    Counter Next(Counter hole, bool previous = false)
    {
        Board board = null;

        if      (b1.Has(hole)) board = b1;
        else if (b2.Has(hole)) board = b2;

        if (!board)
            return null;

        int holeIdx = board.Holes.IndexOf(hole);

        if (previous)
        {
            holeIdx--;

            if (holeIdx < 0)
            {
                board = board == b1 ? b2 : b1;
                holeIdx = board.Holes.Length - 1;
            }
        }

        else {
            holeIdx++;

            if (holeIdx >= board.Holes.Length)
            {
                board = board == b1 ? b2 : b1;
                holeIdx = 0;
            }
        }

        return board.Holes[holeIdx];
    }

    Counter Previous(Counter hole) => Next(hole, true);

    bool HasWin(InGamePlayer inGamePLayer) => inGamePLayer.board.Score.Value >= scoreMinWin;

    bool CanPlay(InGamePlayer inGamePLayer) => inGamePLayer.board.CountFilledHoles() > 0;

    bool NoMoreTokens()
    {
        foreach (Counter hole in b1.Holes.Concat(b2.Holes))
            if (hole.Value > 0)
                return false;

        return true;
    }
}

