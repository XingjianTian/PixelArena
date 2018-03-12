using UnityEngine;
public class DeathCameraFade : MonoBehaviour
{
    public static DeathCameraFade Instance;
    public PlayerControl pc;
    public Shader curShader;
    public bool ifflash = false;
    public float grayScaleAmount = 1.0f;
    private Material curMaterial;
    public Material material
    {
        get
        {
            if(curMaterial == null)
            {
                curMaterial = new Material(curShader);
                curMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return curMaterial;
        }
    }
	// Use this for initialization
	void Start () {
        //if (Player.GetComponent<DeathControl>().ifdead == true)
        // {
	    Instance = this;
        pc = MultiBattle.Instance.list[GameMgr.Instance.id].Player;
            if (SystemInfo.supportsImageEffects == false)
            {
                enabled = false;
                return;
            }
            if (curShader != null && curShader.isSupported == false)
            {
                enabled = false;
            }
       // }
	}
	
	// Update is called once per frame
	void Update () {
        //if (Player.GetComponent<DeathControl>().ifdead == true)
            grayScaleAmount = Mathf.Clamp(grayScaleAmount, 0.0f, 1.0f);
	}
    public void OnDisable()
    {
            if (curMaterial != null)
                DestroyImmediate(curMaterial);
    }
    void OnRenderImage(RenderTexture sourceTexture,RenderTexture destTexture)
    {
            if (curShader != null&&pc!=null&&pc.ifdead&&pc.grounded)
        {
            material.SetFloat("_LuminosityAmount", grayScaleAmount);
                Graphics.Blit(sourceTexture, destTexture, material);

            }
            
            else if(curShader!=null&&ifflash==true)
            {
                material.SetFloat("_LuminosityAmount", grayScaleAmount);
                Graphics.Blit(sourceTexture, destTexture, material);
                Invoke("ChangeColor", 0.15f);
            }
            else
            {
                Graphics.Blit(sourceTexture, destTexture);
            }
    }
    void ChangeColor()
    {
        ifflash = false;
    }
}
