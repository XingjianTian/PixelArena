using System;
using System.Collections.Generic;
public class LockFrame{ 
    private int gameframe = 0;
    private Dictionary<string, int[]> id_cmd = new Dictionary<string, int[]>();
    public void Update_lockstep_data(Player p, int optochange,int changeto)
    {
        //有延迟！客户端结束的时候最后一个点击还会发过来
        lock (id_cmd)
        {
            if (!id_cmd.ContainsKey(p.id))
                return;
            id_cmd[p.id][optochange] = changeto;
        }
    }
    public void Clean_lockstep_data()
    {
       id_cmd.Clear();
    }
    public void initialize(Dictionary<string,Player> l)
    {
        foreach (var p in l)
        {
            int[] cmd = new int[5];
            for (int i = 0; i < 5; ++i)
                cmd[i] = 0;
            //对每一个player，new一个cmd
            id_cmd.Add(p.Key, cmd);
        }
        Console.WriteLine("Initialized");
    }
    public void SendPerFrame(Room room)
    {   //获取房间
        if (id_cmd.Count == 0)
            return;

        lock (id_cmd)
        {
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("FrameOps");
            protocolRet.AddInt(id_cmd.Count);
            protocolRet.AddInt(gameframe);//frame
            foreach (var item in id_cmd)//集和已修改，可能不能遍历
            {
                protocolRet.AddString(item.Key);//id
                foreach (var v in item.Value)
                    protocolRet.AddInt(v);
            }

            room.Broadcast(protocolRet);
            gameframe++;
        }
    }
}
