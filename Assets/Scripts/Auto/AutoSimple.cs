using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class AutoSimple : AutoBase
{
    /// <summary>ピースを集める方向</summary>
    PieceManager.MoveDirection priorityDirection = PieceManager.MoveDirection.RIGHT;
    PieceManager.MoveDirection nextDirection = PieceManager.MoveDirection.RIGHT;
    PieceManager.MoveDirection beforeDirection = PieceManager.MoveDirection.RIGHT;
    private int[,] beforeNumberArray = new int[PieceManager.Height, PieceManager.Width];
    private int[] directionScore = new int[4];
    bool notMove = false;

    private enum AIState
    {
        DefaultMove = 0,
        CanMoveNumScore,
        ChangePriorityDirection,
    }
    private AIState currentState = AIState.DefaultMove;
    public override IEnumerator Play()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetPieceMoveTime() + 0.05f);
            MovePiece(NextSelectBehaviour());
        }
    }
    public PieceManager.MoveDirection NextSelectBehaviour()
    {
        var direction = PieceManager.MoveDirection.UP;
        switch (currentState)
        {
            case AIState.DefaultMove:
                direction = DefaultMovement();
                break;
            case AIState.CanMoveNumScore:
                direction = CheckMaxScoreDirection();
                break;
            case AIState.ChangePriorityDirection:
                return default;
        }
        beforeNumberArray = GetPieceNumArray();
        beforeDirection = direction;
        return direction;
    }
    private PieceManager.MoveDirection DefaultMovement()
    {
        var next = beforeDirection == PieceManager.MoveDirection.RIGHT ? PieceManager.MoveDirection.DOWN : PieceManager.MoveDirection.RIGHT;
        bool[] canMoveDirection = new bool[4] { false, false, false, false };
        var piecesNumber = GetPieceNumArray();
        for (int h = 0; h < PieceManager.Height; h++)
        {
            for (int w = 0; w < PieceManager.Width; w++)
            {
                if (piecesNumber[h, w] == -1)
                    continue;
                for (int direc = 0; direc < canMoveDirection.Length; direc++)
                {
                    if (CheckCanMove((PieceManager.MoveDirection)direc, new PieceManager.PieceCellPosition(w, h)))
                    {
                        canMoveDirection[direc] = true;
                    }
                }
            }
        }
        if (canMoveDirection[(int)next])
        {
            return next;
        }
        bool checkLeftLineActive = piecesNumber[3, 3] != -1 && piecesNumber[2, 3] != -1 && piecesNumber[1, 3] != -1 && piecesNumber[0, 3] != -1;
        if (checkLeftLineActive)
        {
            if (canMoveDirection[(int)PieceManager.MoveDirection.UP])
            {
                return PieceManager.MoveDirection.UP;
            }
        }
        else
        {
            if (canMoveDirection[(int)PieceManager.MoveDirection.DOWN])
            {
                return PieceManager.MoveDirection.DOWN;
            }
            else if (canMoveDirection[(int)PieceManager.MoveDirection.UP])
            {
                return PieceManager.MoveDirection.UP;
            }
        }
        return PieceManager.MoveDirection.LEFT;
    }
    private PieceManager.MoveDirection CheckMaxScoreDirection()
    {
        for (int i = 0; i < directionScore.Length; i++)
        {
            directionScore[i] = 0;
        }

        var numList = GetPieceNumArray();
        var maxPieceNumber = 0;
        for (int h = 0; h < PieceManager.Height; h++)
        {
            for (int w = 0; w < PieceManager.Width; w++)
            {
                var pos = new PieceManager.PieceCellPosition(w, h);
                if (!CheckExceptionPosition(pos))
                {
                    var num = numList[pos.y, pos.x];
                    for (int i = 0; i < 4; i++)
                    {
                        var direction = GetNextPos((PieceManager.MoveDirection)i, pos);
                        if (!CheckExceptionPosition(direction) && num == numList[direction.y, direction.x])
                        {
                            if (maxPieceNumber <= num)
                            {
                                maxPieceNumber = num;
                            }
                            directionScore[i]++;
                        }
                    }
                }
            }
        }
        PieceManager.MoveDirection selectDirection = PieceManager.MoveDirection.UP;
        int maxScore = 0;
        for (int i = 0; i < directionScore.Length; i++)
        {
            //貯めている方向と反対には移動させない
            if (GetMirrorDirection(priorityDirection) == (PieceManager.MoveDirection)i)
                continue;
            if (directionScore[i] >= maxScore)
            {
                selectDirection = (PieceManager.MoveDirection)i;
                maxScore = directionScore[i];
            }
        }
        notMove = directionScore.Sum() == 0;
        return selectDirection;
    }
    private PieceManager.MoveDirection GetMirrorDirection(PieceManager.MoveDirection direction)
    {
        switch (direction)
        {
            case PieceManager.MoveDirection.UP:
                return PieceManager.MoveDirection.DOWN;
            case PieceManager.MoveDirection.DOWN:
                return PieceManager.MoveDirection.UP;
            case PieceManager.MoveDirection.LEFT:
                return PieceManager.MoveDirection.RIGHT;
            case PieceManager.MoveDirection.RIGHT:
                return PieceManager.MoveDirection.LEFT;
        }
        return default;
    }
    private PieceManager.PieceCellPosition GetNextPos(
            PieceManager.MoveDirection direction,
            PieceManager.PieceCellPosition pos)
    {
        return new PieceManager.PieceCellPosition(GetDirection(direction), pos);
    }
}
