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
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        PanelMgr.instance.OpenPanel<LoginPanel>("");
        Close();
    }
    public void OnRegClick()
    {
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        //用户名密码为空
        if (idInput.text == "" || pwInput.text == "")
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "Please Enter rightly!");
            return;
        }

        if(pwInput.text!=repInput.text)
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "Please Repeat password!");
            return;
        }

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
                PanelMgr.instance.OpenPanel<TipPanel>("", "Success！");
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
