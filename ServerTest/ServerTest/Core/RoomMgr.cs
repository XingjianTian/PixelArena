using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RoomMgr
{
    //单例
    public static RoomMgr Instance
    {
        get {return Nested.instance; }
    }

    class Nested
    {
        static Nested() { }
        internal static readonly RoomMgr instance = new RoomMgr();
    }
    //房间列表
    public List<Room> list = new List<Room>();

    //创建房间
    public void CreateRoom(Player player)
    {
        Room room = new Room();
        lock(list)
        {
            list.Add(room);
            room.AddPlayer(player);
        }
    }
    //离开房间
    public void LeaveRoom(Player player)
    {
        var tempData = player.tempData;
        if (tempData.status == PlayerTempData.Status.None)
            return;
        Room room = tempData.room;
        lock(list)
        {
            room.DelPlayer(player.id);
            if (room.list.Count == 0)
                list.Remove(room);
        }
    }
    //输出房间列表
    public ProtocolBytes GetRoomList()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        int count = list.Count;
        //房间数量
        protocol.AddInt(count);
        foreach(Room room in list)
        {
            protocol.AddInt(room.list.Count);
            protocol.AddInt((int)room.status);
        }
        return protocol;
    }

}

