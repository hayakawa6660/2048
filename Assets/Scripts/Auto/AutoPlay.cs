using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
public class AutoPlay : MonoBehaviour
{
    [SerializeField] private AutoType nextAutoType = AutoType.None;
    [SerializeField] AutoBase[] autoList = null;
    [SerializeField] PieceManager pieceManager = null;
    public enum AutoType
    {
        Random = 0,
        Simple,
        None,
    }
    private AutoType currentAutoType = AutoType.None;
    public void Initialize()
    {
        foreach (var auto in autoList)
        {
            auto.Initialize(pieceManager);
        }
        ChangeType();
        this.ObserveEveryValueChanged(_ => nextAutoType).
        Subscribe(x =>
        {
            ChangeType();
        });
    }
    private void ChangeType()
    {
        if (currentAutoType == nextAutoType)
            return;
        if (currentAutoType != AutoType.None)
            autoList[(int)currentAutoType].gameObject.SetActive(false);
        if (nextAutoType != AutoType.None)
        {
            autoList[(int)nextAutoType].gameObject.SetActive(true);
            autoList[(int)nextAutoType].StartAutoPlay();
        }
        currentAutoType = nextAutoType;
    }
}
