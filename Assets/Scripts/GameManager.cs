using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

/// A Singleton Monobehaviour
public class GameManager : MonoBehaviour
{

    #region --------------------    Public Enumerations

    /// <summary>
    /// The available game states
    /// </summary>
    public enum GameState { Intro, About, Gameplay, Pause, ResultsWin, ResultsLose };

    /// <summary>
    /// The available minion lanes
    /// </summary>
    public enum Lane { Top, Mid, Bot };

    #endregion

    #region --------------------    Public Events



    #endregion

    #region --------------------    Public Properties

    /// Stores the singleton instance for the class
    public static GameManager instance { get; private set; } = null;

    /// <summary>
    /// Stores the current game state
    /// </summary>
    public static GameState state { get; private set; } = GameState.Intro;

    /// <summary>
    /// The lookup for each state and its applicable canvas group
    /// </summary>
    public static Dictionary<GameState, CanvasGroup> stateCanvas { get; private set; } = null;

    /// <summary>
    /// Returns the sound effect audio source
    /// </summary>
    public AudioSource sfx => _sfx;

    /// <summary>
    /// The public storage of the lane information
    /// </summary>
    public Dictionary<string, Dictionary<Lane, List<Tower>>> lanes = new Dictionary<string, Dictionary<Lane, List<Tower>>>();

    /// <summary>
    /// The remaining game time
    /// </summary>
    public float gameTimeRemaining { get; set; } = 360;

    #endregion

    #region --------------------    Public Methods

