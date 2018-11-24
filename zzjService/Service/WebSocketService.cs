using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SocketCommunicationFoundation;
using System.Net;
using System.IO;
using System.Threading;
using WebSocketSharp.Server;

namespace zzjService
{
    public partial class WebSocketService : ServiceBase
    {
        public WebSocketService()
        {
            InitializeComponent();
        }

        WebSocketServer wssv = new WebSocketServer(System.Net.IPAddress.Parse("192.168.0.6"), 5050);

        protected override void OnStart(string[] args)
        {
            wssv.AddWebSocketService<WebSocketServiceImplement>("/zzjService");
            wssv.Start();
        }

        protected override void OnStop()
        {
        }
    }
}
