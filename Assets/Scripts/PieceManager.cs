using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using System.Linq;
public class PieceManager : MonoBehaviour
{
    public const int clearPieceNum = 2048;
    /// <summary>幅</summary>
    public const int Width = 4;
    /// <summary>高さ</summary>
    public const int Height = 4;
    /// <summary>ピースが移動する時間</summary>
    [SerializeField] public float moveTime { get; } = 0.2f;
    /// <summary>最初に配置するピースの数</summary>
    [SerializeField] private int initializeCreatePieceNum = 3;
    [SerializeField] private float create4PieceProbability = 0.3f;
    /// <summary>
    /// ピースの位置（配列番号）
    /// </summary>
    public class PieceCellPosition
    {
        public int x = 0;
        public int y = 0;
        public PieceCellPosition(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }
        public PieceCellPosition(PieceCellPosition pos1, PieceCellPosition pos2)
        {
            this.x = pos1.x + pos2.x;
            this.y = pos1.y + pos2.y;
        }
    }
    /// <summary>ピースを移動させる枠のRectTransform</summary>
    [SerializeField] RectTransform piecesAreaTransform = null;
    /// <summary>ピースのプレファブ</summary>
    [SerializeField] Piece piecePrefab = null;
    /// <summary>ピースを移動させるローカルポジション</summary>
    Vector2[,] piecesAreas = new Vector2[Height, Width];
    /// <summary>盤上のピース（非アクティブも含む）</summary>
    private Piece[,] pieces = new Piece[Height, Width];

    /// <summary>非アクティブのピースリスト</summary>
    private List<Piece> isInactivePiece = new List<Piece>();
    public int[,] piecesNumber { get; private set; } = new int[Height, Width];
    private bool nowMoving = false;
    public enum MoveDirection
    {
        UP = 0,
        DOWN,
        RIGHT,
        LEFT
    }

