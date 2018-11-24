using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
namespace zzjService
{
    public class SystemEnvironment
    {
        public static ConcurrentQueue<QueueItem> TxQueue { get; set; } = new ConcurrentQueue<QueueItem>();
        public static ConcurrentQueue<QueueItem> RxQueue { get; set; } = new ConcurrentQueue<QueueItem>();
    }

    public class QueueItem
    {
        public object RawData;
        public string WebSocketID { get; set; }
    }
}
