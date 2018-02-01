using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResPutUp : MonoBehaviour {
    private float varifySpeed = 1.2f;
    public bool if_E_Pressed = false;
    public string resName;
    public bool showtext;
    public bool ifShieldOn = false;
    public GameObject shield;
    public GameObject cannon;
    public bool ifCoroutine = false;
    public RaycastHit2D hit;
    public float curAlpha = 0;
    //private GameObject hero;
    private resList reses;
    public resList Reses
    {
        get
        {
            return reses;
        }
    }
    public void Button_E_Down()
    {
        if_E_Pressed = true;
    }
    public void Button_E_Up()
    {
        if_E_Pressed = false;
    }
    // Use this for initialization
    void Start () {
        reses = new resList();
    }
    void showGetRes(GameObject obj)
    {
        AudioSource.PlayClipAtPoint(GetComponent<PlayerControl>().Events[0], transform.position);
        switch (obj.name)
        {
            case "FireCraker":resName = "爆竹"; break;
            case "Ability1":resName = "二段跳能力";break;
            case "Ability2":resName = "护盾能力";break;
        }
        showtext = true;
        StartCoroutine(fadetext());

    }
    IEnumerator fadetext()
    {
        yield return new WaitForSeconds(1f);
        showtext = false;
    }
    // Update is called once per frame
    void Update()
    {
        hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 0.05f),
            GetComponent<PlayerControl>().facingRight ? Vector2.right : Vector2.left, 0.3f,
            1 << LayerMask.NameToLayer("res"));
        if (hit.collider != null)
        {
            var methodName = "Get" + hit.collider.gameObject.name;
            var mm = GetType().GetMethod(methodName);
            if (mm == null)
                Debug.Log("没有相应函数");
            else
                mm.Invoke(this, null);
        }
    }
    public IEnumerator ShieldFade()
    {
        yield return new WaitForSeconds(5f);
        shield.GetComponent<MeshRenderer>().enabled = false;
        shield.GetComponent<DynamicLight>().enabled = false;
        ifShieldOn = false;
    }

    public IEnumerator Emerge(GameObject obj)
    {
        
        if (obj != null)
        {
            TextMesh tm = obj.GetComponentInChildren<TextMesh>();
            for (; curAlpha < 1; curAlpha += Time.deltaTime *varifySpeed)
            {
                if (obj!=null&&obj.CompareTag("res"))
                    tm.color = new Color(1f, 1f, 1f, curAlpha);
                else if (obj != null && obj.CompareTag("onlyTrigger"))
                {
                    tm.color = new Color(1f, 1f, 1f, curAlpha);
                }
                else if (obj != null && obj.CompareTag("NPC"))
                {
                    tm.color = new Color(0f, 0f, 0f, curAlpha);
                    obj.transform.Find("NPCduihuakuang").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, curAlpha);
                }
                else if (obj != null && obj.CompareTag("EventText"))
                {
                    tm.color = new Color(1f, 1f, 1f, curAlpha);
                }
                yield return 0;
            }
           
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(Fade(obj));
                
        }
    }
    public IEnumerator Fade(GameObject obj)
    {
            
        if (obj != null)
        {
            TextMesh tm = obj.GetComponentInChildren<TextMesh>();
            
                for (; curAlpha > -0.5f; curAlpha -= Time.deltaTime * varifySpeed)
                {
                    if (obj != null && obj.CompareTag("res"))
                        tm.color = new Color(1f, 1f, 1f, curAlpha);
                    else if (obj != null && obj.CompareTag("onlyTrigger"))
                    {
                        tm.color = new Color(1f, 1f, 1f, curAlpha);
                    }
                    else if (obj != null && obj.CompareTag("NPC"))
                    {
                        tm.color = new Color(0f, 0f, 0f, curAlpha);
                        obj.transform.Find("NPCduihuakuang").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, curAlpha);
                    }
                    yield return 0;
                }
                ifCoroutine = false;
                curAlpha = 0f;

        }
    }

}

// 物品列表类

 
public class resList
{
    public Res FireCreaters;
    public Res Ability1;
    public Res Ability2;
    // private Vector2 headPoint;
    public resList()
    {
        FireCreaters = new Res(0, 3);
        Ability1 = new Res(0, 1);
        Ability2 = new Res(0, 1);

    }
}
// 物品组信息的基类
public class Res
{
    public int numOfRes;
    private int maxOfRes;
    public Res(int numHad, int numMax)
    {
        numOfRes = numHad;
        maxOfRes = numMax;
    }
    public bool ifmorethan1{get{return maxOfRes >= 1;}}
    public int num{get{return numOfRes;}}
    public bool empty{get{return numOfRes == 0;}}
    public int fullNum{set{maxOfRes = value;}}
    public bool full{get{return numOfRes == maxOfRes;}}
    public void getRes(GameObject o)
    {
        ++numOfRes;
        numOfRes = System.Math.Min(numOfRes, maxOfRes);
    }
    public void getRes()
    {
        ++numOfRes;
        numOfRes = System.Math.Min(numOfRes, maxOfRes);
    }
    public void clear(){numOfRes = 0;}
}


