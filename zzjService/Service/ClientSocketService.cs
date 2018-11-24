using qiyubrother.MemoryMappedFileImplement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace zzjService
{
    partial class ClientSocketService : ServiceBase
    {
        ClientSocketCoreBusiness coreBusiness = new ClientSocketCoreBusiness();
        public ClientSocketService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            coreBusiness.Start();
        }

        protected override void OnStop()
        {
            coreBusiness.IsStarting = false;
        }
    }
}
