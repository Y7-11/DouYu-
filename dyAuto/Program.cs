using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
namespace dyAuto
{
    class Program
    {
        static void Main(string[] args)
        {

            int port = 8601;
            string Address = "119.97.145.173";
            int roomid = 0;
            Client client = new Client();
            client.connect(Address, port);
            Console.Write("请输入房间号：");
            try
            {
                roomid = int.Parse(Console.ReadLine());
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message); ;
            }
            Log.LogDir += roomid;
            string context = "type@=loginreq/roomid@=" + roomid;
            string Gid = "type@=joingroup/rid@=" + roomid + "/gid@=-9999/";

          
            var islogin= client.LoginRoom(roomid);
            if (islogin)
            {
                client.JoinRoom(roomid);
            }     
            Console.WriteLine("===============================================================================");
            client.Keeplive();
            Task.Run(() =>
            {
                client.receive();
            });
            Console.ReadKey();
        }
        
    }
}
