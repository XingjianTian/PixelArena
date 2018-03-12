using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RolePanel : PanelBase
{
    public int herotype;//0-soilder,1-ninja,2-roshan
    //对应的显示
    private List<string> rolenames;
    private Text roletext;
    private List<string> intros;
    private Text roleintro;
    
    //private DropDown dropDownItem;
    //List<Sprite> sprite_list;
    //public Image other_img;//任意的img，用作被赋值替换
    private Button closeBtn;
    private Button ConfirmBtn;

    #region 生命周期

    //初始化
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RolePanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        Transform RoleShowTrans = skinTrans.Find("RoleShow");

        //dropDownItem = RoleShowTrans.Find("Dropdown").GetComponent<DropDown>();
        roleintro = skinTrans.Find("RoleIntro").GetComponent<Text>();
        roletext = RoleShowTrans.Find("RoleText").GetComponent<Text>();
        closeBtn = skinTrans.Find("CloseButton").GetComponent<Button>();
        ConfirmBtn = skinTrans.Find("ConfirmButton").GetComponent<Button>();

        //按钮事件
        ConfirmBtn.onClick.AddListener(OnComfirmClick);
        closeBtn.onClick.AddListener(OnCloseClick);

    }
    /*
    //关闭
    public override void OnClosing()
    {
        NetMgr.srvConn.msgDist.DelListener("GetAchieve", RecvGetAchieve);
        NetMgr.srvConn.msgDist.DelListener("GetRoomList", RecvGetRoomList);
    }*/
    #endregion
    void Start()
    {
        rolenames = new List<string> {"Soldier", "Ninja", "Roshan"};
        intros = new List<string>
        {
            //soilder
            "Born as a Soldier\n\n" +
            "Having balanced abilities\n" +
            "The most easy-to-control role\n" +
            "Lacking explosive power\n\n" +
            "Special Skill:First-Aid packet\n[Heal HP]",
            //ninja
            "Quick as flash\n\n" +
            "Smooth and flexible\n" +
            "Hard to control but dangerous\n\n" +
            "Special Skill:Rush\n[Deal Damage]\n"+
            "Double Jump and WallJump",
            //roshan
            "Strong like stone\n\n" +
            "Having mighty power\n" +
            "Move slowly but steadly\n" +
            "Just likeee a Biiiiig Tank!\n\n" +
            "Special Skill:Blootproot\n[Literally]"
        };
    }

   
    public override void Update()
    {
        if(roleintro!=null&&intros.Count>herotype)
            roleintro.text = intros[herotype];
        if(roletext!=null&&rolenames.Count>herotype)
            roletext.text = rolenames[herotype];
    }
    //登出按钮
    public void OnCloseClick()
    {
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Logout");
        NetMgr.srvConn.Send(protocol, (ProtocolBase) =>{
            PanelMgr.instance.OpenPanel<TipPanel>("", "Log out");
            PanelMgr.instance.OpenPanel<ConnectPanel>("", "");
            NetMgr.srvConn.Close();
        });
        Close();
    }

    public void OnComfirmClick()
    {AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        PanelMgr.instance.OpenPanel<RoomListPanel>("",herotype.ToString());
        Close();
    }
}
