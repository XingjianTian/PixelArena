using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public partial class HandlePlayerMsg
{
    //获取房间列表
    public void MsgGetRoomList(Player player,ProtocolBase protoBase)
    {
        player.Send(RoomMgr.Instance.GetRoomList());
    }
    //创建房间
    public void MsgCreateRoom(Player player,ProtocolBase protoBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("CreateRoom");
        //条件检测
        if(player.tempData.status!=PlayerTempData.Status.None)
        {
            Console.WriteLine("MsgCreateRoom Fail " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        RoomMgr.Instance.CreateRoom(player);
        protocol.AddInt(0);
        player.Send(protocol);
        Console.WriteLine("MsgCreateRoom Ok" + player.id);
    }
    //加入房间
    public void MsgEnterRoom(Player player,ProtocolBase protoBase)
    {
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        int index = protocol.GetInt(start, ref start);
        Console.WriteLine("[收到MsgEnterRoom]" + player.id + " " + index);
        protocol = new ProtocolBytes();
        protocol.AddString("EnterRoom");
        //判断房间是否存在
        if(index<0||index>=RoomMgr.Instance.list.Count)
        {
            Console.WriteLine("MsgEnterRoom index error " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        Room room = RoomMgr.Instance.list[index];
        //判断房间状态
        if(room.status!=Room.Status.Prepare)
        {
            Console.WriteLine("MsgEnterRoom status error " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        //添加玩家
        if(room.AddPlayer(player))
        {
            room.Broadcast(room.GetRoomInfo());
            protocol.AddInt(0);
            player.Send(protocol);
        }
        else
        {
            Console.WriteLine("MsgEnterRoom maxPlayer error " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
        }
    }
    //获取房间信息
    public void MsgGetRoomInfo(Player player,ProtocolBase protoBase)
    {
        if(player.tempData.status!=PlayerTempData.Status.InRoom)
        {
            Console.WriteLine("MsgGetRoomInfo status error " + player.id);
            return;
        }
        Room room = player.tempData.room;
        player.Send(room.GetRoomInfo());
    }
    //离开房间
    public void MsgLeaveRoom(Player player,ProtocolBase protoBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("LeaveRoom");
        //条件检测
        if(player.tempData.status!=PlayerTempData.Status.InRoom)
        {
            Console.WriteLine("MsgLeaveRoom status error " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        //处理
        protocol.AddInt(0);
        player.Send(protocol);
        Room room = player.tempData.room;
        RoomMgr.Instance.LeaveRoom(player);
        //广播
        if (room != null)
            room.Broadcast(room.GetRoomInfo());
    }
}
