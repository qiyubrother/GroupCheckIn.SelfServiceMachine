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
            // ���ձ���д����־
            LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][�յ�ǰ̨����]{e.Data}", LogType.All);

            var d = e.Data;
            //Console.WriteLine($"[{DateTime.Now}]OnMessage::{d}");
            #region �����л�����дHotelId�������к�
            var code = (JsonConvert.DeserializeObject(d) as Newtonsoft.Json.Linq.JObject).GetValue("TransCode");
            var j = JsonConvert.DeserializeObject(d) as JObject;
            j["HotelId"] = SelfServiceMachineHelper.GetHotelId();
            d = JsonConvert.SerializeObject(j);
            LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][�����л�����дHotelId���]{d}", LogType.All);
            #endregion

            // ��ǰ̨���ͱ���д��Tx����ͨ�� ��δ�����ܣ�
            var expire = DateTime.Now.AddSeconds(5);
            SystemEnvironment.TxQueue.Enqueue(new QueueItem { RawData = d, WebSocketID = ID });
            //LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][��ǰ̨���ͱ���д��Tx����ͨ����δ�����ܣ�]...", LogType.All);
            LogHelper.WriteLogAsync($"[{DateTime.Now}]ID:{ID}", LogType.All);
            DebugHelper.PrintTxMessage(d);
            #region �ȴ�������Ӧ���ģ����պ󷢻�ǰ̨
            do
            {
                if (ConnectionState != WebSocketState.Open)
                {
                    Console.WriteLine($"[{DateTime.Now}][�����ѶϿ�][�������ݷ�ֹ]...");
                    LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][�����ѶϿ�][�������ݷ�ֹ]...", LogType.All);

                    break;
                }
                if (SystemEnvironment.RxQueue.Count > 0)
                {
                    SystemEnvironment.RxQueue.TryDequeue(out QueueItem data);
                    if (data.WebSocketID == ID)
                    {
                        // �����ؽ��ֱ��ת����ǰ̨
                        LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][�յ�SocketClient�������ݲ�����ǰ̨]{data.RawData}", LogType.All);
                        Send(data.RawData as string);
                        DebugHelper.PrintRxMessage(data.RawData as string);
                        break;
                    }
                    else
                    {
                        // ��Ч��ID���ݷ�ֹ
                        continue;
                    }
                }
                Thread.Sleep(10);
            } while (expire >= DateTime.Now);
            if (expire < DateTime.Now)
            {
                // Timeout.
                LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][��ʱ][û��5����û���յ���Ӧ����]...", LogType.All);
            }
            #endregion
            LogHelper.WriteLogAsync($"[{DateTime.Now}][WebSocketService][���OnMessage]", LogType.All);
        }
    }


}
