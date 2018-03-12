using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class HandlePlayerMsg
{
        //获取杀人数和死亡次数
        //协议参数：
        //返回协议：int int
        public void MsgGetKillAndKilledTime(Player player,ProtocolBase protoBase)
        {
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("GetKillAndKilledTime");
            protocolRet.AddInt(player.data.killNum);
            protocolRet.AddInt(player.data.killedNum);
            player.Send(protocolRet);
            Console.WriteLine("MsgGet: " + player.id
                + "KillNum: " + player.data.killNum
                + "KilledNum: " + player.data.killedNum);
        }

        //增加杀人数
        //协议参数：
        public void MsgAddKillNum(Player player, ProtocolBase protoBase)
        {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            //处理
            player.data.killNum += 1;
            player.data.kd = player.data.killedNum == 0 ? player.data.killNum : player.data.killNum / player.data.killedNum;
        player.data.winpercentage = player.data.kd;
        Console.WriteLine("MsgAddKillNum " + player.id + " " + player.data.killNum.ToString());
        }
        //增加被杀数
        //协议参数：
        public void MsgAddKilledNum(Player player, ProtocolBase protoBase)
        {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            //处理
            player.data.killedNum += 1;
        player.data.kd =player.data.killNum / player.data.killedNum;
        player.data.winpercentage = player.data.kd;
        Console.WriteLine("MsgAddKillNum " + player.id + " " + player.data.killedNum.ToString());
        }

        //获取玩家列表
        public void MsgGetList(Player player,ProtocolBase protoBase)
        {
            Scene.Instance.SendPlayerList(player);
        }

    //查询Career
    public void MsgGetAchieve(Player player,ProtocolBase protocol)
    {
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("GetAchieve");
        protocolRet.AddFloat(player.data.kd);
        protocolRet.AddFloat(player.data.winpercentage);
        protocolRet.AddInt(player.data.killNum);
        protocolRet.AddInt(player.data.killedNum);
        player.Send(protocolRet);
        Console.WriteLine("MsgGetCareer " + player.id);
    }
  
    
}
