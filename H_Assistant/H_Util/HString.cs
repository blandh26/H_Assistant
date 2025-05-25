using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Util
{
    public static class HString
    {
       /// <summary>
       /// 倒序字符串
       /// </summary>
       /// <param name="text"></param>
       /// <returns></returns>
        public static string ReverseE(string text)
        {
            char[] cArray = text.ToCharArray();
            StringBuilder reverse = new StringBuilder();
            for (int i = cArray.Length - 1; i > -1; i--)
            {
                reverse.Append(cArray[i]);
            }
            return reverse.ToString();
        }
    }
}
