using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : PanelBase
{
    private InputField idInput;
    private InputField pwInput;
    private InputField IPAdressInput;
    private InputField PortInput;
    private Button loginBtn;
    private Button regBtn;

    #region 生命周期
    //初始化
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "LoginPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        idInput = skinTrans.Find("UsernameInput").GetComponent<InputField>();
        pwInput = skinTrans.Find("PassWordInput").GetComponent<InputField>();
        IPAdressInput = skinTrans.Find("IPAdressInput").GetComponent<InputField>();
        PortInput = skinTrans.Find("PortInput").GetComponent<InputField>();
        loginBtn = skinTrans.Find("LoginButton").GetComponent<Button>();
        regBtn = skinTrans.Find("RegisterButton").GetComponent<Button>();
        
        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
    }


    #endregion

    //注册回调函数
    public void OnRegClick()
    {
        PanelMgr.instance.OpenPanel<RegPanel>("");
        Close();
    }


    //登录回调函数
    public void OnLoginClick()
    {
        //前端校验
        //用户名密码为空
        if (IPAdressInput.text == "" || PortInput.text == "")
        {
            PanelMgr.instance.OpenPanel<TipPanel>("","IP地址或端口号不能为空！");
            return; 
        }
        if(idInput.text==""||pwInput.text=="")
        {
            //Debug.Log("用户名密码不能为空！");
            PanelMgr.instance.OpenPanel<TipPanel>("","用户名或密码不能为空！");
            return; 
        }
        if(NetMgr.srvConn.status!=Connection.Status.Connected)
        {
            string host = IPAdressInput.text;
            int port =int.Parse(PortInput.text);
            NetMgr.srvConn.proto = new ProtocolBytes();
            if(!NetMgr.srvConn.Connect(host, port))
                PanelMgr.instance.OpenPanel<TipPanel>("", "连接服务器失败！");
        }
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        //Debug.Log(" 发送 " + protocol.GetDesc());
        //回调lambda/Send接受protocolbase和委托类型(匿名委托转换为lambda表达式)
        NetMgr.srvConn.Send(protocol, (ProtocolBase p)=>
        {                                   
            //解析协议
            ProtocolBytes proto = (ProtocolBytes)p;
            int start = 0;
            string protoName = proto.GetString(start, ref start);
            int ret = proto.GetInt(start, ref start);
            
            if (ret == 0)
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "登陆成功！");
                //开始游戏
                PanelMgr.instance.OpenPanel<RoomListPanel>("");
                GameMgr.Instance.id = idInput.text;//唯一id
                Close();
            }
            else
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "用户名或密码错误！");
            }
        });

    }
}
