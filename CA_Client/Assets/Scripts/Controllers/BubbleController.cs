using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BubbleController : CreatureController
{
    protected override void Init()
    {
        base.Init();
        Type = GameObjectType.Bubble;
    }

    protected override void UpdateAnimation()
    {
        if (_animator == null)
            return;

        switch (State)
        {
            case CreatureState.Idle:
                _animator.Play("IDLE");
                break;

            case CreatureState.Pop:
                _animator.Play("POP");
                break;
        }
    }

    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Pop:
                UpdatePop();
                break;
        }
    }

    private void UpdateIdle()
    {

    }

    private void UpdatePop()
    {

    }

    /*public override void OnPop()
    {
        State = InanimateState.Pop;

        CreatureController cc = Host.GetComponent<CreatureController>();

        for (int power = cc._power; power > 0; power--)
        {   // up
            Vector3Int destPos = CellPos + Vector3Int.up * (cc._power - power + 1);
            if (Managers.Map.CanGo(destPos) == false)
                break;

            GameObject targetObject = Managers.Object.Find(destPos);
            if (targetObject != null)
            {
                InanimateController ic = targetObject.GetComponent<InanimateController>();
                if (ic != null)
                    ic.OnPop();
                break;
            }

            GameObject go = Managers.Resource.Instantiate("Inanimate/Wave");
            WaveController wc = go.GetComponent<WaveController>();

            wc.CellPos = destPos;
            wc.Dir = WaveDir.Up;
            wc.power = power;
            wc.Host = Host;
        }

        for (int power = cc._power; power > 0; power--)
        {   // down
            Vector3Int destPos = CellPos + Vector3Int.down * (cc._power - power + 1);
            if (Managers.Map.CanGo(destPos) == false)
                break;

            GameObject targetObject = Managers.Object.Find(destPos);
            if (targetObject != null)
            {
                InanimateController ic = targetObject.GetComponent<InanimateController>();
                if (ic != null)
                    ic.OnPop();
                break;
            }

            GameObject go = Managers.Resource.Instantiate("Inanimate/Wave");
            WaveController wc = go.GetComponent<WaveController>();

            wc.CellPos = destPos;
            wc.Dir = WaveDir.Down;
            wc.power = power;
            wc.Host = Host;
        }

        for (int power = cc._power; power > 0; power--)
        {   // left
            Vector3Int destPos = CellPos + Vector3Int.left * (cc._power - power + 1);
            if (Managers.Map.CanGo(destPos) == false)
                break;

            GameObject targetObject = Managers.Object.Find(destPos);
            if (targetObject != null)
            {
                InanimateController ic = targetObject.GetComponent<InanimateController>();
                if (ic != null)
                    ic.OnPop();
                break;
            }

            GameObject go = Managers.Resource.Instantiate("Inanimate/Wave");
            WaveController wc = go.GetComponent<WaveController>();

            wc.CellPos = destPos;
            wc.Dir = WaveDir.Left;
            wc.power = power;
            wc.Host = Host;
        }

        for (int power = cc._power; power > 0; power--)
        {   // right
            Vector3Int destPos = CellPos + Vector3Int.right * (cc._power - power + 1);
            if (Managers.Map.CanGo(destPos) == false)
                break;

            GameObject targetObject = Managers.Object.Find(destPos);
            if (targetObject != null)
            {
                InanimateController ic = targetObject.GetComponent<InanimateController>();
                if (ic != null)
                    ic.OnPop();
                break;
            }

            GameObject go = Managers.Resource.Instantiate("Inanimate/Wave");
            WaveController wc = go.GetComponent<WaveController>();

            wc.CellPos = destPos;
            wc.Dir = WaveDir.Right;
            wc.power = power;
            wc.Host = Host;
        }

        GameObject.Destroy(gameObject, 0.5f);
        //Managers.Object.Remove(gameObject);

        GameObject target = Managers.Object.Find(CellPos);
        if (target != null)
        {
            InanimateController ic = target.GetComponent<InanimateController>();
            if (ic != null)
                ic.OnPop();
        }
    }*/
}