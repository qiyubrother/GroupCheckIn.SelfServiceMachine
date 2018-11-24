#define CONSOLE_SERVICE_MODE
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace zzjService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {

#if CONSOLE_SERVICE_MODE
            LogHelper.LogFilePath = @"\zzjLog";
            Parallel.Invoke(
                ()=>
                {
                    var wssv = new WebSocketServer(System.Net.IPAddress.Parse("192.168.0.6"), 5050);
                    wssv.AddWebSocketService<WebSocketServiceImplement>("/zzjService");
                    wssv.Start();
                    LogHelper.WriteLogAsync($"[{DateTime.Now}]Self-ServiceMachine's service is running..", LogType.All);
                },
                () =>
                {
                    new ClientSocketCoreBusiness().Start();
                    LogHelper.WriteLogAsync($"[{DateTime.Now}]Self-ServiceMachine's client socket is running..", LogType.All);
                }
            );

#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new WebSocketService(),
                new ClientSocketService()
            };
            LogHelper.LogFilePath = @"D:\tool\zzjService";
            LogHelper.WriteLogAsync($"[{DateTime.Now}]Self-ServiceMachine's service is running..", LogType.All);

            ServiceBase.Run(ServicesToRun);
#endif

        }
    }
}
