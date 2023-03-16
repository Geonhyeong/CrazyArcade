using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegisterPacketReq
{
    public string AccountName;
    public string Password;
}

public class RegisterPacketRes
{
    public bool RegisterOk;
}

public class LoginPacketReq
{
    public string AccountName;
    public string Password;
}

public class LoginPacketRes
{
    public bool LoginOk;
    public int AccountId;
    public int Token;
}