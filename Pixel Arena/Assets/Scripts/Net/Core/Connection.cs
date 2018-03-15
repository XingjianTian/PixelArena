using System;
using System.Linq;
using UnityEngine;
using System.Net.Sockets;
//异步socket，大体与服务器端Serv相同
public class Connection
{
    //缓冲区大小常量
    const int BUFFER_SIZE = 1024;
    //Socket
    private Socket socket;
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

    //连接服务端
    public bool Connect(string host,int port)
    {
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
}
