using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LanPanel : PanelBase {

	private InputField IPAdressInput;
    private Button ConnectBtn;
    private Button ClosesBtn;
    private Text IpInput;
    #region 生命周期
    //初始化
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "LanPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        ConnectBtn = skinTrans.Find("ConnectButton").GetComponent<Button>();
        ConnectBtn.onClick.AddListener(OnConnect);

        ClosesBtn = skinTrans.Find("CloseButton").GetComponent<Button>();
        ClosesBtn.onClick.AddListener(OnCloseClick);
        IpInput = skinTrans.Find("IpInputField").Find("Text").GetComponent<Text>();
    }


    #endregion
    public void OnConnect()
    {
        
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        if(NetMgr.srvConn.status!=Connection.Status.Connected)
        {
            string host = IpInput.text;
            int port = 1234;
            NetMgr.srvConn.proto = new ProtocolBytes();
            if (!NetMgr.srvConn.Connect(host, port))
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "Failed + "+host);
                return;
            }
        }
        Close();
        PanelMgr.instance.OpenPanel<LoginPanel>("");
    }
    public void OnCloseClick()
    {
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        PanelMgr.instance.OpenPanel<ConnectPanel>("");
        Close();
    }
}
