using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : MonoBehaviour
{
    public int Id { get; set; }
    public GameObjectType Type { get; protected set; } = GameObjectType.None;

    protected Animator _animator;
    protected bool _updated = false;

    private PositionInfo _positionInfo = new PositionInfo();
    public PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            if (_positionInfo.Equals(value))
                return;

            CellPos = new Vector3Int(value.PosX, value.PosY, 0);
            State = value.State;
            Dir = value.MoveDir;
        }
    }
    #region PositionInfo
    public CreatureState State
    {
        get { return PosInfo.State; }
        set
        {
            if (PosInfo.State == value)
                return;

            PosInfo.State = value;
            UpdateAnimation();
            _updated = true;
        }
    }

    public MoveDir Dir
    {
        get { return PosInfo.MoveDir; }
        set
        {
            if (PosInfo.MoveDir == value)
                return;

            PosInfo.MoveDir = value;

            UpdateAnimation();
            _updated = true;
        }
    }

    public void SyncPos()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.26f, 0.26f, Managers.Map.GetZ(CellPos));
        transform.position = destPos;
    }

    public Vector3Int CellPos
    {
        get { return new Vector3Int(PosInfo.PosX, PosInfo.PosY, 0); }
        set
        {
            if (PosInfo.PosX == value.x && PosInfo.PosY == value.y)
                return;

            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            _updated = true;
        }
    }
    #endregion

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        UpdateController();
    }
    
    protected virtual void Init()
    {
        _animator = GetComponent<Animator>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.26f, 0.26f, Managers.Map.GetZ(CellPos));
        transform.position = pos;

        UpdateAnimation();
    }

    protected virtual void UpdateAnimation()
    {
    }

    protected virtual void UpdateController()
    {
    }
}