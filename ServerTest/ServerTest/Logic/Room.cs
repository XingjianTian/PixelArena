
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Room
{
    //状态
    public enum Status
    {
        Prepare=1,
        InGame=2,
    }
    public Status status = Status.Prepare;
    //玩家
    public int MaxPlayers = 6;
    public Dictionary<string, Player> list = new Dictionary<string, Player>();

    //添加玩家
    public bool AddPlayer(Player player)
    {
        lock(list)
        {
            if(list.Count>=MaxPlayers)
                return false;
            PlayerTempData tempData = player.tempData;
            tempData.room = this;
            tempData.team = SwitchTeam();
            tempData.status = PlayerTempData.Status.InRoom;
            if (list.Count == 0)
                tempData.isOwner = true;
            string id = player.id;
            list.Add(id, player);
        }
        return true;
    }

    //分配队伍
    public int SwitchTeam()
    {
        int count1 = 0;
        int count2 = 0;
        foreach (Player player in list.Values)
        {
            if (player.tempData.team == 1) count1++;
            if (player.tempData.team == 2) count2++;
        }
        if (count1 <= count2)
            return 1;
        else
            return 2;
    }
    //删除玩家
    public void DelPlayer(string id)
    {
        lock(list)
        {
            if (!list.ContainsKey(id))
                return;
            bool isOwner = list[id].tempData.isOwner;
            list[id].tempData.status = PlayerTempData.Status.None;
            list.Remove(id);
            if (isOwner)
                UpdateOwner();
        }
    }
    //更换房主
    public void UpdateOwner()
    {
        lock(list)
        {
            if (list.Count <= 0)
                return;
            foreach (Player player in list.Values)
            {
                player.tempData.isOwner = false;
            }
            Player p = list.Values.First();
            p.tempData.isOwner = true;
        }
    }

    //广播消息
    public void Broadcast(ProtocolBase protocol)
    {
        foreach (Player player in list.Values)
        {
            player.Send(protocol);
        }
    }
    //输出房间信息
    public ProtocolBytes GetRoomInfo()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomInfo");
        //房间信息
        protocol.AddInt(list.Count);
        //每个玩家的消息
        foreach (Player p in list.Values)
        {
            protocol.AddString(p.id);
            protocol.AddInt(p.tempData.team);
            protocol.AddFloat(p.data.kd);
            int isOwner = p.tempData.isOwner ? 1 : 0;
            protocol.AddInt(isOwner);
            int state = -1 ;
            switch (p.tempData.status)
            {
                case PlayerTempData.Status.None: state = 0; break;
                case PlayerTempData.Status.InRoom:state = 1;break;
                case PlayerTempData.Status.InGame:state = 2;break;
            }
            protocol.AddInt(state);
        }
        return protocol;
    }

    //房间能否开战
    public bool CanStart()
    {
        if (status != Status.Prepare)
            return false;
        int count1 = 0;
        int count2 = 0;
        foreach (Player player in list.Values)
        {
            if (player.tempData.team == 1) count1++;
            if (player.tempData.team == 2) count2++;
            if (player.tempData.status == PlayerTempData.Status.InGame)
                return false;
        }
        if (count1 < 1 || count2 < 1)
            return false;
        return true;
    }

    //开战
    public void StartFight()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Fight");
        status = Status.InGame;
        int teamPos1 = 1;
        int teamPos2 = 1;
        lock(list)
        {
            protocol.AddInt(list.Count);
            foreach (Player p in list.Values)
            {
                p.tempData.hp = 100;
                protocol.AddString(p.id);
                protocol.AddInt(p.tempData.team);
                if (p.tempData.team == 1)
                    protocol.AddInt(teamPos1++);
                else
                    protocol.AddInt(teamPos2++);
                p.tempData.status = PlayerTempData.Status.InGame;
            }
        }
        Broadcast(protocol);
    }
    //胜负判断
    public int IsWin()
    {
        if (status != Status.InGame)
            return 0;
        int count1 = 0;
        int count2 = 0;
        foreach(Player player in list.Values)
        {
            PlayerTempData pt = player.tempData;
            if (pt.team == 1 && pt.hp > 0) count1++;
            if (pt.team == 2 && pt.hp > 0) count2++;
        }
        if (count1 <= 0) return 2;
        if (count2 <= 0) return 1;
        return 0;
    }
    //处理战斗结果
    public void UpdateWin()
    {
        int isWin = IsWin();
        if (isWin == 0)
            return;
        //改变状态 数值处理
        lock(list)
        {
            status = Status.Prepare;
            foreach (Player player in list.Values)
            {
                if (player.tempData.team == isWin)
                    player.data.killNum++;
                else
                    player.data.killedNum++;
            }
        }
        //广播
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Result");
        protocol.AddInt(isWin);
        Broadcast(protocol);

    }
    //中途退出战斗
    public void ExitFight(Player player)
    {
        //摧毁角色
        if (list[player.id] != null)
            list[player.id].tempData.hp = -1;
        //广播消息
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Hit");
        protocol.AddString(player.id);
        protocol.AddString(player.id);
        protocol.AddFloat(999);
        Broadcast(protocol);
        //增加失败次数
        if (IsWin() == 0)
            player.data.killedNum++;
        //胜负判断
        UpdateWin();
    }

}
