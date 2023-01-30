using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    [SerializeField]
    public float Speed { get; set; } = 1.0f;
    public int Power { get; set; } = 1;
    public int MaxBubble { get; set; } = 1;

    protected Coroutine _coSkill;
    
    protected override void Init()
    {
        base.Init();
        Type = GameObjectType.Player;
    }

    protected override void UpdateAnimation()
    {
        if (_animator == null)
            return;

        if (State == CreatureState.Idle)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    _animator.Play("IDLE_UP");
                    break;

                case MoveDir.Down:
                    _animator.Play("IDLE_DOWN");
                    break;

                case MoveDir.Left:
                    _animator.Play("IDLE_LEFT");
                    break;

                case MoveDir.Right:
                    _animator.Play("IDLE_RIGHT");
                    break;
            }
        }
        else if (State == CreatureState.Moving)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    _animator.Play("WALK_UP");
                    break;

                case MoveDir.Down:
                    _animator.Play("WALK_DOWN");
                    break;

                case MoveDir.Left:
                    _animator.Play("WALK_LEFT");
                    break;

                case MoveDir.Right:
                    _animator.Play("WALK_RIGHT");
                    break;
            }
        }
        else if (State == CreatureState.Dead)
        {
            _animator.Play("DIE");
        }
        else if (State == CreatureState.Trap)
        {
            _animator.Play("TRAP");
        }
    }

    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Moving:
                UpdateMoving();
                break;
            case CreatureState.Dead:
                UpdateDead();
                break;
            case CreatureState.Trap:
                UpdateTrap();
                break;
        }
    }

    protected virtual void UpdateIdle()
    {
    }

    protected virtual void UpdateMoving()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.26f, 0.26f, Managers.Map.GetZ(CellPos));
        Vector3 moveDir = destPos - transform.position;

        // 도착 여부 체크
        float dist = moveDir.magnitude;
        if (dist < Time.deltaTime * Speed)
        {
            transform.position = destPos;
            SetNextPos();
        }
        else
        {
            transform.position += moveDir.normalized * Time.deltaTime * Speed;
            State = CreatureState.Moving;
        }
    }
    
    protected virtual void UpdateDead()
    {
    }

    protected virtual void UpdateTrap()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.26f, 0.26f, Managers.Map.GetZ(CellPos));
        Vector3 moveDir = destPos - transform.position;

        // 도착 여부 체크
        float dist = moveDir.magnitude;
        if (dist < Time.deltaTime * 0.5f)
        {
            transform.position = destPos;
            SetNextPos();
        }
        else
        {
            transform.position += moveDir.normalized * Time.deltaTime * 0.5f;
        }
    }

    protected virtual void SetNextPos()
    {
    }

    /*public void UseSkill(SkillInfo info)
    {
        //_coSkill = StartCoroutine("CoStartBubble");
        Debug.Log("Make Bubble!");
        
    }

    private IEnumerator CoStartBubble()
    {
        *//*GameObject go = Managers.Resource.Instantiate("Inanimate/Bubble");
        BubbleController bc = go.GetComponent<BubbleController>();
        bc.CellPos = CellPos;
        bc.Host = gameObject;

        yield return new WaitForSeconds(0.5f);
        _coSkill = null;*//*
        yield return new WaitForSeconds(0.5f);
    }*/
}