using DummyClient;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    // step5
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
        ServerSession serverSession = (ServerSession)session;

        serverSession.Dir = enterGamePacket.Player.PosInfo.MoveDir;
        serverSession.PosX = enterGamePacket.Player.PosInfo.PosX;
        serverSession.PosY = enterGamePacket.Player.PosInfo.PosY;

        Console.WriteLine($"DummyClient_{serverSession.DummyId} enter Game");

        Map.Instance.Move(serverSession);
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
    }

    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeStat statPacket = packet as S_ChangeStat;
    }

    public static void S_PopHandler(PacketSession session, IMessage packet)
    {
        S_Pop popPacket = packet as S_Pop;
    }

    public static void S_TrapHandler(PacketSession session, IMessage packet)
    {
        S_Trap trapPacket = packet as S_Trap;
    }

    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;
    }

    // Step1
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        // 입장 Request (with Token, AccountDbId)
        C_Login loginPacket = new C_Login();
        ServerSession serverSession = (ServerSession)session;

        loginPacket.AccountDbId = -1;
        loginPacket.Token = -1;
        serverSession.Send(loginPacket);
    }

    public static void S_DisconnectedHandler(PacketSession session, IMessage packet)
    {
    }

    // Step2
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;
        ServerSession serverSession = (ServerSession)session;

        C_EnterRoom enterRoomPacket = new C_EnterRoom();
        enterRoomPacket.Nickname = $"DummyClient_{serverSession.DummyId.ToString("0000")}";
        enterRoomPacket.RoomCode = "TEST";
        serverSession.Send(enterRoomPacket);
    }

    // Step3
    public static void S_EnterRoomHandler(PacketSession session, IMessage packet)
    {
        S_EnterRoom enterRoomPacket = (S_EnterRoom)packet;
    }

    public static void S_RoomPlayerListHandler(PacketSession session, IMessage packet)
    {
    }

    public static void S_LeaveRoomHandler(PacketSession session, IMessage packet)
    {
    }

    // Step4
    public static void S_StartGameHandler(PacketSession session, IMessage packet)
    {
        ServerSession serverSession = (ServerSession)session;

        C_EnterGame enterGamePkt = new C_EnterGame();
        serverSession.Send(enterGamePkt);
    }

    public static void S_EndGameHandler(PacketSession session, IMessage packet)
    {
    }
}