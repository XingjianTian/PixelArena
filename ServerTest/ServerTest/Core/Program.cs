using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Scene scene = new Scene();
            ServNet servNet = new ServNet();
            servNet.proto = new ProtocolBytes();
            //192.168.0.102
            servNet.Start("192.168.0.100", 1234);
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
