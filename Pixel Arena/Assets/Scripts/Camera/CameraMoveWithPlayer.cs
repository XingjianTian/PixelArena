using UnityEngine;

public class CameraMoveWithPlayer : MonoBehaviour {
    private  bool ifReturnOriginalSize;
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
        ifMoveWithOther = false;
        ifMoveToSolidPoint = false;
        gameObject.GetComponent<DeathCameraFade>().enabled = true;
    }

    public void Reset()
    {
        ifMoveWithPlayer = false;
        transform.position = new Vector2(0,-1.36f);
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
        //相机范围
        //x -1.7 -1.84
        if (ifMoveWithPlayer)
        {
            Vector3 TargetPosition = Vector3.zero;
        
            if (character.position.x >= -1.7f && character.position.x <= 1.84f)
                TargetPosition.x = character.position.x;
            else if (character.position.x < -1.7f)
                TargetPosition.x = -1.7f;
            else if (character.position.x > 1.84f)
                TargetPosition.x = 1.84f;
            //y -0.68f -2.65
            if (character.position.y >= -2.65f && character.position.y <= -0.68f)
                TargetPosition.y = character.position.y;
            else if (character.position.y < -2.65f)
                TargetPosition.y = -2.65f;
            else if (character.position.y > -0.68f)
                TargetPosition.y = -0.68f;
            
            TargetPosition.z = -3f;
            transform.position = TargetPosition;
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