    public void Initialize()
    {
        //ピースを移動させるローカルポジションを格納
        for (int i = 0; i < piecesAreaTransform.childCount; i++)
        {
            var rect = piecesAreaTransform.GetChild(i).transform as RectTransform;
            int areaX = (i % Height);
            int areaY = (i / Width);
            piecesAreas[areaY, areaX] = rect.localPosition;
        }
        //ピースを生成
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                pieces[h, w] = Instantiate(piecePrefab, this.transform);
                pieces[h, w].gameObject.SetActive(false);
            }
        }
        SetKey();
        ReStert();
        UpdatePieceNumArray();
    }
    public void ReStert()
    {
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                pieces[h, w].Initialize(2, GetColor(2), new PieceCellPosition(w, h));
                pieces[h, w].gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < initializeCreatePieceNum; i++)
        {
            SetRandomPiece();
        }
        GameMain.Instance.ChangeGameState(GameMain.GameState.Play);
    }
    /// <summary>
    /// キーを設定
    /// </summary>
    private void SetKey()
    {
        this.ObserveEveryValueChanged(_ => Input.GetKey(KeyCode.UpArrow)).
        Where(push => push).
        Subscribe(push =>
        {
            AllMovePiece(MoveDirection.UP);
        });
        this.ObserveEveryValueChanged(_ => Input.GetKey(KeyCode.DownArrow)).
        Where(push => push).
        Subscribe(push =>
        {
            AllMovePiece(MoveDirection.DOWN);
        });
        this.ObserveEveryValueChanged(_ => Input.GetKey(KeyCode.LeftArrow)).
        Where(push => push).
        Subscribe(push =>
        {
            AllMovePiece(MoveDirection.LEFT);
        });
        this.ObserveEveryValueChanged(_ => Input.GetKey(KeyCode.RightArrow)).
        Where(push => push).
        Subscribe(push =>
        {
            AllMovePiece(MoveDirection.RIGHT);
        });
    }
    /// <summary>
    /// 開いたマスでランダムな場所にピースを設定する
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private void SetRandomPiece()
    {
        //非アクティブのピースを探す
        isInactivePiece.Clear();
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                if (!pieces[h, w].gameObject.activeSelf)
                    isInactivePiece.Add(pieces[h, w]);
            }
        }
        int randomIndex = Random.Range(0, isInactivePiece.Count - 1);
        var setPiece = isInactivePiece[randomIndex];
        RectTransform rect = setPiece.transform as RectTransform;
        //ローカル位置をセット
        rect.localPosition = piecesAreas[setPiece.myPosition.y, setPiece.myPosition.x];
        //ランダムな値をセット
        var randomNum = CreateRandomPieceNum();
        setPiece.Initialize(randomNum, GetColor(randomNum));
        setPiece.gameObject.SetActive(true);
    }
    /// <summary>
    /// デバッグ用
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void SetPiece(int x, int y)
    {
        var setCell = new PieceCellPosition(x, y);
        pieces[y, x].Initialize(2, GetColor(2));
        pieces[y, x].myPosition = setCell;
        RectTransform rect = pieces[y, x].transform as RectTransform;
        rect.localPosition = piecesAreas[y, x];
        pieces[y, x].gameObject.SetActive(true);
    }
    private void ChangePieceArray(PieceCellPosition pos1, PieceCellPosition pos2)
    {
        var temp = pieces[pos1.y, pos1.x];
        pieces[pos1.y, pos1.x] = pieces[pos2.y, pos2.x];
        pieces[pos2.y, pos2.x] = temp;
        pieces[pos2.y, pos2.x].myPosition = pos2;
        pieces[pos1.y, pos1.x].myPosition = pos1;
    }
    /// <summary>
    /// ランダムなピース情報を生成
    /// </summary>
    /// <param name="GetRandomPieceInfo("></param>
    /// <returns></returns>
    private int CreateRandomPieceNum()
    {
        int randomNum = Random.Range(1, 3) * 2;
        int resultValue = Random.value < create4PieceProbability ? 4 : 2;
        return resultValue;
    }
    /// <summary>
    /// 全てのピースを指定方向に移動
    /// </summary>
    /// <param name="direction"></param>
    public bool AllMovePiece(MoveDirection direction)
    {
        if (nowMoving)
            return false;
        bool checkMove = false;
        switch (direction)
        {
            case MoveDirection.UP:
            case MoveDirection.LEFT:
                for (int h = 0; h < Height; h++)
                {
                    for (int w = 0; w < Width; w++)
                    {
                        if (MovePiece(direction, w, h))
                        {
                            checkMove = true;
                        }
                    }
                }
                break;
            case MoveDirection.DOWN:
            case MoveDirection.RIGHT:
                for (int h = Height - 1; h >= 0; h--)
                {
                    for (int w = Width - 1; w >= 0; w--)
                    {
                        if (MovePiece(direction, w, h))
                        {
                            checkMove = true;
                        }
                    }
                }
                break;
        }

        if (checkMove)
        {
            nowMoving = true;
            Observable.FromCoroutine(DelayCreatePiece).
            Subscribe(_ =>
            {
                nowMoving = false;
            }).AddTo(this);
        }
        return checkMove;
    }
    private void CheckGameOver()
    {
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                if (!pieces[h, w].gameObject.activeSelf)
                    return;
            }
        }

        //全方向動かない
        if (!CheckAllCanMoveOverlapPiece())
        {
            GameMain.Instance.ChangeGameState(GameMain.GameState.GameOver);
        }
    }
    /// <summary>
    /// ピース単体を移動
    /// </summary>
    /// <param name="direction">移動方向</param>
    /// <param name="x">移動させるピースX</param>
    /// <param name="y">移動させるピースY</param>
    /// <param name="checkCanMove">実際には移動させず移動できるかのみ確認</param>
    private bool MovePiece(MoveDirection direction, int x, int y)
    {
        var movePiece = pieces[y, x];
        if (!movePiece.gameObject.activeSelf)
            return false;
        //移動後の位置取得
        var movedSell = GetMovedPosition(direction, movePiece.myPosition);
        //配列のデータとmyPositionを入れ替える
        ChangePieceArray(movedSell, movePiece.myPosition);
        //次のマスで重なるか確認
        var checkOverlapNextPos = new PieceCellPosition(GetDirectionPos(direction), movedSell);

        bool overlapFlag = !CheckException(checkOverlapNextPos) &&//例外チェック
            pieces[checkOverlapNextPos.y, checkOverlapNextPos.x].gameObject.activeSelf &&
            pieces[checkOverlapNextPos.y, checkOverlapNextPos.x].CheckOverlap(movePiece.number);
        movedSell = overlapFlag ? checkOverlapNextPos : movedSell;

        movePiece.Move(GetLocalPosition(movedSell), moveTime);
        if (overlapFlag)
        {
            var overlapPiece = pieces[checkOverlapNextPos.y, checkOverlapNextPos.x];
            overlapPiece.SetColor(GetColor(overlapPiece.number * 2));
            overlapPiece.isOverlap = true;
            movePiece.isInactive = true;
        }

        return movedSell.x != x || movedSell.y != y;
    }
    /// <summary>
    /// 上下左右を確認して動けるか調べる
    /// </summary>
    /// <returns></returns>
    public bool CheckAllCanMoveOverlapPiece()
    {
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                var pos = new PieceCellPosition(w, h);
                if (!CheckException(pos) &&
                    (CheckCanMovePiece(MoveDirection.UP, pos) ||
                    CheckCanMovePiece(MoveDirection.DOWN, pos) ||
                    CheckCanMovePiece(MoveDirection.LEFT, pos) ||
                    CheckCanMovePiece(MoveDirection.RIGHT, pos)))
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool CheckCanMovePiece(MoveDirection direction, PieceCellPosition checkPos)
    {
        var movedPos = new PieceCellPosition(GetDirectionPos(direction), checkPos);
        return !CheckException(movedPos) &&
        (piecesNumber[movedPos.y, movedPos.x] == -1 ||
         piecesNumber[checkPos.y, checkPos.x] == piecesNumber[movedPos.y, movedPos.x]);
    }
    /// <summary>
    /// ピースの移動後の位置を返す(再起関数)
    /// </summary>
    /// <param name="direction">移動方向</param>
    /// <param name="position">現在位置</param>
    /// <returns></returns>
    private PieceCellPosition GetMovedPosition(MoveDirection direction, PieceCellPosition position)
    {
        var movedPos = new PieceCellPosition(position, GetDirectionPos(direction));
        if (!CheckException(movedPos) &&
        (!pieces[movedPos.y, movedPos.x].gameObject.activeSelf ||
          pieces[movedPos.y, movedPos.x].isInactive))//既に非アクティブになる予定
        {
            position = GetMovedPosition(direction, movedPos);
        }
        return position;
    }
    public PieceCellPosition GetDirectionPos(MoveDirection direction)
    {
        PieceCellPosition result = new PieceCellPosition();
        switch (direction)
        {
            case MoveDirection.UP: result.y--; break;
            case MoveDirection.DOWN: result.y++; break;
            case MoveDirection.LEFT: result.x--; break;
            case MoveDirection.RIGHT: result.x++; break;
        }
        return result;
    }
    /// <summary>
    /// カンバス上のローカルポジションを返す
    /// </summary>
    /// <param name="pos">現在位置</param>
    /// <returns></returns>
    private Vector2 GetLocalPosition(PieceCellPosition pos)
    {
        return piecesAreas[pos.y, pos.x];
    }
    /// <summary>
    /// マスからはみ出していないか
    /// </summary>
    /// <param name="checkPos"></param>
    /// <returns></returns>
    public bool CheckException(PieceCellPosition checkPos)
    {
        return (checkPos.x < 0) ||
                (checkPos.x > Width - 1) ||
                (checkPos.y < 0) ||
                (checkPos.y > Height - 1);
    }
    private IEnumerator DelayCreatePiece()
    {
        yield return new WaitForSeconds(moveTime + 0.05f);
        SetRandomPiece();
        UpdatePieceNumArray();
        CheckGameOver();
    }
    /// <summary>
    /// ピースの数字の配列コピー
    /// </summary>
    private void UpdatePieceNumArray()
    {
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                piecesNumber[h, w] = pieces[h, w].gameObject.activeSelf ? pieces[h, w].number : -1;
            }
        }
    }
    public Color GetColor(int num)
    {
        Color color = new Color();
        switch (num % 2048)
        {
            case 2:
                color = Colors.Green;
                break;
            case 4:
                color = Colors.Aqua;
                break;
            case 8:
                color = Colors.Bisque;
                break;
            case 16:
                color = Colors.Coral;
                break;
            case 32:
                color = Colors.DarkSalmon;
                break;
            case 64:
                color = Colors.LawnGreen;
                break;
            case 128:
                color = Colors.MidnightBlue;
                break;
            case 256:
                color = Colors.LightGoldenodYellow;
                break;
            case 512:
                color = Colors.SeaGreen;
                break;
            case 1024:
                color = Colors.Teal;
                break;
            case 2048:
                color = Colors.Gold;
                break;
        }
        return color;
    }
}
