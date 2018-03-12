using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//玩家事件类，处理玩家上线下线等事件
public class HandlePlayerEvent
{
    //上线
    public void OnLogin(Player player)
    {
       //Scene.instance.AddPlayer(player.id);
    }
        //下线
    public void OnLogout(Player player)
    {
        //Scene.instance.DelPlayer(player.id);
        if(player.tempData.status==PlayerTempData.Status.InRoomNotReady||
            player.tempData.status == PlayerTempData.Status.InRoomReady)
        {
            var room = player.tempData.room;
            RoomMgr.Instance.LeaveRoom(player);
            if (room != null)
                room.Broadcast(room.GetRoomInfo());
        }
        //战斗中
        if(player.tempData.status==PlayerTempData.Status.InGame)
        {
            var room = player.tempData.room;
            room.ExitFight(player);
            RoomMgr.Instance.LeaveRoom(player);
        }
    }
}
