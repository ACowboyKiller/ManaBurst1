using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// A Singleton Monobehaviour
public class GameManager : MonoBehaviour
{

    #region --------------------    Public Enumerations

    /// <summary>
    /// The available game states
    /// </summary>
    public enum GameState { Intro, About, Gameplay, Results };

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

    //  TODO:   RESET TO INTRO
    /// <summary>
    /// Stores the current game state
    /// </summary>
    public static GameState state { get; private set; } = GameState.Gameplay;

    /// <summary>
    /// The lookup for each state and its applicable canvas group
    /// </summary>
    public static Dictionary<GameState, CanvasGroup> stateCanvas { get; private set; } = null;

    /// <summary>
    /// Returns the sound effect audio source
    /// </summary>
    public AudioSource sfx => _sfx;

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
    public void GoToGameplay() => HideCanvas(stateCanvas[state], () => ShowCanvas(stateCanvas[GameState.Gameplay], () => state = GameState.Gameplay));

    /// <summary>
    /// Sets the state to results after closing the current canvas and opening the results canvas
    /// </summary>
    public void GoToResults() => HideCanvas(stateCanvas[state], () => ShowCanvas(stateCanvas[GameState.Results], () => state = GameState.Results));

    /// <summary>
    /// Returns the next target tower based off of the provided lane
    /// </summary>
    /// <param name="_pLane"></param>
    /// <returns></returns>
    public Tower NextTower(string _pTag, Lane _pLane)
    {
        switch (_pTag)
        {
            case "EnemyTeam":
                switch (_pLane)
                {
                    case Lane.Top: return ((_topPlayerTowers.Count > 0) ? _topPlayerTowers[0] : _playerBase);
                    case Lane.Mid: return ((_midPlayerTowers.Count > 0) ? _midPlayerTowers[0] : _playerBase);
                    case Lane.Bot: return ((_botPlayerTowers.Count > 0) ? _botPlayerTowers[0] : _playerBase);
                }
                break;
            case "PlayerTeam":
                switch (_pLane)
                {
                    case Lane.Top: return ((_topAITowers.Count > 0) ? _topAITowers[0] : _aiBase);
                    case Lane.Mid: return ((_midAITowers.Count > 0) ? _midAITowers[0] : _aiBase);
                    case Lane.Bot: return ((_botAITowers.Count > 0) ? _botAITowers[0] : _aiBase);
                }
                break;
        }
        return null;
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
    //[SerializeField] private CanvasGroup _introCanvas = null;
    //[SerializeField] private CanvasGroup _aboutCanvas = null;
    //[SerializeField] private CanvasGroup _gameplayCanvas = null;
    //[SerializeField] private CanvasGroup _resultsCanvas = null;

    [Header("Sound Configurations")]
    //[SerializeField] private AudioSource _bgComp = null;
    [SerializeField] private AudioSource _sfx = null;

    [Header("Game Configurations")]
    [SerializeField] private List<Tower> _topPlayerTowers = new List<Tower>();
    [SerializeField] private List<Tower> _midPlayerTowers = new List<Tower>();
    [SerializeField] private List<Tower> _botPlayerTowers = new List<Tower>();
    [SerializeField] private Tower _playerBase = null;

    [SerializeField] private List<Tower> _topAITowers = new List<Tower>();
    [SerializeField] private List<Tower> _midAITowers = new List<Tower>();
    [SerializeField] private List<Tower> _botAITowers = new List<Tower>();
    [SerializeField] private Tower _aiBase = null;

    #endregion

    #region --------------------    Private Methods

    /// Used to perform configuration for the class
    private void Awake() => _SetSingleton();

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

    #endregion

}