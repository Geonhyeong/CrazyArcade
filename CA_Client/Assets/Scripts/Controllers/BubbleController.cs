using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BubbleController : InanimateController
{
    public GameObject Host { get; set; }

    protected override void Init()
    {
        base.Init();
        StartCoroutine("CoLifeTime");
    }

    IEnumerator CoLifeTime()
    {
        yield return new WaitForSeconds(3.5f);
        OnPop();
    }

    void OnPop()
    {
        State = InanimateState.Pop;

        CreatureController cc = Host.GetComponent<CreatureController>();

        for (int power = cc._power; power > 0; power--)
        {   // up
            Vector3Int destPos = CellPos + Vector3Int.up * (cc._power - power + 1);
            if (Managers.Map.CanGo(destPos) == false)
                break;

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

            GameObject go = Managers.Resource.Instantiate("Inanimate/Wave");
            WaveController wc = go.GetComponent<WaveController>();

            wc.CellPos = destPos;
            wc.Dir = WaveDir.Right;
            wc.power = power;
            wc.Host = Host;
        }

        GameObject.Destroy(gameObject, 0.5f);
    }
}
