using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace dyAuto
{
    public class Client
    {
        IPEndPoint ipe;
        Socket socket;
        int bytes;
        //List<string> renshu = new List<string>();
        public void connect(string ip, int port)
        {
            try
            {
                ipe = new IPEndPoint(IPAddress.Parse(ip), port);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipe);
                Console.WriteLine("服务器连接成功");
                Log.WriteLog("服务器连接成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine("服务器连接失败：" + ex.Message);
                Log.WriteLog("服务器连接失败：" + ex.Message);
            }
        }


        public bool LoginRoom(long roomid)
        {
            try
            {
                Console.WriteLine("请求登陆房间号：" + roomid);
                Log.WriteLog("请求登陆房间号：" + roomid);
                if (roomid<1000)
                {
                    Console.WriteLine("房间号错误");
                    Log.WriteLog("房间号错误:"+roomid);
                    return false;
                }
                string context = "type@=loginreq/roomid@=";
                SendMsg(context);
                return true;                                                   
          
            }
            catch (Exception ex)
            {
                Console.WriteLine("登陆失败: roomid：" + roomid + "错误：" + ex.Message);
                Log.WriteLog("登陆失败: roomid：" + roomid + "错误：" + ex.Message);
                return false;
            }
        }

        public void JoinRoom(int roomid)
        {
            try
            {
                Console.WriteLine("请求加入：" + roomid + " 弹幕组");
                Log.WriteLog("请求加入：" + roomid + " 弹幕组");
                string Gid = "type@=joingroup/rid@=" + roomid + "/gid@=-9999/";
                SendMsg(Gid);
                Console.WriteLine("加入成功：组号-9999");
                Log.WriteLog("加入成功：组号-9999");
            }
            catch (Exception ex)
            {
                Console.WriteLine("加入弹幕组失败: roomid：" + roomid + "gid:-9999" + "错误：" + ex.Message); ;
                Log.WriteLog("加入弹幕组失败: roomid：" + roomid + "gid:-9999" + "错误：" + ex.Message);
            }
        }

        public void SendMsg(string context)
        {
            var length = new byte[] { (byte)(context.Length + 9), 0x00, 0x00, 0x00 };
            var code = length;
            var magic = new byte[] { 0xb1, 0x02, 0x00, 0x00 };
            var Msg = Encoding.UTF8.GetBytes(context);
            var end = new byte[] { 0x00 };

            List<byte> data = new List<byte>();
            data.AddRange(length);
            data.AddRange(code);
            data.AddRange(magic);
            data.AddRange(Msg);
            data.AddRange(end);
            socket.SendTo(data.ToArray(), ipe);

        }

        public void Keeplive()
        {
            System.Timers.Timer t = new System.Timers.Timer(45000);
            t.Elapsed += t_Elapsed;
            t.Start();


        }
         void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var start = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            var time = (DateTime.Now.Ticks - start.Ticks) / 1000;
            SendMsg("type@=keeplive/tick@=" + time + "/");
            Log.WriteLog("发送心跳包");
        }
        public  string receive()
        {
         
            while (true)
            {
                string resStr = "";
         
                byte[] resBytes = new byte[4096];
                //var newsocket = socket.Accept();
                bytes = socket.Receive(resBytes, resBytes.Length, 0);
                if (bytes >= 4)
                {
                    if (resBytes[8] == 178 && resBytes[9] == 2)
                    {
                        resStr += Encoding.UTF8.GetString(resBytes, 0, bytes);
                        GetDanmu(resStr);
                    }
                }
            }

        }

        public void GetDanmu(string Info)
        {     
            string txt = null;
            string nickname = null;
            string uid = null;
            string level = null;
            string bnn = null;
            string bl = null;
            
            Info.Replace("@A", "@").Replace("@S", "/");
            string[] ms = Info.Split('/');
            if (ms.Length > 5)
            {

                foreach (var item in ms)
                {
                    string[] msg = item.Replace("@=", "=").Split('=');
                    if (msg[0] == "txt")
                    {
                        txt = msg[1];   //弹幕
                    }
                    if (msg[0] == "nn")
                    {

                        nickname = msg[1];  //昵称
                    }
                    if (msg[0] == "uid")
                    {

                        uid = msg[1];  //用户id
                    }
                    if (msg[0] == "level")
                    {

                        level = msg[1];  //斗鱼等级
                    }
                    if (msg[0] == "bnn")
                    {

                        bnn = msg[1];  //粉丝牌
                    }
                    if (msg[0] == "bl")
                    {

                        bl = msg[1];   //粉丝牌等级
                    }

                }
                if (txt != null && nickname != null)
                {
                    var dm="LV_" + level + " 粉丝牌：" + bnn + "<LV." + bl + ">  " + nickname + "\t:" + txt;
                    //renshu.Add(nickname);    人数
                    //renshu=renshu.Distinct().ToList();
                    Console.WriteLine(dm);
                    Log.WriteLog(dm);
            
                }
            }
        }

    }
}
