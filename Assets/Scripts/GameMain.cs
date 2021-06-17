using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
public class GameMain : SingletonMonoBehaviour<GameMain>
{
    [SerializeField] private Text gameStateText = null;
    [SerializeField] private PieceManager pieceManager = null;
    [SerializeField] private AutoPlay auto = null;
    [SerializeField] private float timeScale = 1f;
    public enum GameState
    {
        Play = 0,
        Auto,
        GameOver,
        GameClear
    }
    private GameState currentState = GameState.Play;
    private void Start()
    {
        this.ObserveEveryValueChanged(_ => timeScale).
        Subscribe(x =>
        {
            Time.timeScale = x;
        });
        Time.timeScale = timeScale;
        pieceManager.Initialize();
        this.ObserveEveryValueChanged(_ => Input.GetKey(KeyCode.R)).
        Where(push => push).
        Subscribe(push =>
        {
            pieceManager.ReStert();
        });
        auto.Initialize();
        gameStateText.gameObject.SetActive(false);
    }
    public void ChangeGameState(GameState state)
    {
        if (state == currentState)
            return;
        switch (state)
        {
            case GameState.Play:
                break;
            case GameState.Auto:
                break;
            case GameState.GameClear:
                gameStateText.text = "GameClear";
                break;
            case GameState.GameOver:
                gameStateText.text = "GameOver";
                break;
        }
        gameStateText.gameObject.SetActive(state == GameState.GameClear || state == GameState.GameOver);
        currentState = state;
    }
}
