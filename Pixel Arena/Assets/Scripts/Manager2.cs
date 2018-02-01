using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public enum DisplayState
{
    Play,
    Pause
}
public enum ChapterOrder
{
    ChapterPre,
    Chapter1,
    Chapter2
}
public class Manager2 : MonoBehaviour
{
    public AudioListener cameAL;
    public bool ifvolumeon;
    public Sprite VolumeOn;
    public Sprite VolumeOff;
    public Image img;
    private int SceneID;
    //private ScenePortal scenedoor;
    public AudioSource BGM;
    private bool ifs = false;
    public ChapterOrder co =ChapterOrder.ChapterPre;
    private int size = 60;
    private GameObject Player;
    private DeathControl dc;
    private ResPutUp rpu;
    public DisplayState state;//展示状态,暂停或运行
    GUIStyle guiRectStyle;
    GUIStyle guiTextRectStyle1;
    GUIStyle guiTextRectStyle2;
    //按钮
    public GameObject Pause_btn;
    public GameObject Pause_Follow;

    //道具
    public Texture FireCreaters_img;
    public Texture Ability1_img;
    public Texture Ability2_img;
    public Texture RedPocket_img;
    public Texture Dumplings_img;
    public Texture fu_img;

    public float screenX;
    public float screenY;
    public float scaleX;
    public float scaleY;

