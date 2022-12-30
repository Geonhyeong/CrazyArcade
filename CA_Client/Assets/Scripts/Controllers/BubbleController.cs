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
        State = InanimateState.Pop;
        Managers.Resource.Destroy(gameObject);
    }
}
