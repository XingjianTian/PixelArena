using UnityEngine;
using UnityEngine.UI;

public class ConnectPanel : PanelBase {

    private InputField IPAdressInput;
    private InputField PortInput;
    private Button NetBtn;
    private Button LocalBtn;
    public string localIpv4;
    public string NetIpv4 = "123.206.73.52";
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
        NetBtn.onClick.AddListener(()=>Connect(NetIpv4));
        LocalBtn.onClick.AddListener(()=>Connect(GetIpAddress()));
    }


    #endregion
    public void Connect(string ip)
    {
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        if(NetMgr.srvConn.status!=Connection.Status.Connected)
        {
            string host = ip;
            int port = 1234;
            NetMgr.srvConn.proto = new ProtocolBytes();
            if (!NetMgr.srvConn.Connect(host, port))
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "Failed");
                return;
            }
        }
        Close();
        PanelMgr.instance.OpenPanel<LoginPanel>("");
    }
    private static string GetIpAddress()
    {
        /*
        try
        {
            string HostName = Dns.GetHostName(); //得到主机名
            IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
            for (int i = 0; i < IpEntry.AddressList.Length; i++)
            {
                //从IP地址列表中筛选出IPv4类型的IP地址
                //AddressFamily.InterNetwork表示此IP为IPv4,
                //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return IpEntry.AddressList[i].ToString();
                }
            }
            return "";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return "";
        }*/
        //return Network.player.ipAddress;
        return "192.168.0.115";
    }
}
