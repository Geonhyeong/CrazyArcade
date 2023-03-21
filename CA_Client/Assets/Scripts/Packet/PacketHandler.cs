using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;

        Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
        Managers.Object.RemoveMyPlayer();
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;
        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            Managers.Object.Add(obj, myPlayer: false);
        }
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int id in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
        }
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;

        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;

        // 서버에서 응답 패킷이 늦게와서 클라에서는 이미 이동했는데 다시 뒤로 가야하는 소위 가위 눌리는 현상 방지
        if (Managers.Object.MyPlayer.Id == movePacket.ObjectId)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc == null)
            return;

        cc.PosInfo = movePacket.PosInfo;
    }

    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeStat statPacket = packet as S_ChangeStat;

        GameObject go = Managers.Object.FindById(statPacket.PlayerId);
        if (go == null)
            return;

        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;

        pc.Speed = statPacket.StatInfo.SpeedLvl * 0.8f;
        pc.Power = statPacket.StatInfo.Power;
        pc.AvailBubble = statPacket.StatInfo.AvailBubble;
    }

    public static void S_PopHandler(PacketSession session, IMessage packet)
    {
        S_Pop popPacket = packet as S_Pop;

        GameObject go = Managers.Object.FindById(popPacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc == null)
            return;

        cc.State = CreatureState.Pop;
    }

    public static void S_TrapHandler(PacketSession session, IMessage packet)
    {
        S_Trap trapPacket = packet as S_Trap;

        GameObject go = Managers.Object.FindById(trapPacket.PlayerId);
        if (go == null)
            return;

        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;

        pc.State = CreatureState.Trap;
    }

    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;

        GameObject go = Managers.Object.FindById(diePacket.PlayerId);
        if (go == null)
            return;

        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;

        pc.State = CreatureState.Dead;
    }

    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        // 입장 Request (with Token, AccountDbId)
        C_Login loginPacket = new C_Login();
        loginPacket.AccountDbId = Managers.Network.AccountId;
        loginPacket.Token = Managers.Network.Token;
        Managers.Network.Send(loginPacket);
    }

    public static void S_DisconnectedHandler(PacketSession session, IMessage packet)
    {
        Managers.Scene.LoadScene(Define.Scene.Login);
        // TODO : disconnect 팝업 출력
    }

    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;
        
        if (loginPacket.LoginOk)
        {
            // 게임서버에 접속 성공 시 로비(또는 방선택화면)로 이동
            Managers.Scene.LoadScene(Define.Scene.Lobby);
        }
    }

    public static void S_EnterRoomHandler(PacketSession session, IMessage packet)
    {
        S_EnterRoom enterRoomPacket = (S_EnterRoom)packet;

        if (enterRoomPacket.EnterRoomOk)
        {
            Managers.Scene.LoadScene(Define.Scene.Room);
            // TODO
        }
    }
}