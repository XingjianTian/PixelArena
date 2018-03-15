using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBattle : MonoBehaviour {

    //单例
    public static MultiBattle Instance;
    /*
    {
        get { return Nested.instance; }
    }

    class Nested
    {
        static Nested(){ }
        internal static readonly MultiBattle instance = new MultiBattle();
    }*/
    //角色预设
    public GameObject[] playerPrefabs;
    //地图预设
    public GameObject[] mapPrefabs;
    public GameObject currentmap;
    //buffStone预设
    public GameObject[] BuffPrefabs;
    //场内的所有角色
    public Dictionary<string, BattlePlayer> list = new Dictionary<string, BattlePlayer>();
	// Use this for initialization
	void Start ()
	{
	    Instance = this;
	}
    //获取阵营，0表示错误
    public int GetCamp(GameObject playerObj)
    {
        foreach(BattlePlayer p in list.Values)
        {
            if (p.Player.gameObject == playerObj)
                return p.camp;
        }
        return 0;
    }
    //是否同一阵营
    public bool IfSameCamp(GameObject p1,GameObject p2)
    {
        return GetCamp(p1) == GetCamp(p2);
    }

    //清理场景，初始化角色列表
    public void ClearBattle()
    {
        list.Clear();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for(int i = 0;i<players.Length;i++)
        {
            Destroy(players[i]);
        }
        Destroy(GameObject.FindGameObjectWithTag("Controls"));
        Destroy(GameObject.FindWithTag("Map"));
    }

    //开始战斗
    public void StartBattle(ProtocolBytes proto)
    {
        //解析协议
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        if (protoName != "Fight")
            return;
        //玩家总数
        int count = proto.GetInt(start, ref start);
        //maptype
        int maptype = proto.GetInt(start, ref start);
        //清理场景
        ClearBattle();
        //创建地图
        currentmap = Instantiate(mapPrefabs[maptype]);
        for(int i= 0;i<count;i++)
        {
            string id = proto.GetString(start, ref start);
            int team = proto.GetInt(start, ref start);
            int bornPosID = proto.GetInt(start, ref start);
            int herotype = proto.GetInt(start, ref start);
            GeneratePlayer(id, team, bornPosID,herotype);
        }
        //还有oncelistenner已添加过
        NetMgr.srvConn.msgDist.AddListener("Hit",RecvHit);
        NetMgr.srvConn.msgDist.AddListener("Result",RecvResult);
        NetMgr.srvConn.msgDist.AddListener("FrameOps",RecvOps);
        NetMgr.srvConn.msgDist.AddListener("CreateBuff",CreateBuff);
        PanelMgr.instance.OpenPanel<ResTipPanel>("","Start");

    }
    public void GeneratePlayer(string id, int team, int bornPosID,int herotype)
    {
        //获取出生点
        Transform bornPoints =currentmap.transform.Find("BornPoints").transform;
        Transform bornTrans;
        if(team==1)
        {
            Transform teamBorn = bornPoints.GetChild(0);
            bornTrans = teamBorn.GetChild(bornPosID - 1);
        }
        else if(team==2)
        {
            Transform teamBorn = bornPoints.GetChild(1);
            bornTrans = teamBorn.GetChild(bornPosID - 1);
        }
        else if(team==3)
        {
            Transform teamBorn = bornPoints.GetChild(2);
            bornTrans = teamBorn.GetChild(bornPosID - 1);
        }
        else
        {
            Transform teamBorn = bornPoints.GetChild(3);
            bornTrans = teamBorn.GetChild(bornPosID - 1);
        }
        if(bornTrans==null)
        {
            Debug.LogError("GeneratePlayer 出生点错误!");
            return;
        }
        //预设
        if(playerPrefabs.Length<2)
        {
            Debug.LogError("角色预设数量不够！");
            return;
        }
        //产生角色
        GameObject playerObj = Instantiate(playerPrefabs[herotype]);
        playerObj.name = id;
        playerObj.GetComponentInChildren<TextMesh>().text = id;
        playerObj.transform.position = bornTrans.position;

        //列表处理
        BattlePlayer Bp = new BattlePlayer();
        Bp.Player = playerObj.GetComponent<PlayerControl>();
        Bp.camp = team;
        list.Add(id, Bp);

        //玩家处理
        if(id==GameMgr.Instance.id)
        {
            Bp.Player.ctrlType = CtrlType.Player;
            CameraMoveWithPlayer cmwp = Camera.main.gameObject.GetComponent<CameraMoveWithPlayer>();
            cmwp.SetCharaterTarget(Bp.Player.gameObject);
        }
        else
            Bp.Player.ctrlType = CtrlType.Net;
    }

    public void GenerateBuffStone(int pos, int bufftype)
    {
        Transform buffPoints =currentmap.transform.Find("BuffPoints").transform;
        Transform buffTrans;
        buffTrans = buffPoints.GetChild(pos);
        if(buffTrans==null)
        {
            Debug.LogError("GeneratePlayer 出生点错误!");
            return;
        }
        GameObject buffObj = Instantiate(BuffPrefabs[bufftype]);
        buffObj.transform.position = buffTrans.position;
    }
    #region 战场内的更新，状态同步
    //收到产生buff协议
    public void CreateBuff(ProtocolBase protocol)
    {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes) protocol;
        string protoName = proto.GetString(start, ref start);
        int pos = proto.GetInt(start, ref start);
        int bufftype = proto.GetInt(start, ref start);
        GenerateBuffStone(pos,bufftype);
    }
    //收到hit协议
    public void RecvHit(ProtocolBase protocol)
    {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes) protocol;
        string protoName = proto.GetString(start, ref start);
        string attID = proto.GetString(start, ref start);
        string defID = proto.GetString(start, ref start);
        float damage = proto.GetFloat(start, ref start);
        
        //获取BattlePlayer
        if (!list.ContainsKey(attID)||!list.ContainsKey(defID))
            return;
        //被击中的玩家
        if(attID==defID)//中毒，自减血
            list[defID].Player.BeAttacked(damage,list[attID].Player);
        else if(!IfSameCamp(list[attID].Player.gameObject,list[defID].Player.gameObject))//队友
            list[defID].Player.BeAttacked(damage,list[attID].Player);
    }
    #endregion
    //收到Result协议
    public void RecvResult(ProtocolBase protocol)
    {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes) protocol;
        string protoName = proto.GetString(start, ref start);
        int winteam = proto.GetInt(start, ref start);
        
        //弹出胜负面板
        string id = GameMgr.Instance.id;
        BattlePlayer Bp = list[id];
        //取消监听
        NetMgr.srvConn.msgDist.DelListener("Hit",RecvHit);
        NetMgr.srvConn.msgDist.DelListener("Result",RecvResult);
        NetMgr.srvConn.msgDist.DelListener("FrameOps",RecvOps);
        NetMgr.srvConn.msgDist.DelListener("CreateBuff",CreateBuff);
        Camera.main.GetComponent<CameraFilterPack_AAA_SuperComputer>().enabled = false;
        Camera.main.GetComponent<CameraFilterPack_Blur_BlurHole>().enabled = false;
        Camera.main.GetComponent<CameraFilterPack_Blur_Focus>().enabled = false;
        Camera.main.GetComponent<CameraFilterPack_Color_Chromatic_Aberration>().enabled = false;
        StartCoroutine(WaitAndOpenTip(Bp, winteam));
        gameframe = 0;
    }
    IEnumerator WaitAndOpenTip(BattlePlayer Bp,int winteam)
    {
        yield return new WaitForSeconds(2f);
        
        //ControlKeys.Instance.DestroyPanel();
        if(Bp.camp == winteam)
        {
            AudioSource.PlayClipAtPoint(Volume.instance.Events[3],Camera.main.transform.position);
            PanelMgr.instance.OpenPanel<TipPanel>("", "You Won !");
            Bp.Player.CanvasHealth.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            AudioSource.PlayClipAtPoint(Volume.instance.Events[4],Camera.main.transform.position);
            PanelMgr.instance.OpenPanel<TipPanel>("", "You Lost !");
            Bp.Player.CanvasHealth.transform.GetChild(0).gameObject.SetActive(false);
        }
    
        
    }

    public int gameframe = 0;
    
    //每个客户端只传一人，但接收所有
    public void RecvOps(ProtocolBase protocol)
    {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes) protocol;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        int frame = proto.GetInt(start, ref start);
        if (gameframe > frame) //丢包或
        {
            Debug.Log("gameframe>frame");
            return;
        }
        for (int i = 0; i < count; ++i)
        {
            string id = proto.GetString(start, ref start);
            if (!list.ContainsKey(id))//场景中没有该玩家
            {
                Debug.Log("list not contain");
                return;
            }
            //对应角色处理操作集
            int []ops = new int[5];
            for (int j = 0; j < 5; ++j)
            {
                ops[j] = proto.GetInt(start, ref start);
            }
            if(ops[0]==1)
                Debug.Log("recv Left");
            if(ops[1]==1)
                Debug.Log("recv Right");
            list[id].Player.ProcessOps(ops);
        }
        gameframe++;
        
    }
}
