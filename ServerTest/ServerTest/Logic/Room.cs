
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Net;
using System.Net.Sockets;
//using pair = System.Collections.Generic.KeyValuePair<string, Player>;

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
    public int MaxPlayers = 4;
    //map
    public int MapType = 0;
    public Dictionary<string, Player> list = new Dictionary<string, Player>();
    public LockFrame lf = new LockFrame();

    Thread FrameSend;
    Thread CreateBuff;
    bool ifsendon = false;
    System.Timers.Timer t1;
    System.Timers.Timer t2;
    Random rd = new Random();
    //添加玩家
    public bool AddPlayer(Player player,int herotype)
    {
        lock (list)
        {
            if (list.Count >= MaxPlayers)
                return false;
            PlayerTempData tempData = player.tempData;
            tempData.room = this;
            tempData.team = SwitchTeam();
            tempData.herotype = herotype;
            switch(herotype)
            {
                case 0:tempData.maxHp = 150;break;//solider
                case 1:tempData.maxHp = 100;break;//ninja
                case 2:tempData.maxHp = 250;break;//roshan
            }
            if (list.Count == 0)
            {
                tempData.isOwner = true;
                tempData.status = PlayerTempData.Status.InRoomReady;
            }
            else
            {
                tempData.isOwner = false;
                tempData.status = PlayerTempData.Status.InRoomNotReady;
            }
            string id = player.id;
            list.Add(id,player);
        }
        return true;
    }

    //分配队伍
    public int SwitchTeam()
    {
        int count1 = 0;
        int count2 = 0;
        int count3 = 0;
        int count4 = 0;
        foreach (Player player in list.Values)
        {
            if (player.tempData.team == 1) count1++;
            else if (player.tempData.team == 2) count2++;
            else if (player.tempData.team == 3) count3++;
            else count4++;
        }
        if (count1 < 1)
            return 1;
        else if (count2 < 1)
            return 2;
        else if (count3 < 1)
            return 3;
        else
            return 4;
    }
    //删除玩家
    public void DelPlayer(string id)
    {
        lock(list)
        {
            if (!list.ContainsKey(id))
                return;
            bool isOwner = list[id].tempData.isOwner;
            list[id].tempData.status = PlayerTempData.Status.OutOfRoom;
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
            p.tempData.status = PlayerTempData.Status.InRoomReady;
        }
    }

    //udp
    public void udpBroadcast(ProtocolBase protocol)
    {
        lock (list)
        {
            foreach (Player player in list.Values)
            {
                player.udpSend(protocol);
            }
        }
    }
    //广播消息
    public void Broadcast(ProtocolBase protocol)
    {
        lock (list)
        {
            foreach (Player player in list.Values)
            {
                player.Send(protocol);
            }
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
        foreach (var items in list)
        {
            var p = items.Value;
            protocol.AddString(p.id);
            protocol.AddInt(p.tempData.team);
            protocol.AddInt(p.tempData.herotype);
            int isOwner = p.tempData.isOwner ? 1 : 0;
            protocol.AddInt(isOwner);
            Console.WriteLine(p.id+(isOwner>0?"是房主":"不是"));
            int state = -1 ;
            switch (p.tempData.status)
            {
                case PlayerTempData.Status.InRoomNotReady: state = 0; break;
                case PlayerTempData.Status.InRoomReady:state = 1;break;
                case PlayerTempData.Status.InGame:state = 2;break;
            }
            protocol.AddInt(state);
            protocol.AddInt(MapType);
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
            if (player.tempData.status == PlayerTempData.Status.InGame||
                player.tempData.status==PlayerTempData.Status.InRoomNotReady)
                return false;
        }
        if (count1 < 1 )//|| count2 < 1)
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
        int teamPos3 = 1;
        int teamPos4 = 1;
        lock (list)
        {
            protocol.AddInt(list.Count);
            protocol.AddInt(MapType);
            foreach (Player p in list.Values)
            {
                p.tempData.currentHp = p.tempData.maxHp;
                protocol.AddString(p.id);
                protocol.AddInt(p.tempData.team);

                if (p.tempData.team == 1)
                    protocol.AddInt(teamPos1++);
                else if(p.tempData.team ==2)
                    protocol.AddInt(teamPos2++);
                else if (p.tempData.team == 3)
                    protocol.AddInt(teamPos3++);
                else if (p.tempData.team == 4)
                    protocol.AddInt(teamPos4++);

                protocol.AddInt(p.tempData.herotype);
                p.tempData.status = PlayerTempData.Status.InGame;
            }
        }
        Broadcast(protocol);

        //lockframe初始化
        lf.initialize(list);
        ifsendon = true;
        FrameSend = new Thread(lockframesend);
        FrameSend.Start();
        
        CreateBuff = new Thread(CreateBuffStone);
        CreateBuff.Start();
    }
    private void lockframesend()
    {
        Console.Write("1");
        t1 = new System.Timers.Timer(20);//实例化，设置间隔时间；
        t1.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
        t1.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
        t1.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
    }
    void theout(object source, System.Timers.ElapsedEventArgs e)
    {
        lf.SendPerFrame(this);
    }
    private void CreateBuffStone()
    {
        t2 = new System.Timers.Timer(15000);//实例化，设置间隔时间；
        t2.Elapsed += new System.Timers.ElapsedEventHandler(CreateB);//到达时间的时候执行事件；
        t2.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
        t2.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件
    }
    //每15s
    void CreateB(object source, System.Timers.ElapsedEventArgs e)
    {
        
        //前闭后开
        int randpos = rd.Next(0, 4);
        int bufftype = rd.Next(0, 5);
        ProtocolBytes protocolret = new ProtocolBytes();
        protocolret.AddString("CreateBuff");
        protocolret.AddInt(randpos);
        protocolret.AddInt(bufftype);
        Broadcast(protocolret);
    }

    //胜负判断
    public int IsWin()
    {
        if (status != Status.InGame)
            return 0;
        int count1 = 0;
        int count2 = 0;
        int count3 = 0;
        int count4 = 0;
        foreach(Player player in list.Values)
        {
            PlayerTempData pt = player.tempData;
            if (pt.team == 1 && pt.currentHp > 0) ++count1;
            if (pt.team == 2 && pt.currentHp > 0) ++count2;
            if (pt.team == 3 && pt.currentHp > 0) ++count3;
            if (pt.team == 4 && pt.currentHp > 0) ++count4;
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
               // player.tempData.currentHp = player.tempData.maxHp;
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
        Console.WriteLine("check update battle");
        StopFrameSend();
    }
    public void StopFrameSend()
    {
        FrameSend.Abort();
        CreateBuff.Abort();
        lf.Clean_lockstep_data();
        t1.Close();
        t2.Close();
    }
    //中途退出战斗
    public void ExitFight(Player player)
    {
        //摧毁角色
        if (list[player.id] != null)
            list[player.id].tempData.currentHp = -1;
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
