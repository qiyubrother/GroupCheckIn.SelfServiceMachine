using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zzjService
{
    //public class Env
    //{
    //    public static ShareData ChannelA { get; set; } = new ShareData();
    //    public static ShareData ChannelB { get; set; } = new ShareData();
    //}

    public class ShareData
    {
        private object _obj = null;
        private bool _hasValidData = false;
        private DateTime _expire = DateTime.Now;
        public DateTime PushData(object obj, int seconds)
        {
            lock (this)
            {
                _obj = obj;
                _expire = DateTime.Now.AddSeconds(seconds);
                _hasValidData = true;

                return _expire;
            }
        }
        public object PopData()
        {
            lock (this)
            {
                _hasValidData = false;
            }
            return _obj;
        }
        public bool HasValidData { get=>_hasValidData && DateTime.Now <= _expire; }

        public DateTime Expire { get => _expire; }
    }
}
