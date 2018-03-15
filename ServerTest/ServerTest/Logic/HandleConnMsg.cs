using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public partial class HandleConnMsg
    {
        //心跳
        //协议参数：无
        public void MsgHeartBeat(Conn conn,ProtocolBase protoBase)
        {
            conn.lastTickTime = Sys.GetTimeStamp();
            Console.WriteLine("[更新心跳时间]" + conn.GetAdress());
        }

        //注册
        //协议参数：str用户名，str密码
        //返回协议：-1表示失败，0表示成功
        /// <summary>
        /// 客户端发送：协议名，用户名，密码
        /// 服务端发送：协议名，结果（int）
        /// </summary>
        public void MsgRegister(Conn conn,ProtocolBase protobase)
        {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protobase;
            string protoName = protocol.GetString(start, ref start);
            string id = protocol.GetString(start, ref start);
            string pw = protocol.GetString(start, ref start);
            string strFormat = "[收到注册协议]" + conn.GetAdress();
            Console.WriteLine(strFormat + " 用户名: " + id + " 密码: " + pw);

            //构建返回协议
            protocol = new ProtocolBytes();
            protocol.AddString("Register");
            //注册
            if (DataMgr.instance.Register(id, pw))
                protocol.AddInt(0);
            else
                protocol.AddInt(-1);
        //创建角色
            Console.WriteLine("Create");
            DataMgr.instance.CreatePlayer(id);
            //返回协议给客户端
            conn.Send(protocol);
        }

        //登录
        //协议参数：str用户名，str密码
        //返回协议：-1表示失败，0表示成功
        public void MsgLogin(Conn conn,ProtocolBase protoBase)
        {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            string id = protocol.GetString(start, ref start);
            string pw = protocol.GetString(start, ref start);
            string strFormat = "[收到登录协议]" + conn.GetAdress();
            Console.WriteLine(strFormat + " 用户名: " + id + " 密码: " + pw);
            //构建返回协议
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("Login");
            //验证
            if(!DataMgr.instance.CheckPassWord(id,pw))
            {
                protocolRet.AddInt(-1);
                conn.Send(protocolRet);
                return;
            }
            //是否已经登陆
            ProtocolBytes protocolLogout = new ProtocolBytes();
            protocolLogout.AddString("Logout");
            if(!Player.KickOff(id,protocolLogout))
            {
                protocolRet.AddInt(-1);
                conn.Send(protocolRet);
                return;
            }
            //获取玩家数据
            PlayerData playerdata= DataMgr.instance.GetPlayerData(id);
            if(playerdata==null)
            {
                protocolRet.AddInt(-1);
                conn.Send(protocolRet);
                return;
            }
            conn.player = new Player(id, conn);
            conn.player.data = playerdata;
            //事件触发
            ServNet.instance.handlePlayerEvent.OnLogin(conn.player);
            //返回
            protocolRet.AddInt(0);
            conn.Send(protocolRet);
            return;
        }

        //下线
        //协议参数：
        //返回协议：0表示正常下线
        public void MsgLogout(Conn conn,ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("Logout");
            protocol.AddInt(0);
            if(conn.player==null)
            {
                conn.Send(protocol);
                conn.Close();
            }
            else
            {
                conn.Send(protocol);
                conn.player.Logout();
            }

        }

    }
