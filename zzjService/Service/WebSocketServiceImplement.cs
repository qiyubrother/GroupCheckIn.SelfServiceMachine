using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using qiyubrother.MemoryMappedFileImplement;
using System.Text;
using System.Threading;
using Transaction;
using Transaction.Definition;
using zzjService.Code;
namespace zzjService
{
    public class WebSocketServiceImplement : WebSocketBehavior
    {
        protected override void OnMessage (MessageEventArgs e)
        {
            // 接收报文写入日志
            LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][收到前台请求]{e.Data}", LogType.All);

            var d = e.Data;
            //Console.WriteLine($"[{DateTime.Now}]OnMessage::{d}");
            #region 反序列化，填写HotelId，再序列号
            var code = (JsonConvert.DeserializeObject(d) as Newtonsoft.Json.Linq.JObject).GetValue("TransCode");
            var j = JsonConvert.DeserializeObject(d) as JObject;
            j["HotelId"] = SelfServiceMachineHelper.GetHotelId();
            d = JsonConvert.SerializeObject(j);
            LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][反序列化，填写HotelId完成]{d}", LogType.All);
            #endregion

            // 将前台上送报文写入Tx数据通道 （未作加密）
            var expire = DateTime.Now.AddSeconds(5);
            SystemEnvironment.TxQueue.Enqueue(new QueueItem { RawData = d, WebSocketID = ID });
            //LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][将前台上送报文写入Tx数据通道（未作加密）]...", LogType.All);
            LogHelper.WriteLogAsync($"[{DateTime.Now}]ID:{ID}", LogType.All);
            DebugHelper.PrintTxMessage(d);
            #region 等待接收响应报文，接收后发回前台
            do
            {
                if (ConnectionState != WebSocketState.Open)
                {
                    Console.WriteLine($"[{DateTime.Now}][连接已断开][上送数据废止]...");
                    LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][连接已断开][上送数据废止]...", LogType.All);

                    break;
                }
                if (SystemEnvironment.RxQueue.Count > 0)
                {
                    SystemEnvironment.RxQueue.TryDequeue(out QueueItem data);
                    if (data.WebSocketID == ID)
                    {
                        // 将返回结果直接转发给前台
                        LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][收到SocketClient返回数据并发回前台]{data.RawData}", LogType.All);
                        Send(data.RawData as string);
                        DebugHelper.PrintRxMessage(data.RawData as string);
                        break;
                    }
                    else
                    {
                        // 无效的ID数据废止
                        continue;
                    }
                }
                Thread.Sleep(10);
            } while (expire >= DateTime.Now);
            if (expire < DateTime.Now)
            {
                // Timeout.
                LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][超时][没有5秒内没有收到响应报文]...", LogType.All);
            }
            #endregion
            LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][完成OnMessage]", LogType.All);
        }
    }


}
