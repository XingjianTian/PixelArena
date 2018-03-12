using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : PanelBase
{
    private InputField idInput;
    private InputField pwInput;
    private Button loginBtn;
    private Button regBtn;
    private Button cloBtn;
    #region 生命周期
    //初始化
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "LoginPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        idInput = skinTrans.Find("UsernameInput").GetComponent<InputField>();
        pwInput = skinTrans.Find("PassWordInput").GetComponent<InputField>();
        loginBtn = skinTrans.Find("LoginButton").GetComponent<Button>();
        regBtn = skinTrans.Find("RegisterButton").GetComponent<Button>();
        cloBtn = skinTrans.Find("CloseButton").GetComponent<Button>();
        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
        cloBtn.onClick.AddListener(OnCloseClick);
    }


    #endregion

    //注册回调函数
    public void OnRegClick()
    {
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        PanelMgr.instance.OpenPanel<RegPanel>("");
        Close();
    }

    public void OnCloseClick()
    {
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        NetMgr.srvConn.Close();
        PanelMgr.instance.OpenPanel<ConnectPanel>("");
        Close();
    }

    //登录回调函数
    public void OnLoginClick()
    {
        AudioSource.PlayClipAtPoint(Volume.instance.Events[0],Camera.main.transform.position);
        //前端校验
        //用户名密码为空
        if(idInput.text==""||pwInput.text=="")
        {
            //Debug.Log("用户名密码不能为空！");
            PanelMgr.instance.OpenPanel<TipPanel>("","Please Enter rightly!");
            return; 
        }
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        //Debug.Log(" 发送 " + protocol.GetDesc());
        //回调lambda/Send接受protocolbase和委托类型(匿名委托转换为lambda表达式)
        NetMgr.srvConn.Send(protocol, (ProtocolBase p)=>
        {                                   
            //解析协议
            ProtocolBytes proto = (ProtocolBytes)p;
            int start = 0;
            string protoName = proto.GetString(start, ref start);
            int ret = proto.GetInt(start, ref start);
            
            if (ret == 0)
            {
                //PanelMgr.instance.OpenPanel<TipPanel>("", "登陆成功！");
                //开始游戏
                PanelMgr.instance.OpenPanel<RolePanel>("");
                GameMgr.Instance.id = idInput.text;//唯一id
                Close();
            }
            else
            {
                PanelMgr.instance.OpenPanel<TipPanel>("", "Please Enter rightly!");
            }
        });

    }
}
