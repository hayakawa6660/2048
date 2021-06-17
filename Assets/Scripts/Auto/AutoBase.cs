using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AutoBase : MonoBehaviour
{
    private PieceManager pieceManager = null;
    protected Coroutine autoCoroutine = null;
    public void Initialize(PieceManager pieceManager)
    {
        this.pieceManager = pieceManager;
    }
    public abstract IEnumerator Play();
    public void StartAutoPlay()
    {
        if (pieceManager != null)
            autoCoroutine = StartCoroutine(Play());
    }
    private void OnDisable()
    {
        if (autoCoroutine != null)
        {
            StopCoroutine(autoCoroutine);
        }
    }
    protected void MovePiece(PieceManager.MoveDirection direction)
    {
        pieceManager.AllMovePiece(direction);
    }
    protected int[,] GetPieceNumArray()
    {
        return pieceManager.piecesNumber;
    }
    protected float GetPieceMoveTime()
    {
        return pieceManager.moveTime;
    }
    protected PieceManager.PieceCellPosition GetDirection(PieceManager.MoveDirection direction)
    {
        return pieceManager.GetDirectionPos(direction);
    }
    protected bool CheckExceptionPosition(PieceManager.PieceCellPosition checkPos)
    {
        return pieceManager.CheckException(checkPos);
    }
    protected bool CheckCanMove(PieceManager.MoveDirection direction, PieceManager.PieceCellPosition checkPos)
    {
        return pieceManager.CheckCanMovePiece(direction, checkPos);
    }
}
