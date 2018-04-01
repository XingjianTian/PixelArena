using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Debug = UnityEngine.Debug;

//异步socket，大体与服务器端Serv相同
class UdpState 
{ 
    public UdpClient u; 
    public IPEndPoint e; 
} 
public class Connection
{
    //缓冲区大小常量
    const int BUFFER_SIZE = 1024;
    //Socket
    public Socket socket;

    public UdpClient MyUdpClientInstance;

    public string local_remoteip;
    //buff
    private byte[] readBuff = new byte[BUFFER_SIZE];
    private int buffCount = 0;
    //粘包分包
    private Int32 msgLength = 0;
    private byte[] lenBytes = new byte[sizeof(Int32)];
    //协议
    public ProtocolBase proto;
    //心跳时间
    public float lastTickTime = 0;
    public float heartBeatTime = 30;
    //消息分发，随着update，依次读取
    public MsgDistribution msgDist = new MsgDistribution();

    //状态
    public enum Status
    {
        None,
        Connected
    };
    public Status status = Status.None;
    public bool udpcontinue = false;

    public void GetConnIndex(ProtocolBase protocol)
    {
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes) protocol;
        string protoName = proto.GetString(start, ref start);
        MultiBattle.Instance.connIndex = proto.GetInt(start, ref start);
        local_remoteip = proto.GetString(start, ref start);
        Debug.Log(local_remoteip);
    }
    //连接服务端
    public bool Connect(string host,int port)
    {
        NetMgr.srvConn.msgDist.AddListener("ConnIndex",GetConnIndex);
        try
        {
            //socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;//关闭Nagel算法
            //Connect
            socket.Connect(host, port);
            //BeginReceive 异步回调
            socket.BeginReceive(readBuff, buffCount, BUFFER_SIZE - buffCount, SocketFlags.None,
                ReceiveCb, readBuff);
            //Debug.Log("连接成功");
            //状态
            status = Status.Connected;
            return true;
        }
        catch (System.Exception e)
        {
            Debug.Log("连接失败： " + e.Message);
            return false;
        }
    }
    //关闭
    public bool Close()
    {
        try
        {
            NetMgr.srvConn.msgDist.DelListener("ConnIndex",GetConnIndex);
            socket.Close();
            return true;
        }
        catch (System.Exception e)
        {
            //Debug.Log("关闭失败： " + e.Message);
            return false;
        }
    }
    //异步回调
    private void ReceiveCb(IAsyncResult ar)
    {
        try
        {
            int count = socket.EndReceive(ar);
            buffCount = buffCount + count;
            ProcessData();
            socket.BeginReceive(readBuff, buffCount, BUFFER_SIZE - buffCount, SocketFlags.None,
                ReceiveCb,readBuff);

        }
        catch (System.Exception e)
        {
            status = Status.None;
        }
    }
    //消息处理
    private void ProcessData()
    {
        //粘包分包处理
        if(buffCount<sizeof(Int32))
            return;
        //包体长度
        Array.Copy(readBuff, lenBytes, sizeof(Int32));
        msgLength = BitConverter.ToInt32(lenBytes, 0);
        if(buffCount<msgLength+sizeof(Int32))
            return;
        //协议解码
        ProtocolBase protocol = proto.Decode(readBuff, sizeof(Int32), msgLength);
        lock (msgDist.msgList)//锁住
        {
            msgDist.msgList.Add(protocol);
        }
        //清除已处理的消息
        int count = buffCount - msgLength - sizeof(Int32);
        Array.Copy(readBuff, sizeof(Int32) + msgLength, readBuff, 0, count);
        buffCount = count;
        if (buffCount > 0)
            ProcessData();
    }

    //发送消息
    public bool Send(ProtocolBase protocol)
    {
        if(status!=Status.Connected)
        {
            return true;
        }
        byte[] b = protocol.Encode();
        byte[] length = BitConverter.GetBytes(b.Length);
        byte[] sendbuff = length.Concat(b).ToArray();
        socket.Send(sendbuff);
        
        return true;
    }
    public bool Send(ProtocolBase protocol,string cbName,MsgDistribution.Delegate cb)
    {
        if (status != Status.Connected)
            return false;
        msgDist.AddOnceListener(cbName, cb);
        return Send(protocol);

    }
    public bool Send(ProtocolBase protocol,MsgDistribution.Delegate cb)
    {
        string cbName = protocol.GetName();
        return Send(protocol,cbName, cb);
    }
    

    //心跳机制
    public void Update()
    {
        //消息
        msgDist.Update();
        //心跳
        if(status==Status.Connected)
        {
            if(Time.time-lastTickTime>heartBeatTime)
            {
                ProtocolBase protocol = NetMgr.GetHeartBeatProtocol();
                Send(protocol);
                lastTickTime = Time.time;
            }
        }
    }
    public static string GetIpAddress()
    {
        try
        {
            string HostName = Dns.GetHostName(); //得到主机名
            IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
            for (int i = 0; i < IpEntry.AddressList.Length; i++)
            {
                //从IP地址列表中筛选出IPv4类型的IP地址
                //AddressFamily.InterNetwork表示此IP为IPv4,
                //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return IpEntry.AddressList[i].ToString();
                }
            }
            return "";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return "";
        }
    }

    public void CheckTest()
    {
        //探测消息
        ProtocolBytes Testproto = new ProtocolBytes();
        Testproto.AddString("Test");
        Testproto.AddInt(MultiBattle.Instance.connIndex);
        udpSend(Testproto);
        udpSend(Testproto);
    }

    public void MyClose()
    {
        MyUdpClientInstance.Close();
    }
    public void CreateUdpClient()
    {
        //MyUdpClientInstance.
        MyUdpClientInstance = null;
        Debug.Log("Create UdpClient");
        var locateIp = IPAddress.Parse(GetIpAddress());
        //Debug.Log(locateIp);
        var locatePoint = new IPEndPoint(locateIp, 1235);
        MyUdpClientInstance = new UdpClient(locatePoint);
        
        CheckTest();
        /*
        var allow = new Thread(()=>
        MyUdpClientInstance.AllowNatTraversal(true));
        allow.Start();*/
        udpcontinue = true;
        //监听创建好后，就开始接收信息，并创建一个线程
        MyUdpClientInstance.BeginReceive(udpReceiveCb, null);
    }
    private void udpReceiveCb(IAsyncResult ar)
    {
        byte[] recBuffer;
        IPEndPoint remotePoint = new IPEndPoint(IPAddress.Any, 0);//远端IP
        while (udpcontinue)
        {
            try
            {
                recBuffer = MyUdpClientInstance.Receive(ref remotePoint);
                ProtocolBytes protocol = (ProtocolBytes)proto.Decode(recBuffer, sizeof(Int32), BitConverter.ToInt32(recBuffer, 0));
                int start = 0;
                int count = protocol.GetInt(start, ref start);
                int frame = protocol.GetInt(start, ref start);
                if (MultiBattle.Instance.gameframe > frame)
                {
                    continue;
                }
                    
                for (int i = 0; i < count; ++i)
                {
                    string id = protocol.GetString(start, ref start);
                    if (!MultiBattle.Instance.list.ContainsKey(id)) //场景中没有该玩家
                    {
                        Debug.Log("list not contain");
                        throw new Exception();
                    }
                    string ops = protocol.GetString(start, ref start);
                    
                    if(ops[0]=='1')
                        Debug.Log("recv left");
                    if(ops[1]=='1')
                        Debug.Log("recv right");
                    int[] op = new int[5];
                    for (int j = 0; j < 5; ++j)
                        op[j] = ops[j] - '0';
                    Loom.RunAsync(() =>
                    {
                        //回到unity线程继续运行,单线程!
                        Loom.QueueOnMainThread((object a) =>
                        {
                            MultiBattle.Instance.list[id].Player.ProcessOps(op);
                            ++MultiBattle.Instance.gameframe;
                        }, null);
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        MyUdpClientInstance.EndReceive(ar, ref remotePoint);
        Debug.Log("EndReceive");
        MyClose();
        //MyUdpClientInstance = null;
    }

    public void udpSend(ProtocolBytes protocol)
    {
        try
        {
            byte[] bytes = protocol.Encode(); //编码
            byte[] length = BitConverter.GetBytes(bytes.Length);
            byte[] sendbuff = length.Concat(bytes).ToArray();
            MyUdpClientInstance.Send(sendbuff, sendbuff.Length, socket.RemoteEndPoint as IPEndPoint);
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}
