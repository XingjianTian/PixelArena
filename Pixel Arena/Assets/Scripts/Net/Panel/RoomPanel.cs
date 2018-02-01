using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RoomPanel : PanelBase
{

    private List<Transform> prefabs = new List<Transform>();
    private Button closeBtn;
    private Button startBtn;

    #region 生命周期
    //初始化
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RoomPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;

        prefabs.Add(skinTrans.Find("PlayerPrefab").GetComponent<Transform>());
        prefabs.Add(skinTrans.Find("PlayerPrefab2").GetComponent<Transform>());
        //组件
        closeBtn = skinTrans.Find("CloseButton").GetComponent<Button>();
        startBtn = skinTrans.Find("ReadyButton").GetComponent<Button>();
        //按钮事件
        closeBtn.onClick.AddListener(OnCloseClick);
        startBtn.onClick.AddListener(OnStartClick);

        //监听
        NetMgr.srvConn.msgDist.AddListener("GetRoomInfo", RecvGetRoomInfo);
        NetMgr.srvConn.msgDist.AddListener("Fight", RecvFight);
        //发送查询
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomInfo");
        NetMgr.srvConn.Send(protocol);
    }
    public override void OnClosing()
    {
        NetMgr.srvConn.msgDist.DelListener("GetRoomInfo", RecvGetRoomInfo);
        NetMgr.srvConn.msgDist.DelListener("Fight", RecvFight);
    }
    #endregion
    
    //刷新列表
    public void RecvGetRoomInfo(ProtocolBase protocol)
    {
        //获取总数
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start,ref start);
        for(int i=0;i<count;i++)
        {
            string id = proto.GetString(start, ref start);
            int team = proto.GetInt(start, ref start);
            float kd = proto.GetFloat(start, ref start);
            int isOwner = proto.GetInt(start, ref start);
            int state = proto.GetInt(start, ref start);
            //0 none 1 inroom 2ingame
            //信息处理
            Transform trans = prefabs[i];
            Text IDtext = trans.Find("IdText").GetComponent<Text>();
            IDtext.text = "ID: "+id;
            Text KDText = trans.Find("KDText").GetComponent<Text>();
            KDText.text = "K/D: "+kd.ToString();
            Text IfSelfText = trans.Find("IfSelfText").GetComponent<Text>();
            Text IfOwnerText = trans.Find("IfOwnerText").GetComponent<Text>();
            if (id==GameMgr.Instance.id)
                IfSelfText.text ="【我自己】";
            if (isOwner == 1)
                IfOwnerText.text = "【房主】";
            Text StateText = trans.Find("StateText").GetComponent<Text>();
            switch (state)
            {
                case 0:StateText.text = "None";break;
                case 1: StateText.text = "准备好啦";break;
                case 2:StateText.text = "还在战场里";break;
            }
        }
        if(count==1)
        {
            Transform trans = prefabs[1];
            Text IDtext = trans.Find("IdText").GetComponent<Text>();
            IDtext.text = "等待其他玩家加入";
            Text KDText = trans.Find("KDText").GetComponent<Text>();
            KDText.text ="";
        }
    }
    //退出按钮
    public void OnCloseClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("LeaveRoom");
        NetMgr.srvConn.Send(protocol, (ProtocolBase p) =>
         {
             //获取数值
             ProtocolBytes proto = (ProtocolBytes)p;
             int start = 0;
             string protoName = proto.GetString(start, ref start);
             int ret = proto.GetInt(start, ref start);
             //处理
             if(ret==0)
             {
                 PanelMgr.instance.OpenPanel<TipPanel>("", "退出成功！");
                 PanelMgr.instance.OpenPanel<RoomListPanel>("");
                 Close();
             }
             else
             {
                 PanelMgr.instance.OpenPanel<TipPanel>("", "退出失败！");
             }
         });
    }
    //开始按钮
    public void OnStartClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartFight");
        NetMgr.srvConn.Send(protocol, (ProtocolBase p) =>
         {
             //获取数值
             ProtocolBytes proto = (ProtocolBytes)p;
             int start = 0;
             string protoName = proto.GetString(start, ref start);
             int ret = proto.GetInt(start, ref start);
             //处理
             if(ret!=0)
             {
                 PanelMgr.instance.OpenPanel<TipPanel>("", "开始游戏失败！");
             }
         });
    }
    public void RecvFight(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        MultiBattle.Instance.StartBattle(proto);
        Close();
    }
}
