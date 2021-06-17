using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
public class Piece : MonoBehaviour
{
    [SerializeField] private Image pieceImage = null;
    [SerializeField] private Text numText = null;
    public PieceManager.PieceCellPosition myPosition { get; set; } = new PieceManager.PieceCellPosition();
    private int num = 0;
    public bool isOverlap { get; set; } = false;
    public bool isInactive { get; set; } = false;
    public Color nextColor = Color.green;
    private void Start()
    {
        this.ObserveEveryValueChanged(_ => number).Subscribe(x => numText.text = x.ToString());
    }
    public void Initialize(int num, Color color, PieceManager.PieceCellPosition myPosition = null)
    {
        if (myPosition != null)
        {
            this.myPosition = myPosition;
        }
        pieceImage.color = color;
        this.num = num;
    }
    public int number
    {
        get { return num; }
    }
    public void Move(Vector2 movePosition, float time)
    {
        RectTransform rect = this.transform as RectTransform;
        rect.DOLocalMove(movePosition, time).onComplete = () =>
        {
            if (isInactive)
            {
                gameObject.SetActive(false);
                isInactive = false;
            }
            else if (isOverlap)
            {
                num += num;//重なっていたら加算
                pieceImage.color = nextColor;
                if (num == PieceManager.clearPieceNum)
                {
                    GameMain.Instance.ChangeGameState(GameMain.GameState.GameClear);
                }
                isOverlap = false;
            }
        };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public bool CheckOverlap(int num)
    {
        return this.num == num && !isOverlap;
    }
    public void SetColor(Color color)
    {
        nextColor = color;
    }
}
