using System;
using System.Collections.Generic;

public class PlayerTempData
{
    public enum Status
    {
        None,
        InRoom,
        InGame,
    }
    public Status status;
    //room״̬
    public Room room;
    public int team = 1;
    public bool isOwner = false;

    //战场相关
    public long lastUpdateTime;
    public float posX;
    public float posY;
    public long lastShootTime;
    public float hp = 100;


	public PlayerTempData()
	{
        status = Status.None;
	}
}