    /// <summary>
    /// Hides the provided canvas after the provided period of time
    /// </summary>
    /// <param name="_pGroup"></param>
    /// <param name="_pDuration"></param>
    public void HideCanvas(CanvasGroup _pGroup, System.Action _pCallback = null, float _pDuration = 0.25f)
    {
        _pGroup.blocksRaycasts = false;
        _pGroup.interactable = false;
        _pGroup.DOFade(0f, _pDuration)
            .SetUpdate(true)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => _pCallback?.Invoke());
    }

    /// <summary>
    /// Shows the provided canvas after the provided period of time
    /// </summary>
    /// <param name="_pGroup"></param>
    /// <param name="_pDuration"></param>
    public void ShowCanvas(CanvasGroup _pGroup, System.Action _pCallback, float _pDuration = 0.25f)
    {
        _pGroup.DOFade(1f, _pDuration)
            .SetUpdate(true)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _pGroup.blocksRaycasts = true;
                _pGroup.interactable = true;
                _pCallback?.Invoke();
            });
    }

    /// <summary>
    /// Sets the state to intro after closing the current canvas and opening the intro canvas
    /// </summary>
    public void GoToIntro() => HideCanvas(stateCanvas[state], () => ShowCanvas(stateCanvas[GameState.Intro], () => state = GameState.Intro));

    /// <summary>
    /// Sets the state to about after closing the current canvas and opening the about canvas
    /// </summary>
    public void GoToAbout() => HideCanvas(stateCanvas[state], () => ShowCanvas(stateCanvas[GameState.About], () => state = GameState.About));

    /// <summary>
    /// Sets the state to gameplay after closing the current canvas and opening the gameplay canvas
    /// </summary>
    public void GoToGameplay() => HideCanvas(stateCanvas[state], () => ShowCanvas(stateCanvas[GameState.Gameplay], RestartGame));

    /// <summary>
    /// Pauses the game
    /// </summary>
    public void Pause()
    {
        state = GameState.Pause;
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.1f, 0.1f)
            .OnComplete(() => { Time.timeScale = 0f; });
        ShowCanvas(_pauseCanvas, () => { });
    }

    /// <summary>
    /// Resumes the game
    /// </summary>
    public void Resume()
    {
        state = GameState.Gameplay;
        Time.timeScale = 0.1f;
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 0.1f);
        HideCanvas(_pauseCanvas, () => { });
    }

    /// <summary>
    /// Sets the state to results after closing the current canvas and opening the results canvas
    /// </summary>
    public void GoToResultsWin() => HideCanvas(stateCanvas[state], () => ShowCanvas(stateCanvas[GameState.ResultsWin], () => 
    {
        Time.timeScale = 0f;
        state = GameState.ResultsWin;
        _curtain.alpha = 1f;
    }));

    /// <summary>
    /// Sets the state to results after closing the current canvas and opening the results canvas
    /// </summary>
    public void GoToResultsLose() => HideCanvas(stateCanvas[state], () => ShowCanvas(stateCanvas[GameState.ResultsLose], () =>
    {
        Time.timeScale = 0f;
        state = GameState.ResultsLose;
        _curtain.alpha = 1f;
    }));

    /// <summary>
    /// Returns the next target tower based off of the provided lane
    /// </summary>
    /// <param name="_pLane"></param>
    /// <returns></returns>
    public iCombatable NextTower(string _pTag, Lane _pLane)
    {
        switch (_pTag)
        {
            case "EnemyTeam":
                switch (_pLane)
                {
                    case Lane.Top: return ((_topPlayerTowers.Count > 0) ? (iCombatable)_topPlayerTowers[0] : (iCombatable)_playerBase);
                    case Lane.Mid: return ((_midPlayerTowers.Count > 0) ? (iCombatable)_midPlayerTowers[0] : (iCombatable)_playerBase);
                    case Lane.Bot: return ((_botPlayerTowers.Count > 0) ? (iCombatable)_botPlayerTowers[0] : (iCombatable)_playerBase);
                }
                break;
            case "PlayerTeam":
                switch (_pLane)
                {
                    case Lane.Top: return ((_topAITowers.Count > 0) ? (iCombatable)_topAITowers[0] : (iCombatable)_aiBase);
                    case Lane.Mid: return ((_midAITowers.Count > 0) ? (iCombatable)_midAITowers[0] : (iCombatable)_aiBase);
                    case Lane.Bot: return ((_botAITowers.Count > 0) ? (iCombatable)_botAITowers[0] : (iCombatable)_aiBase);
                }
                break;
        }
        return null;
    }

    /// <summary>
    /// Quits the game
    /// </summary>
    public void QuitGame() => Application.Quit();

    /// <summary>
    /// Restarts the game
    /// </summary>
    public void RestartGame()
    {
        _curtain.DOFade(0f, 0.25f);
        state = GameState.Gameplay;
        gameTimeRemaining = 360f;
        Minion.allMinions.ForEach(m => m.Die());
        Tower.allTowers.ForEach(t => t.Restart());
        BaseTower.allBases.ForEach(b => b.Restart());
        _player.Spawn(_playerBase.playerSpawner, Lane.Mid);
        Time.timeScale = 1f;

    }

    #endregion

    #region --------------------	Protected Fields



    #endregion

    #region --------------------	Protected Methods



    #endregion

    #region --------------------    Private Fields

    [Header ("Singleton Configurations")]
    [SerializeField] private bool _isPersistent = false;

    [Header ("UI Configurations")]
    [SerializeField] private CanvasGroup _introCanvas = null;
    [SerializeField] private CanvasGroup _aboutCanvas = null;
    [SerializeField] private CanvasGroup _curtain = null;
    [SerializeField] private CanvasGroup _gameplayCanvas = null;
    [SerializeField] private CanvasGroup _pauseCanvas = null;
    [SerializeField] private CanvasGroup _resultsCanvasWin = null;
    [SerializeField] private CanvasGroup _resultsCanvasLose = null;

    [Header("Sound Configurations")]
    //[SerializeField] private AudioSource _bgComp = null;
    [SerializeField] private AudioSource _sfx = null;

    [Header("Game Configurations")]
    [SerializeField] private List<Tower> _topPlayerTowers = new List<Tower>();
    [SerializeField] private List<Tower> _midPlayerTowers = new List<Tower>();
    [SerializeField] private List<Tower> _botPlayerTowers = new List<Tower>();
    [SerializeField] private BaseTower _playerBase = null;
    [SerializeField] private PlayerBot _player = null;

    [SerializeField] private List<Tower> _topAITowers = new List<Tower>();
    [SerializeField] private List<Tower> _midAITowers = new List<Tower>();
    [SerializeField] private List<Tower> _botAITowers = new List<Tower>();
    [SerializeField] private BaseTower _aiBase = null;

    [SerializeField] private TMP_Text _clock = null;

    #endregion

    #region --------------------    Private Methods

    /// Used to perform configuration for the class
    private void Awake() => _SetSingleton();

    /// <summary>
    /// Sets up the lanes
    /// </summary>
    private void Start()
    {
        stateCanvas = new Dictionary<GameState, CanvasGroup>();
        stateCanvas.Add(GameState.Intro, _introCanvas);
        stateCanvas.Add(GameState.About, _aboutCanvas);
        stateCanvas.Add(GameState.Gameplay, _gameplayCanvas);
        stateCanvas.Add(GameState.Pause, _pauseCanvas);
        stateCanvas.Add(GameState.ResultsWin, _resultsCanvasWin);
        stateCanvas.Add(GameState.ResultsLose, _resultsCanvasLose);
        lanes.Add("PlayerTeam",
            new Dictionary<Lane, List<Tower>>()
            {
                { Lane.Bot, _botPlayerTowers },
                { Lane.Mid, _midPlayerTowers },
                { Lane.Top, _topPlayerTowers }
            });
        lanes.Add("EnemyTeam",
            new Dictionary<Lane, List<Tower>>()
            {
                { Lane.Bot, _botAITowers },
                { Lane.Mid, _midAITowers },
                { Lane.Top, _topAITowers }
            });
        Time.timeScale = 0f;
    }

    /// Sets the singleton for the class
    private void _SetSingleton()
    {
        instance = instance ?? this;
        if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        if (_isPersistent)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        switch (state)
        {
            case GameState.Gameplay:
                if (Input.GetKeyDown(KeyCode.Escape)) Pause();
                _clock.text = $"{Mathf.FloorToInt(gameTimeRemaining / 60)}:{(Mathf.FloorToInt(gameTimeRemaining % 60)).ToString("0#")}";
                gameTimeRemaining = Mathf.Max(gameTimeRemaining - Time.deltaTime, 0f);
                if (gameTimeRemaining == 0f)
                {
                    GoToResultsLose();
                }
                break;
            case GameState.Pause:
                if (Input.GetKeyDown(KeyCode.Escape)) Resume();
                break;
        }
    }

    #endregion

}