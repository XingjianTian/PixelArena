using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : PanelBase
{
    private Text text;
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
        text = skinTrans.Find("Text").GetComponent<Text>();
        text.text = str;
        //关闭按钮
        btn = skinTrans.Find("CloseButton").GetComponent<Button>();
        btn.onClick.AddListener(OnCloseButtonClick);
    }


    #endregion
    //按下按钮
    public void OnCloseButtonClick()
    {
        Close();
        if (text.text == "你胜利了！" || text.text == "你失败了！")
        {
            SendLeaveBattleInfo();
            MultiBattle.Instance.ClearBattle();
            DeathCameraFade.Instance.enabled = false;
            PanelMgr.instance.OpenPanel<RoomPanel>("");
            
        }
    }
    public void SendLeaveBattleInfo()
    {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("LeaveBattle");
        proto.AddString(GameMgr.Instance.id);
        NetMgr.srvConn.Send(proto);
    }

}
