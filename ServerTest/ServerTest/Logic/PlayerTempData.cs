using System;
using System.Collections.Generic;

public class PlayerTempData
{
    public enum Status
    {
        OutOfRoom,
        InRoomNotReady,
        InRoomReady,
        InGame,
    }
    public int herotype;//0-soilder,1-ninja,2-roshan
    public Status status;
    //room״̬
    public Room room;
    public int team = 1;
    public bool isOwner = false;

    //战场相关
    public long lastUpdateTime;
    public float posX;
    public float posY;
    public float maxHp;
    public float currentHp;



	public PlayerTempData()
	{
        status = Status.OutOfRoom;
	}
}