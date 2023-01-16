using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    protected Coroutine _coSkill;

    protected override void Init()
    {
        base.Init();
    }

    private IEnumerator CoStartBubble()
    {
        GameObject go = Managers.Resource.Instantiate("Inanimate/Bubble");
        BubbleController bc = go.GetComponent<BubbleController>();
        bc.CellPos = CellPos;
        bc.Host = gameObject;

        yield return new WaitForSeconds(0.5f);
        _coSkill = null;
    }
}