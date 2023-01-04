using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class InanimateController : MonoBehaviour
{
    public Vector3Int CellPos { get; set; } = Vector3Int.zero;
    protected Animator _animator;

    InanimateState _state = InanimateState.Idle;
    public InanimateState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
                return;

            _state = value;
            UpdateAnimation();
        }
    }

    void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        _animator = GetComponent<Animator>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.26f, 0.26f, Managers.Map.GetZ(CellPos));
        transform.position = pos;
    }

    protected virtual void UpdateAnimation()
    {
        switch (_state)
        {
            case InanimateState.Idle:
                _animator.Play("IDLE");
                break;
            case InanimateState.Pop:
                _animator.Play("POP");
                break;
        }
    }
}
