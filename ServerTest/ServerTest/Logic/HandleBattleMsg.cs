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
        if (player.tempData.status != PlayerTempData.Status.InRoom)
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
    //hit协议
    /*客户端发送enemyid，damage
      服务器广播 id enemyid damage*/
    public void MsgHit(Player player,ProtocolBase protoBase)
    {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        string enemyID = protocol.GetString(start, ref start);
        float damage = protocol.GetFloat(start, ref start);
        //作弊校验
        long lastShootTime = player.tempData.lastShootTime;
        if(Sys.GetTimeStamp()-lastShootTime<1)
        {
            Console.WriteLine("MsgHit 开炮作弊 " + player.id);
            return;
        }
        player.tempData.lastShootTime = Sys.GetTimeStamp();
        //更多作弊校验 略
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
        if (enemy == null)
            return;
        if (enemy.tempData.hp <= 0)
            return;
        enemy.tempData.hp -= damage;
        Console.WriteLine("MsgHit " + enemyID + " hp" + enemy.tempData.hp);
        //广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("Hit");
        protocolRet.AddString(player.id);
        protocolRet.AddString(enemyID);
        protocolRet.AddFloat(damage);
        room.Broadcast(protocolRet);
        //胜负判断
        room.UpdateWin();
    }
    //动画协议
    public void MsgAnim(Player player,ProtocolBase protobase)
    {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protobase;
        string protoName = protocol.GetString(start, ref start);
        string animname = protocol.GetString(start, ref start);
        if (player.tempData.status != PlayerTempData.Status.InGame)
            return;
        Room room = player.tempData.room;
        //广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("Anim");
        protocolRet.AddString(player.id);
        protocolRet.AddString(animname);
        room.Broadcast(protocolRet);
    }
    public void MsgLeaveBattle(Player player,ProtocolBase protobase)
    {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protobase;
        string protoName = protocol.GetString(start, ref start);
        if (player.tempData.status != PlayerTempData.Status.InGame)
            return;
        player.tempData.status = PlayerTempData.Status.InRoom;
    }
    public void MsgShooting(Player player,ProtocolBase protoBase)
    {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        float posX = protocol.GetFloat(start, ref start);
        float posY = protocol.GetFloat(start, ref start);
        float rotX = protocol.GetFloat(start, ref start);
        float rotY = protocol.GetFloat(start, ref start);
        float rotZ = protocol.GetFloat(start, ref start);
        int ifflip = protocol.GetInt(start, ref start);
        if (player.tempData.status != PlayerTempData.Status.InGame)
            return;
        Room room = player.tempData.room;
        //广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("Shooting");
        protocolRet.AddString(player.id);
        protocolRet.AddFloat(posX);
        protocolRet.AddFloat(posY);
        protocolRet.AddFloat(rotX);
        protocolRet.AddFloat(rotY);
        protocolRet.AddFloat(rotZ);
        protocolRet.AddInt(ifflip);
        room.Broadcast(protocolRet);
    }
    //同步报告角色单元信息
    public void MsgUpdateUnitInfo(Player player,ProtocolBase protoBase)
    {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        float posX = protocol.GetFloat(start, ref start);
        float posY = protocol.GetFloat(start, ref start);
        float rotX = protocol.GetFloat(start, ref start);
        float rotY = protocol.GetFloat(start, ref start);
        float rotZ = protocol.GetFloat(start, ref start);
        int ifFlip = protocol.GetInt(start, ref start);
        float speed = protocol.GetFloat(start, ref start);
        //获取房间
        if (player.tempData.status != PlayerTempData.Status.InGame)
            return;
        Room room = player.tempData.room;
        player.tempData.posX = posX;
        player.tempData.posY = posY;
        player.tempData.lastUpdateTime = Sys.GetTimeStamp();
        //广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("UpdateUnitInfo");
        protocolRet.AddString(player.id);
        protocolRet.AddFloat(posX);
        protocolRet.AddFloat(posY);
        protocolRet.AddFloat(rotX);
        protocolRet.AddFloat(rotY);
        protocolRet.AddFloat(rotZ);
        protocolRet.AddInt(ifFlip);
        protocolRet.AddFloat(speed);
        room.Broadcast(protocolRet);
    }
}
