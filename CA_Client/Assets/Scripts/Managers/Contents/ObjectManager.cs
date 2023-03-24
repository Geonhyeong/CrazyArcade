using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    private Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }

    public void Add(ObjectInfo info, bool myPlayer = false)
    {
        GameObjectType objectType = GetObjectTypeById(info.ObjectId);
        
        if (objectType == GameObjectType.Player)
        {
            if (myPlayer)
            {
                GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                MyPlayer = go.GetComponent<MyPlayerController>();
                MyPlayer.Id = info.ObjectId;
                MyPlayer.PosInfo = info.PosInfo;
                MyPlayer.Speed = 1.0f; // TEMP
                /*MyPlayer.Speed = info.StatInfo.SpeedLvl * 0.8f;
                MyPlayer.Power = info.StatInfo.Power;
                MyPlayer.AvailBubble = info.StatInfo.AvailBubble;*/
                MyPlayer.SyncPos();
            }
            else
            {
                GameObject go = Managers.Resource.Instantiate("Creature/Player");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                PlayerController pc = go.GetComponent<PlayerController>();
                pc.Id = info.ObjectId;
                pc.PosInfo = info.PosInfo;
                pc.Speed = 1.0f; // TEMP
                /*pc.Speed = info.StatInfo.SpeedLvl * 0.8f;
                pc.Power = info.StatInfo.Power;
                pc.AvailBubble = info.StatInfo.AvailBubble;*/
                pc.SyncPos();
            }
        }
        else if (objectType == GameObjectType.Block)
        {
            GameObject go = Managers.Resource.Instantiate($"Creature/{info.Name}");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            BlockController bc = go.GetComponent<BlockController>();
            bc.Id = info.ObjectId;
            bc.PosInfo = info.PosInfo;
            bc.SyncPos();
        }
        else if (objectType == GameObjectType.Bubble)
        {
            GameObject go = Managers.Resource.Instantiate("Creature/Bubble");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            BubbleController bc = go.GetComponent<BubbleController>();
            bc.Id = info.ObjectId;
            bc.PosInfo = info.PosInfo;
            bc.SyncPos();
        }
        else if (objectType == GameObjectType.Wave)
        {
            GameObject go = Managers.Resource.Instantiate("Creature/Wave");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            WaveController wc = go.GetComponent<WaveController>();
            wc.Id = info.ObjectId;
            wc.PosInfo = info.PosInfo;
            wc.IsEdge = info.IsEdge;
            wc.SyncPos();
        }
        else if (objectType == GameObjectType.Item)
        {
            GameObject go = Managers.Resource.Instantiate($"Creature/{info.Name}");
            go.name = $"Item_{info.ObjectId}";
            _objects.Add(info.ObjectId, go);

            ItemController ic = go.GetComponent<ItemController>();
            ic.Id = info.ObjectId;
            ic.PosInfo = info.PosInfo;
            ic.SyncPos();
        }
    }

    public void Remove(int id)
    {
        GameObject go = FindById(id);
        if (go == null)
            return;

        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public void RemoveMyPlayer()
    {
        if (MyPlayer == null)
            return;

        Remove(MyPlayer.Id);
        MyPlayer = null;
    }

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public GameObject Find(Vector3Int cellPos)
    {
        foreach (GameObject obj in _objects.Values)
        {
            CreatureController cc = obj.GetComponent<CreatureController>();
            if (cc == null)
                continue;

            if (cc.CellPos == cellPos)
                return obj;
        }

        return null;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in _objects.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }

        return null;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
            Managers.Resource.Destroy(obj);

        _objects.Clear();
    }
}