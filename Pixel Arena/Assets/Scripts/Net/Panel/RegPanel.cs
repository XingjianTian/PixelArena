using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegPanel : PanelBase {

    private InputField idInput;
    private InputField pwInput;
    private InputField repInput;
    private Button regBtn;
    private Button closeBtn;

    #region 生命周期

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RegPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        idInput = skinTrans.Find("UsernameInput").GetComponent<InputField>();
        pwInput = skinTrans.Find("PassWordInput").GetComponent<InputField>();
        repInput = skinTrans.Find("RepInput").GetComponent<InputField>();
        regBtn = skinTrans.Find("RegisterConfirmedButton").GetComponent<Button>();
        closeBtn = skinTrans.Find("CloseButton").GetComponent<Button>();

        closeBtn.onClick.AddListener(OnCloseClick);
        regBtn.onClick.AddListener(OnRegClick);
    }

    #endregion

    public void OnCloseClick()
    {
        PanelMgr.instance.OpenPanel<LoginPanel>("");
        Close();
    }
    public void OnRegClick()
    {
        //用户名密码为空
        if (idInput.text == "" || pwInput.text == "")
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "用户名或密码不能为空！");
            return;
        }

        if(pwInput.text!=repInput.text)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "请确认密码！");
            return;
        }

        if(NetMgr.srvConn.status!=Connection.Status.Connected)
        {
            string host = "127.0.0.1";
            int port = 1234;
            NetMgr.srvConn.proto = new ProtocolBytes();
            NetMgr.srvConn.Connect(host, port);
        }
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Register");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        Debug.Log(" 发送 " + protocol.GetDesc());
        //匿名委托转换为lambda表达式
        NetMgr.srvConn.Send(protocol, (ProtocolBase p)=>
        {
            ProtocolBytes proto = (ProtocolBytes)p;
            int start = 0;
            string protoName = proto.GetString(start, ref start);
            int ret = proto.GetInt(start, ref start);
            if (ret == 0)
            {
                Debug.Log("注册成功!");
                PanelMgr.instance.OpenPanel<LoginPanel>("");
                Close();
            }
            else
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "注册失败！");
            }
        });//发送Login协议
    }
   

}
