using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RegisterPacketReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class RegisterPacketRes
{
    public bool RegisterOk { get; set; }
}

public class LoginPacketReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}

public class LoginPacketRes
{
    public bool LoginOk { get; set; }
    public int AccountId { get; set; }
    public int Token { get; set; }
}