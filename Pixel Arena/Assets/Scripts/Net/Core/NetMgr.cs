using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class NetMgr
{
    public static Connection srvConn = new Connection();
    public static void Update()
    {
        srvConn.Update();
    }
    public static ProtocolBase GetHeartBeatProtocol()
    {
        //具体的发送内容根据服务器端端设定进行改动
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("HeartBeat");
        return protocol;
    }
}