    ResPutUp ResPart;
    public Vector2 pointStart; // 物品列表顶点
    // Use this for initialization
    void Start()
    {
        img = GameObject.Find("Volume").GetComponent<Image>();
        ifvolumeon = true;
        SceneID = Application.loadedLevel;
        //scenedoor =GameObject.Find("SceneDoor").GetComponentInChildren<ScenePortal>();
        if(GameObject.Find("BGM_Audio")!=null)
            BGM = GameObject.Find("BGM_Audio").GetComponent<AudioSource>();
        Player = GameObject.Find("NewHero(Clone)");
        if (Player != null)
        {
            dc = Player.GetComponent<DeathControl>();
            rpu = Player.GetComponent<ResPutUp>(); 
            ResPart = Player.GetComponent<ResPutUp>();
        }
        screenX = Screen.width;
        screenY = Screen.height;
        scaleX = screenX / 1280;
        scaleY = screenY / 720;
        guiRectStyle = new GUIStyle();
        //guiRectStyle.border = new RectOffset(0, 0, 0, 0);
        guiRectStyle.alignment = TextAnchor.MiddleCenter;
        //textstyle1
        guiTextRectStyle1 = new GUIStyle();
        guiTextRectStyle1.fontSize = (int)(32*scaleX);
        guiTextRectStyle1.fontStyle = FontStyle.Bold;
        guiTextRectStyle1.normal.textColor = Color.white;

        //textstyle2
        guiTextRectStyle2 = new GUIStyle();
        guiTextRectStyle2.fontSize = (int)(36*scaleX);
        guiTextRectStyle2.fontStyle = FontStyle.Bold;
        guiTextRectStyle2.normal.textColor = Color.white;
        guiTextRectStyle2.alignment = TextAnchor.MiddleRight;
        pointStart = new Vector2(1100*scaleX, 20*scaleY);

        
    }
    public void TFade()
    {
        co = ChapterOrder.ChapterPre;
        ifs = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(BGM!=null)
            if(BGM.volume<=0.1f)//&&!scenedoor.ifAudioFade)
            BGM.volume += Time.deltaTime / 6;
    }
    void OnGUI()
    {
        if (rpu!=null&&rpu.showtext)
        {
            string text = "你获得了 " + rpu.resName;
            GUIStyle bb = new GUIStyle();
            bb.fontStyle = FontStyle.Bold;
            bb.alignment = TextAnchor.MiddleCenter;
            bb.normal.background = null;    //这是设置背景填充的
            bb.normal.textColor = new Color(1, 1, 1);   //设置字体颜色的
            bb.fontSize = (int)(40 * scaleX);
            GUI.Label(new Rect(Screen.width * 0.4f, Screen.height * 0.2f, 250 * scaleX, 250 * scaleY), text, bb);
        }
        if (dc!=null&&dc.ifdead && dc.isgrounded)
        {
            GUIStyle bb = new GUIStyle();
            bb.normal.background = null;    //这是设置背景填充的
            bb.normal.textColor = new Color(1, 1, 1);   //设置字体颜色的
            bb.fontSize = (int)(100 * scaleX);
            GUI.Label(new Rect(Screen.width * 0.3f, Screen.height * 0.3f, 250 * scaleX, 250 * scaleY), "你死了", bb);
        }

        if(co==ChapterOrder.Chapter1&&!ifs)
        {
            GUIStyle bb = new GUIStyle();
            bb.normal.background = null;    //这是设置背景填充的
            bb.normal.textColor = new Color(1, 1, 1);   //设置字体颜色的
            bb.fontSize = (int)(100 * scaleX);
            GUI.Label(new Rect(Screen.width * 0.3f, Screen.height * 0.25f, 250 * scaleX, 250 * scaleY), "第一章： 春", bb);
            Invoke("TFade",01.5f);
        }
        
        /*
        if (state == DisplayState.Play)
        {

            if (GUI.Button(new Rect(20 * scaleX, 20 * scaleY,
                Pause_btn.width * scaleX, Pause_btn.height * scaleY),
                Pause_btn,
                guiRectStyle))
            {
                state = DisplayState.Pause;
                Time.timeScale = 0;
            }
        }
        if (state == DisplayState.Pause)
        {
            if (GUI.Button(new Rect(screenX * 0.5f - Exit_btn.width * 0.5f * scaleX,
                screenY * 0.4f + Replay_btn.height * 0.5f * scaleY + 10f * scaleY,
                Exit_btn.width * scaleX, Exit_btn.height * scaleY),
                Exit_btn,
                guiRectStyle))
                Application.Quit();


            if (GUI.Button(new Rect(screenX * 0.5f - Replay_btn.width * 0.5f * scaleX,
                screenY * 0.4f - Replay_btn.height * 0.5f * scaleY,
                Replay_btn.width * scaleX, Replay_btn.height * scaleY),
                Replay_btn,
                guiRectStyle))
            {
                Time.timeScale = 1;
                SceneManager.LoadScene(SceneID);
            }

            if (GUI.Button(new Rect(screenX * 0.5f - Continue_btn.width * 0.5f * scaleX,
                screenY * 0.4f - Replay_btn.height * 0.5f * scaleY - Continue_btn.height * scaleY - 10f * scaleY,
                Continue_btn.width * scaleX, Continue_btn.height * scaleY),
                Continue_btn,
                guiRectStyle))
            {
                Time.timeScale = 1;
                state = DisplayState.Play;
            }
        }*/

        int order = 0;
       
        // Debug.Log(ResPart.ToString());
        
        if (ResPart != null && !ResPart.Reses.FireCreaters.empty)
        {
            listing(FireCreaters_img, ref order, ResPart.Reses.FireCreaters.num.ToString()+" X ","爆竹");
        }
        if (ResPart != null && !ResPart.Reses.Ability1.empty)
        {
            listing(Ability1_img, ref order, "", "二段跳能力石");
        }
        if (ResPart != null && !ResPart.Reses.Ability2.empty)
        {
            listing(Ability2_img, ref order, "", "护盾能力石");
        }
        // Debug.Log(order.ToString());
    }
    void listing(Texture t, ref int order, string num,string description)
    {
        GUI.Label (new Rect(pointStart.x - 70* scaleX, pointStart.y + (5 + order) * scaleY,
            size * scaleX, size * scaleY),
            description,
            guiTextRectStyle2);
        GUI.Label(new Rect(pointStart.x, pointStart.y + (order * scaleY),
            size * scaleX, size * scaleY),
            t,
            guiRectStyle);
        GUI.Label(new Rect(pointStart.x - 150 * scaleX, pointStart.y +(15+ order) * scaleY,
            size * scaleX, size * scaleY),
            num,
            guiTextRectStyle1);
        order += 80*(int)scaleX;
    }
    

    
}