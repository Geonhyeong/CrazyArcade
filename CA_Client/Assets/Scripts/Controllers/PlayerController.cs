using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    Coroutine _coSkill;

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
                GetSkillInput();
                break;
        }

        base.UpdateController();
    }

    void GetDirInput()
    {
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
            Dir = MoveDir.None;
        }
    }

    void GetSkillInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _coSkill = StartCoroutine("CoStartBubble");
        }
    }

    IEnumerator CoStartBubble()
    {
        GameObject go = Managers.Resource.Instantiate("Inanimate/Bubble");
        BubbleController bc = go.GetComponent<BubbleController>();
        bc.CellPos = CellPos;
        bc.Host = gameObject;

        yield return new WaitForSeconds(0.5f);
        _coSkill = null;
    }
}
