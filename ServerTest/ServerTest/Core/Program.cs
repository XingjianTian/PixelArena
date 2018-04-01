using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerTest
{
    class Program
    {
        public static string GetIpAddress()
        {
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return IpEntry.AddressList[i].ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }
        static void Main(string[] args)
        {
            Scene scene = new Scene();
            ServNet servNet = new ServNet();
            servNet.proto = new ProtocolBytes();
            //192.168.0.102
            servNet.Start(GetIpAddress(), 1234);//tcp
            servNet.CreateUdpClient();//udp
            //Console.ReadLine();
            DataMgr dataMgr = new DataMgr();
            RoomMgr roomMgr = new RoomMgr();
            while (true)
            {
                string str = Console.ReadLine();
                switch (str)
                {
                    case "quit": servNet.Close(); return;
                    case "print": servNet.Print(); break;
                }
            }
        }

    }
    
}
