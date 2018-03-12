using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RoomPanel : PanelBase
{
    public int SelfHeroType;
    public List<Transform> PlayerPanels = new List<Transform>();
    private Button closeBtn;
    private Button startBtn;
    private Text maptitle;
    public bool ifisowner = false;
    public int selfnumsinprefabs = 0;
    
    //hero and readystate imgs
    public Sprite[] heroicons;
    public Sprite[] readyimgs;
    
    //mapdropdown
    private DropDown dropDownItem;
    private int currentmaptype = 0;
    
    public List<string> mapnames;
    public Sprite[] mapimgs;
    //对应的地图背景显示
    public SpriteRenderer mapimage;
    
    #region 生命周期
    //初始化
    public override void Init(params object[] args)
    {
        //Resources.LoadAll("Ui/HeroIcon",typeof(Sprite)) as Texture[]; 不行？？？
        heroicons = Resources.LoadAll<Sprite>("Ui/HeroIcon");
        readyimgs = Resources.LoadAll<Sprite>("Ui/ReadyImg");
        mapimgs = Resources.LoadAll<Sprite>("Ui/MapBg");
        
        mapimage = GameObject.Find("bg").GetComponent<SpriteRenderer>();
        mapnames = new List<string> {"Ice Land", "Forest", "Wilderness"};
        base.Init(args);
        if (args.Length == 1)
            currentmaptype = int.Parse((string)args[0]);
        skinPath = "RoomPanel";
        layer = PanelLayer.Panel;
    }

    private void Update()
    {
        if (ifisowner && dropDownItem.maptype != currentmaptype)
        {
            currentmaptype = dropDownItem.maptype; //房主
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("SwitchMap");
            protocol.AddInt(currentmaptype);
            NetMgr.srvConn.Send(protocol);
        }
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        //playerpanels
        for (int i = 0; i < 4; ++i)
        {
            string name = "PlayerPanelPrefab" + i.ToString();
            Transform prefabi = skinTrans.Find("Players").Find(name);
            PlayerPanels.Add(prefabi);
        }
        //组件
        closeBtn = skinTrans.Find("CloseButton").GetComponent<Button>();
        startBtn = skinTrans.Find("ReadyButton").GetComponent<Button>();
        
        dropDownItem = skinTrans.Find("Dropdown").GetComponent<DropDown>();
        maptitle = skinTrans.Find("MapTitle").GetComponent<Text>();
        
        //按钮事件
        closeBtn.onClick.AddListener(OnCloseClick);
        startBtn.onClick.AddListener(OnStartClick);

        //初始化
        mapimage.sprite = mapimgs[currentmaptype];
        maptitle.text += mapnames[currentmaptype]; 
        //监听
        NetMgr.srvConn.msgDist.AddListener("GetRoomInfo", RecvGetRoomInfo);
        NetMgr.srvConn.msgDist.AddListener("Fight", RecvFight);
        NetMgr.srvConn.msgDist.AddListener("ChangeRState",RecvChangeRState);
        NetMgr.srvConn.msgDist.AddListener("SwitchMap",RecvSwitchMap);
        //发送查询
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomInfo");
        NetMgr.srvConn.Send(protocol);
    }
    public override void OnClosing()
    {
        NetMgr.srvConn.msgDist.DelListener("GetRoomInfo", RecvGetRoomInfo);
        NetMgr.srvConn.msgDist.DelListener("Fight", RecvFight);
        NetMgr.srvConn.msgDist.DelListener("ChangeRState",RecvChangeRState);
        NetMgr.srvConn.msgDist.DelListener("SwitchMap",RecvSwitchMap);
    }
    #endregion


    public void RecvSwitchMap(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        currentmaptype = proto.GetInt(start,ref start);
        //更改背景图
        mapimage.sprite = mapimgs[currentmaptype];
        //非房主更改地图显示
        if(!ifisowner)
        {
            maptitle.text = "Map: " + mapnames[currentmaptype];
        }
    }
    //刷新列表
    public void RecvGetRoomInfo(ProtocolBase protocol)
    {
        //获取总数
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start,ref start);
        if (count < 4)
        {
            for(int i = count;i<4;i++)
                PlayerPanels[i].gameObject.SetActive(false);
        }
        for(int i=0;i<count;i++)
        {
            string id = proto.GetString(start, ref start);
            int team = proto.GetInt(start, ref start);
            int herotype = proto.GetInt(start, ref start);
            int isOwner = proto.GetInt(start, ref start);
            int state = proto.GetInt(start, ref start);
            currentmaptype = proto.GetInt(start, ref start);
            //0 inroomnotready 1 inroomready 2ingame
            //信息处理
            //0蓝，1红
            Transform trans = PlayerPanels[i];
            trans.gameObject.SetActive(true);
            
            Text IDtext = trans.Find("IdText").GetComponent<Text>();
            IDtext.text = "ID: "+id;
            Image HeroIcon = trans.Find("HeroIcon").GetComponent<Image>();
            HeroIcon.sprite = heroicons[herotype];
            Image SelfFlag = trans.gameObject.GetComponent<Image>();
            Text IfOwnerText = trans.Find("IfOwnerText").GetComponent<Text>();
            
            if (id == GameMgr.Instance.id)//是自己
            {
                SelfHeroType = herotype;
                SelfFlag.color = new Color(0f,0.545f,1f);
                selfnumsinprefabs = i;
                if (isOwner == 1)
                {
                    IfOwnerText.text = "Owner";
                    ifisowner = true;
                    startBtn.gameObject.GetComponentInChildren<Text>().text = "Start Game";
                }
                else
                {
                    IfOwnerText.text = "";
                    ifisowner = false;
                    Debug.Log("isOwner = "+isOwner);
                    startBtn.gameObject.GetComponentInChildren<Text>().text = "Ready";
                }
            }
            else
            {
                SelfFlag.color = new Color(1f,0f,0f);
                IfOwnerText.text = isOwner == 1 ? "Owner" : "";
            }
            if (ifisowner)
                maptitle.text = "Map: ";
            else
                dropDownItem.gameObject.SetActive(false);
            //ReadyImg
            Image ReadyIcon = trans.Find("ReadyIcon").gameObject.GetComponent<Image>();
            ReadyIcon.sprite = state!=1 ? readyimgs[1] : readyimgs[0];
        }
        /*
        if(count==1)
        {
            Transform trans = PlayerPanels[1];//剩余的
            Text IDtext = trans.Find("IdText").GetComponent<Text>();
            Text StateText = trans.Find("StateText").GetComponent<Text>();
            Text IfSelfText = trans.Find("IfSelfText").GetComponent<Text>();
            Text IfOwnerText = trans.Find("IfOwnerText").GetComponent<Text>();
            Text RoleText = trans.Find("RoleText").GetComponent<Text>();
            RoleText.text = "";
            IDtext.text = "";
            IfOwnerText.text = "";
            StateText.text = "";
            IfSelfText.text = "等待其他玩家加入";
        }*/
    }

    public void RecvChangeRState(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int changestatenum = proto.GetInt(start,ref start);
        Image ReadyIcon = PlayerPanels[changestatenum].Find("ReadyIcon").GetComponent<Image>();
        Text btnText = startBtn.gameObject.GetComponentInChildren<Text>();
        if (ReadyIcon.sprite == readyimgs[1])
        {
            ReadyIcon.sprite= readyimgs[0];
            if(selfnumsinprefabs==changestatenum)
                btnText.text = "Cancel";
        }
        else if(ReadyIcon.sprite == readyimgs[0])
        {

            ReadyIcon.sprite = readyimgs[1];
            if(selfnumsinprefabs==changestatenum)
                btnText.text = "Ready";
        }
    }
    //退出按钮
    public void OnCloseClick()
    {
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
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
                 //PanelMgr.instance.OpenPanel<TipPanel>("", "退出成功！");
                 PanelMgr.instance.OpenPanel<RoomListPanel>("",SelfHeroType.ToString());
                 Close();
                 mapimage.sprite = mapimgs[3];
             }
             else
             {
                 PanelMgr.instance.OpenPanel<TipPanel>("", "Failed");
             }
         });
    }
    //开始按钮
    public void OnStartClick()
    {
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        if (ifisowner)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("StartFight");
            NetMgr.srvConn.Send(protocol, (ProtocolBase p) =>
            {
                //获取数值
                ProtocolBytes proto = (ProtocolBytes) p;
                int start = 0;
                string protoName = proto.GetString(start, ref start);
                int ret = proto.GetInt(start, ref start);
                //处理
                if (ret != 0)
                {
                    PanelMgr.instance.OpenPanel<TipPanel>("", "Failed to start");
                }
            });
        }
        else
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("ChangeRState");
            protocol.AddInt(selfnumsinprefabs);
            NetMgr.srvConn.Send(protocol);
        }
    }
    public void RecvFight(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        MultiBattle.Instance.StartBattle(proto);
        ControlKeys.Instance.createPanel();
        Close();
    }
}
