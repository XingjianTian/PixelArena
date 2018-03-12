using UnityEngine;
using UnityEngine.UI;

public class TipPanel : PanelBase
{
    private Text Text;
    private Button btn;
    string str = "";

    #region 生命周期
    //初始化
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "TipPanel";
        layer = PanelLayer.Tips;
        //参数args[1]表示提示的内容
        if (args.Length == 1)
            str = (string)args[0];
    }

    //显示之前
    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        //文字
        Text = skinTrans.Find("Text").GetComponent<Text>();
        Text.text = str;
        //关闭按钮
        btn = skinTrans.Find("CloseButton").GetComponent<Button>();
        btn.onClick.AddListener(OnCloseButtonClick);
        
    }


    #endregion
    //按下按钮
    public void OnCloseButtonClick()
    {
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        Close();
        if (Text.text == "You Won !" || Text.text == "You Lost !")
        {
            SendLeaveBattleInfo();
            CameraMoveWithPlayer cmwp = Camera.main.gameObject.GetComponent<CameraMoveWithPlayer>();
            cmwp.Reset();
            MultiBattle.Instance.ClearBattle();
            DeathCameraFade.Instance.enabled = false;
            PanelMgr.instance.OpenPanel<RoomPanel>("");
            
        }
        if (Text.text == "Want to quit ?")
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("Logout");
            NetMgr.srvConn.Send(protocol, (ProtocolBase) =>NetMgr.srvConn.Close());
            Application.Quit();
        }
    }
    public void SendLeaveBattleInfo()
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("LeaveBattle");
        NetMgr.srvConn.Send(proto);
    }

}
