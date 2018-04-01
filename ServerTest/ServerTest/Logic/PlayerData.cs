using System;

[Serializable]
public class PlayerData
{
    public int winTimes = 0;
    public float kd = 0 ;
	public int killNum=0;
    public int killedNum=0;
	public PlayerData()
	{
        killNum = 0;
        killedNum = 0;
        kd = 0;
        winTimes = 0;
	}
}