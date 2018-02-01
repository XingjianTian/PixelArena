using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveWithPlayer : MonoBehaviour {
    private  bool ifReturnOriginalSize;
    private PlayerControl pc;//摄像机不随主角移动时，主角禁止操作
    public bool ifMoveToSolidPoint = false;
    public Vector3 SolidtargetPosition;//移动到定点
    //跟随
    public bool ifMoveWithOther = false;
    public GameObject FollowOther;//跟随其他精灵
    public bool ifMoveWithPlayer = false;
    public Transform character;   //跟随角色 
    private float smoothTime = 0.01f;  //摄像机平滑移动的时间
    private Vector3 cameraVelocity = Vector3.zero;

    //主摄像机    
    public void SetCharaterTarget(GameObject target)
    {
        character = target.transform;
        ifMoveWithPlayer = true;
        gameObject.GetComponent<DeathCameraFade>().enabled = true;
    } 
    void Start()
    {
        //character = GameObject.Find("NewHero(Clone)").transform;
      
       // if(character!=null)pc = GameObject.Find("NewHero(Clone)").GetComponent<PlayerControl>();
    }

    void Update()
    {
        if (character == null)
            return;
        //Vector3 TargetPosition = Vector3.zero;
        //TargetPosition.x = character.position.x >= -3.779f ? character.position.x : -3.779f;
        //TargetPosition.y = character.position.y <= 2.83f ? character.position.y : 2.83f;
        if (ifMoveWithPlayer)
        {
            transform.position = character.position + new Vector3(0, 0, -3);
        }
        else if(ifMoveToSolidPoint)
        {
            transform.position = Vector3.SmoothDamp(transform.position, SolidtargetPosition,
            ref cameraVelocity, smoothTime*80);
            /*
            if(ifReturnOriginalSize)
            {
                StartCoroutine(WaitAndReturnOriginalSize(7f));
            }
            else if(!ifReturnOriginalSize)
            {
                StartCoroutine(JustWaitAndReturn(7f));
            }*/
        }
        else if(ifMoveWithOther)
        {
            transform.position = Vector3.SmoothDamp
                (transform.position,
                FollowOther.transform.position + new Vector3(0, 0, -3),
                ref cameraVelocity,
                smoothTime*80);
            //StartCoroutine(WaitAndReturnOriginalSize(10f));
        }

    }
    /*
    public void changeFollowObject(GameObject ob)
    {
        pc.allowable = false;
        ifMoveWithPlayer = false;
        ifMoveWithOther = true;
        FollowOther = ob;

    }
    public void changeToSolidPoint(Vector3 point,bool ifReturn)
    {
        pc.allowable = false;
        ifMoveWithPlayer = false;
        ifMoveToSolidPoint = true;
        SolidtargetPosition = point;
        SolidtargetPosition.z = -5f;
        ifReturnOriginalSize = ifReturn;
    }
    IEnumerator JustWaitAndReturn(float s)
    {
        yield return new WaitForSeconds(s);
        ifMoveWithPlayer = true;
        ifMoveToSolidPoint = false;
        ifMoveWithOther = false;
        pc.allowable = true;
    }
    IEnumerator WaitAndReturnOriginalSize(float s)
    {
        yield return new WaitForSeconds(s); 
        ifMoveWithPlayer = true;
        ifMoveToSolidPoint = false;
        ifMoveWithOther = false;
        pc.allowable = true;
        GetComponent<Camera>().orthographicSize = 1.5f;
    }
    public void stopAllCoroutine()
    {
        StopAllCoroutines();
        ifMoveWithPlayer = true;
        ifMoveToSolidPoint = false;
        ifMoveWithOther = false;
        pc.allowable = true;
        
    }*/
}
