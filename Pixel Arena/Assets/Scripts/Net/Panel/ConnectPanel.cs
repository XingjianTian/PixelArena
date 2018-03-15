using UnityEngine;
using UnityEngine.UI;
using System.Net.NetworkInformation;
using System.Net.Sockets;
public class ConnectPanel : PanelBase {

    private Button NetBtn;
    private Button LocalBtn;
    #region 生命周期
    //初始化
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "ConnectPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        NetBtn = skinTrans.Find("NetButton").GetComponent<Button>();
        LocalBtn = skinTrans.Find("LocalButton").GetComponent<Button>();
        NetBtn.onClick.AddListener(WanConnect);
        LocalBtn.onClick.AddListener(LanConnect);
    }


    #endregion
    public void WanConnect()
    {
        string ip = "123.206.73.52";
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        if(NetMgr.srvConn.status!=Connection.Status.Connected)
        {
            string host = ip;
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

    public void LanConnect()
    {
        PanelMgr.instance.OpenPanel<LanPanel>("");
        Close();
    }
    private static string GetIpAddress()
    {
        Debug.Log(Network.player.ipAddress);
        return Network.player.ipAddress;
        string userIp = "";
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces(); ;
        foreach (NetworkInterface adapter in adapters)
        {
            if (adapter.Supports(NetworkInterfaceComponent.IPv4))
            {
                UnicastIPAddressInformationCollection uniCast = adapter.GetIPProperties().UnicastAddresses;
                if (uniCast.Count > 0)
                {
                    foreach (UnicastIPAddressInformation uni in uniCast)
                    {
                        //得到IPv4的地址。 AddressFamily.InterNetwork指的是IPv4
                        if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            userIp =uni.Address.ToString();
                        }
                    }
                }
            }
        }
        return userIp;
    }
}
