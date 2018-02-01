using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanel : PanelBase
{
    private Text idText;
    private Text WinPercentageText;
    private Text KDText;
    private Text BestKillsText;
    private Text TotalKillsText;
    private Text TotalDeathsText;
    private Transform content;
    private GameObject roomPrefab;
    private Button closeBtn;
    private Button newBtn;
    private Button refreshBtn;

    #region 生命周期

    //初始化
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RoomListPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        Transform CareerTrans = skinTrans.Find("CareerPanel");

        //生涯栏控件
        idText = CareerTrans.Find("IDtext").GetComponent<Text>();
        WinPercentageText = CareerTrans.Find("Wintext").GetComponent<Text>();
        KDText = CareerTrans.Find("KDText").GetComponent<Text>();
        BestKillsText = CareerTrans.Find("BestKillnum").GetComponent<Text>();
        TotalKillsText = CareerTrans.Find("Killnum").GetComponent<Text>();
        TotalDeathsText = CareerTrans.Find("Killednum").GetComponent<Text>();

        //列表栏控件
        Transform scrollRect = skinTrans.Find("ScrollRect");
        content = scrollRect.Find("Content");
        roomPrefab = content.Find("RoomPrefab").gameObject;
        roomPrefab.SetActive(false);

        closeBtn = skinTrans.Find("CloseButton").GetComponent<Button>();
        newBtn = skinTrans.Find("CreateButton").GetComponent<Button>();
        refreshBtn = skinTrans.Find("RefreshButton").GetComponent<Button>();

        //按钮事件
        refreshBtn.onClick.AddListener(OnRefreshClick);
        newBtn.onClick.AddListener(OnNewClick);
        closeBtn.onClick.AddListener(OnCloseClick);

        //监听 获取成绩和房间列表
        NetMgr.srvConn.msgDist.AddListener("GetAchieve", RecvGetAchieve);
        NetMgr.srvConn.msgDist.AddListener("GetRoomList", RecvGetRoomList);
        //发送查询
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        NetMgr.srvConn.Send(protocol);

        protocol = new ProtocolBytes();
        protocol.AddString("GetAchieve");
        NetMgr.srvConn.Send(protocol);

    }
    //关闭
    public override void OnClosing()
    {
        NetMgr.srvConn.msgDist.DelListener("GetAchieve", RecvGetAchieve);
        NetMgr.srvConn.msgDist.DelListener("GetRoomList", RecvGetRoomList);
    }
    #endregion

    //刷新Career
    public void RecvGetAchieve(ProtocolBase protocol)
    {
        //解析协议
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        float KD = proto.GetFloat(start, ref start);
        float WinPercentage = proto.GetFloat(start, ref start);
        int BestKills = proto.GetInt(start, ref start);
        int TotalKills = proto.GetInt(start, ref start);
        int TotalDeaths = proto.GetInt(start, ref start);

        //处理
        idText.text += GameMgr.Instance.id;
        KDText.text += KD.ToString();
        WinPercentageText.text += WinPercentage.ToString();
        BestKillsText.text += BestKills.ToString();
        TotalKillsText.text += TotalKills.ToString();
        TotalDeathsText.text += TotalDeaths.ToString();
    }

    //刷新房间列表
    public void RecvGetRoomList(ProtocolBase protocol)
    {
        //清理
        ClearRoomUnit();
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        for (int i = 0; i < count; i++)
        {
            int num = proto.GetInt(start, ref start);
            int status = proto.GetInt(start, ref start);
            GenerateRoomUnit(i, num, status);
        }
    }

    //清理房间显示
    public void ClearRoomUnit()
    {
        for (int i = 0; i < content.childCount; i++)
            if (content.GetChild(i).name.Contains("Clone"))
                Destroy(content.GetChild(i).gameObject);
    }

    //创建一个房间单元
    //参数i：房间序号
    //参数name：房间名
    //参数num：房间里的玩家数
    //参数status：房间状态，1准备中，2游戏中
    public void GenerateRoomUnit(int i, int num, int status)
    {
        //添加房间
        //content.GetComponent<RectTransform>().
        GameObject o = Instantiate(roomPrefab);
        o.transform.SetParent(content);
        o.transform.localScale = new Vector3(1,1,1);
        o.SetActive(true);
        //房间信息
        Transform trans = o.transform;
        Text RoomIDText = trans.Find("RoomIDText").GetComponent<Text>();
        Text countText = trans.Find("CountText").GetComponent<Text>();
        Text statusText = trans.Find("StatusText").GetComponent<Text>();
        RoomIDText.text += (i + 1).ToString();
        countText.text += num.ToString();
        if (status == 1)
        {
            statusText.color = Color.white;
            statusText.text += " Ready";
        }
        else
        {
            statusText.color = Color.red;
            statusText.text += " At War";
        }

        //按钮事件
        Button btn = trans.Find("JoinButton").GetComponent<Button>();
        btn.name = i.ToString();
        btn.onClick.AddListener(delegate ()
        {
            OnJoinBtnClick(btn.name);
        });
    }

    //刷新按钮
    public void OnRefreshClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        NetMgr.srvConn.Send(protocol);
    }
    //加入按钮
    public void OnJoinBtnClick(string name)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("EnterRoom");
        protocol.AddInt(int.Parse(name));
        NetMgr.srvConn.Send(protocol, (ProtocolBase p)=>{
            //解析参数
            ProtocolBytes proto = (ProtocolBytes)p;
            int start = 0;
            string protoName = proto.GetString(start, ref start);
            int ret = proto.GetInt(start, ref start);
            if (ret == 0)
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "成功进入房间!");
                PanelMgr.instance.OpenPanel<RoomPanel>("");
                Close();
            }
            else
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "进入房间失败!");
            }
        });
        Debug.Log("请求进入房间 " + name);
    }
    

    //创建新房间按钮
    public void OnNewClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("CreateRoom");
        NetMgr.srvConn.Send(protocol, (ProtocolBase p)=>{
            //解析参数
            ProtocolBytes proto = (ProtocolBytes)p;
            int start = 0;
            string protoName = proto.GetString(start, ref start);
            int ret = proto.GetInt(start, ref start);
            //处理
            if (ret == 0)
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "创建房间成功！");
                PanelMgr.instance.OpenPanel<RoomPanel>("");
                Close();
            }
            else
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "创建房间失败!");
            }
        });

    }
   

    //登出按钮
    public void OnCloseClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Logout");
        NetMgr.srvConn.Send(protocol, (ProtocolBase) =>{
            PanelMgr.instance.OpenPanel<TipPanel>("", "已登出！");
            PanelMgr.instance.OpenPanel<LoginPanel>("", "");
            NetMgr.srvConn.Close();
        });
        Close();
    }
  
}

