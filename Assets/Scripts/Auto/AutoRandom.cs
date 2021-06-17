using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRandom : AutoBase
{
    public override IEnumerator Play()
    {
        while (true)
        {
            MovePiece((PieceManager.MoveDirection)Random.Range(0, 4));
            yield return new WaitForSeconds(GetPieceMoveTime() + 0.05f);
        }
    }
}
