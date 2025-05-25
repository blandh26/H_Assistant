using H_Assistant.Framework.Const;
using H_Assistant.Framework.liteDbModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Assistant.Framework
{
    public static class LanguageHepler
    {
        static string jsonLanguage = "";// Json语言设定
        private static string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;   //存储在本程序目录下
        /// <summary>
        /// 数据库中获取系统语言代码
        /// </summary>
        /// <returns></returns>
        public static string GetDbLanguage()
        {
            var liteDBHelper = LiteDBHelper.GetInstance();
            var db_SystemSet = liteDBHelper.db.GetCollection<SystemSet>();
            SystemSet l_Model = db_SystemSet.FindOne(x => x.Name == SysConst.Sys_Language);// 语言设定
            return l_Model.Value;
        }
        /// <summary>
        /// Json文件中获取对应字段名称
        /// </summary>
        /// <param name="key">语言代码</param>
        /// <returns></returns>
        public static string GetLanguage(string key)
        {
            try
            {
                jsonLanguage = System.IO.File.ReadAllText(path + "Language" + "\\" + GetDbLanguage() + ".json");
                Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonLanguage);
                return dic.Where(S => S.Key == key).Select(S => S.Value).First().ToString();
            }
            catch (Exception ee)
            {
                return "";
            }
        }
    }
}
