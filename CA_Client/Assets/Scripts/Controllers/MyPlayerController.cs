using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    private bool _moveKeyPressed = false;

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                GetSkillInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                GetSkillInput();
                break;
            case CreatureState.Dead:
                break;
            case CreatureState.Trap:
                GetDirInput();
                break;
        }
        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        if (_moveKeyPressed)
        {
            State = CreatureState.Moving;
            return;
        }
    }

    private void GetDirInput()
    {
        _moveKeyPressed = true;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            _moveKeyPressed = false;
        }
    }

    protected override void SetNextPos()
    {
        if (_moveKeyPressed == false)
        {
            if (State == CreatureState.Moving)
                State = CreatureState.Idle;
            CheckUpdatedFlag();
            return;
        }
   
        Vector3Int destPos = CellPos;

        switch (Dir)
        {
            case MoveDir.Up:
                destPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                destPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                destPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos += Vector3Int.right;
                break;
        }

        if (Managers.Map.CanGo(destPos))
        {
            GameObject obstacle = Managers.Object.Find(destPos);

            if (obstacle == null)
            {
                CellPos = destPos;
            }
            else
            {
                CreatureController cc = obstacle.GetComponent<CreatureController>();
                if (cc != null && cc.Type == GameObjectType.Player)
                {
                    CellPos = destPos;
                }
            }
            CellPos = destPos;
        }
        CheckUpdatedFlag();
    }

    void CheckUpdatedFlag()
    {
        if (_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }

    private void GetSkillInput()
    {
        if (_coSkillCooltime == null && Input.GetKeyDown(KeyCode.Space))
        {
            C_Skill skill = new C_Skill() { Info = new SkillInfo() { PosInfo = new PositionInfo() } };
            skill.Info.Power = Power;
            skill.Info.PosInfo.State = CreatureState.Idle;
            skill.Info.PosInfo.PosX = PosInfo.PosX;
            skill.Info.PosInfo.PosY = PosInfo.PosY;
            Managers.Network.Send(skill);

            _coSkillCooltime = StartCoroutine("CoInputCooltime", 0.2f);
        }
    }

    Coroutine _coSkillCooltime;
    IEnumerator CoInputCooltime(float time)
    {
        yield return new WaitForSeconds(time);
        _coSkillCooltime = null;
    }
}