using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace zzjService.Code
{
    sealed public class SelfServiceMachineHelper
    {
        static private string hotelId = string.Empty;
        static public string GetHotelId()
        {
            if (hotelId == string.Empty)
            {
                using (StreamReader sr = new StreamReader("HotelId.txt"))
                {
                    var arr = sr.ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 0)
                    {
                        throw new Exception("Invalid HotelId.");
                    }
                    hotelId = arr[0].Trim();
                }
            }
            return hotelId;
        }
    }
}
