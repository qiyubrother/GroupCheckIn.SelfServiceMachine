using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SrvDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            //设定服务器IP地址 
            IPAddress ip = IPAddress.Parse("192.168.0.6");
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(new IPEndPoint(ip, 5050)); //配置服务器IP与端口 
                Console.WriteLine("连接服务器成功");
            }
            catch(Exception ex)
            {
                Console.WriteLine("连接服务器失败，请按回车键退出！");
                return;
            }
            byte[] recvBuffer = new byte[1024];
            //通过 clientSocket 发送数据 
            for (int i = 0; i < 100000000; i++)
            {
                try
                {
                    Thread.Sleep(50);    //等待1秒钟 
                    var rd = new Random((int)DateTime.Now.ToFileTimeUtc());
                    var sendMessage = $"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()} {DateTime.Now.Millisecond}";
                    clientSocket.Send(Encoding.Default.GetBytes(sendMessage));
                    var len = clientSocket.Receive(recvBuffer);
                    Console.WriteLine($"S::{sendMessage}\nR::{Encoding.Default.GetString(recvBuffer, 0, len)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"E::{ex.Message}");
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    break;
                }
            }
            //byte[] recvBuffer = new byte[204800000];
            //var len = clientSocket.Receive(recvBuffer);
            //Console.WriteLine("从服务器接收的消息：");
            //Console.WriteLine(System.Text.Encoding.Default.GetString(recvBuffer, 0, len));
            Console.WriteLine("发送完毕，按回车键退出");
            Console.ReadLine();
        }
    }
}
