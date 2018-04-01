using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class HandlePlayerMsg
{
    //开始战斗
    public void MsgStartFight(Player player, ProtocolBase protoBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartFight");
        //条件判断
        if (player.tempData.status != PlayerTempData.Status.InRoomReady)
        {
            Console.WriteLine("MsgStartFight status error " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        if (!player.tempData.isOwner)
        {
            Console.WriteLine("MsgStartFight Owner error " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        Room room = player.tempData.room;
        if (!room.CanStart())
        {
            Console.WriteLine("MsgStartFight CanStart error " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        //开始战斗
        protocol.AddInt(0);
        player.Send(protocol);
        //
        room.StartFight();
    }
    //非房主玩家准备
    public void MsgChangeRState(Player player,ProtocolBase protoBase)
    {
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        int start = 0;
        string protoname = protocol.GetString(start, ref start);
        int changestateprefabnum = protocol.GetInt(start, ref start);
        ProtocolBytes protocolret = new ProtocolBytes();
        protocolret.AddString("ChangeRState");
        protocolret.AddInt(changestateprefabnum);
        if (player.tempData.status != PlayerTempData.Status.InRoomNotReady &&
            player.tempData.status != PlayerTempData.Status.InRoomReady)
            return;
        Console.WriteLine("playersChangeStateNum " + changestateprefabnum);
        player.tempData.status = player.tempData.status==PlayerTempData.Status.InRoomReady?
            PlayerTempData.Status.InRoomNotReady: PlayerTempData.Status.InRoomReady;
        Room room = player.tempData.room;
        room.Broadcast(protocolret);
        
    }
    //房主玩家改变地图
    public void MsgSwitchMap(Player player,ProtocolBase protoBase)
    {
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        int start = 0;
        string protoname = protocol.GetString(start, ref start);
        int changetomap = protocol.GetInt(start, ref start);
        ProtocolBytes protocolret = new ProtocolBytes();
        protocolret.AddString("SwitchMap");
        protocolret.AddInt(changetomap);
        if (player.tempData.status != PlayerTempData.Status.InRoomNotReady &&
           player.tempData.status != PlayerTempData.Status.InRoomReady)
            return;

        Room room = player.tempData.room;
        Console.Write("OriginalMap " + room.MapType);
        room.MapType = changetomap;
        Console.WriteLine(" ChangesTo " + room.MapType);
        room.Broadcast(protocolret);
    }
    public void MsgESoilder(Player player,ProtocolBase protoBase)
    {
        float afterhealinghp = player.tempData.currentHp + 60;
        player.tempData.currentHp = Math.Min(afterhealinghp, player.tempData.maxHp);
    }
    //hit协议
    /*客户端发送enemyid，damage
      服务器广播 id enemyid damage*/
    public void MsgHit(Player player,ProtocolBase protoBase)
    {
        Console.WriteLine("recv hit");
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        string enemyID = protocol.GetString(start, ref start);
        float damage = protocol.GetFloat(start, ref start);
        /*
        //作弊校验
        if (enemyID != player.id)//非中毒情况
        {
            long lastShootTime = player.tempData.lastShootTime;
            if (Sys.GetTimeStamp() - lastShootTime < 1)
            {
                Console.WriteLine("MsgHit 开炮作弊 " + player.id);
                return;
            }
            player.tempData.lastShootTime = Sys.GetTimeStamp();
        }*/
        //获取房间
        if (player.tempData.status != PlayerTempData.Status.InGame)
            return;
        Room room = player.tempData.room;
        //扣除生命值
        if(!room.list.ContainsKey(enemyID))
        {
            Console.WriteLine("MsgHit 没有这样的敌人 " + enemyID);
            return;
        }
        Player enemy = room.list[enemyID];
        if (enemy == null|| enemy.tempData.currentHp <= 0)
            return;
        enemy.tempData.currentHp -= damage;
        Console.WriteLine("MsgHit " + enemyID + " hp" + enemy.tempData.currentHp);
        //广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("Hit");
        protocolRet.AddString(player.id);
        protocolRet.AddString(enemyID);
        protocolRet.AddFloat(damage);
        room.Broadcast(protocolRet);
        Console.WriteLine("send hit");
        //胜负判断
        room.UpdateWin();
    }
    public void MsgLeaveBattle(Player player,ProtocolBase protobase)
    {
        //获取数值
        if (player.tempData.status != PlayerTempData.Status.InGame)
            return;
        if(player.tempData.isOwner==true)
            player.tempData.status = PlayerTempData.Status.InRoomReady;
        else
            player.tempData.status = PlayerTempData.Status.InRoomNotReady;
        Room room = player.tempData.room;
        if (room != null)
        {
            Console.WriteLine("check leave battle");
            room.Broadcast(room.GetRoomInfo());
        }
    }
}
