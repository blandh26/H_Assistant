using H_Assistant.Framework;
using H_Assistant.Framework.Const;
using H_Assistant.Framework.liteDbModel;
using H_Util;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Assistant.Helper
{
    /// <summary>
    /// 授权验证
    /// 1001:机器MD5和注册MD5不一致
    /// 1002:本机MD5和注册码MD5 不一致
    /// 1003:试用期超时
    /// 1004:本机MD5和授权码MD5 不一致
    /// 1005:授权时间过期
    /// 1006:注册表和数据库注册码不一致
    /// 1007:这次时间和上一次操作时间不一样
    /// 9999:异常
    /// </summary>
    public static class License
    {
        private static string H_UtilE = @"D:\Blank\H_Assistant\H_Assistant\H_UtilE\bin\Debug\H_UtilE.dll";// 加密dll路径
        private static string H_UtilE_Namespace = "H_UtilE.Authorize";// dll空间
        private static string Register = "Register";// 注册函数
        private static string Register2 = "Register2";// 更新操作时间
        private static string Decrypt = "Decrypt";// 解密函数
        static LiteDBHelper liteDBHelper = LiteDBHelper.GetInstance();
        static ILiteCollection<SystemSet> db_sys = liteDBHelper.db.GetCollection<SystemSet>();
        /// <summary>
        /// 验证授权
        /// </summary>
        /// <returns></returns>
        public static string Verify()
        {
            try
            {
                string key = "";// 
                bool isFirst = false;// 是否是第一次
                string cpuId = LiteDBHelper.GetCPUID();
                string md5 = LiteDBHelper.GetMD5(cpuId);
                SystemSet model = db_sys.FindOne(x => x.Name == md5);// 获取注册码
                SystemSet licenseModel = db_sys.FindOne(x => x.Name == "License");// 获取授权码
                if (model == null) { isFirst = true; }
                // 判断是否第一次运行 根据注册表判断
                if (isFirst)
                {
                    key = DllHelp.dllMethod(H_UtilE, H_UtilE_Namespace, Register, null).ToString();//生成注册码
                    if (md5 == key.Substring(12, 32))
                    {
                        RegeditHelp.SetValue(@"SOFTWARE\Microsoft\Windows\", md5, key);// 存注册表
                        db_sys.Insert(new SystemSet { Name = md5, Type = 99, Value = key });// 存数据库
                    }
                    else { return "1001"; }// MD5不一致
                }
                else
                {// 不是第一次
                    object[] keys = { model.Value };
                    key = DllHelp.dllMethod(H_UtilE, H_UtilE_Namespace, Decrypt, keys).ToString();// 解密
                    string regeditKey = RegeditHelp.GetValue(@"SOFTWARE\Microsoft\Windows\", md5).ToString();// 存注册表
                    int firstTime = Convert.ToInt32(key.Substring(32, 8));// 数据库中第一次运行时间
                    int lastTime = Convert.ToInt32(key.Substring(40, 8));// 数据库中最后运行时间
                    int nowTime = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));// 现在时间
                    string dbMd5 = model.Value.Substring(12, 32);// 数据库 MD5
                    if (model.Value == regeditKey)
                    {
                        if (dbMd5 != md5) { return "1002"; }// 本机MD5和注册码MD5 不一致
                                                            //判断最后一次操作时间
                        if (licenseModel == null)// 是否注册
                        { // 未注册  试用期
                            if (firstTime + 1 < nowTime) { return "1003"; }// 试用期超时
                        }
                        else
                        { // 已注册
                            object[] keys3 = { regeditKey, "20230930" };
                            string aa = DllHelp.dllMethod(H_UtilE, H_UtilE_Namespace, "License", keys3).ToString();
                            object[] keys1 = { aa };
                            string key1 = DllHelp.dllMethod(H_UtilE, H_UtilE_Namespace, Decrypt, keys1).ToString();// 解密
                            if (key1.Substring(0, 32) != md5) { return "1004"; }// 本机MD5和授权码MD5 不一致
                            if (nowTime > Convert.ToInt32(key1.Substring(40, 8))) { return "1005"; }// 授权时间过期
                        }
                    }
                    else { return "1006"; }// 注册表和数据库注册码不一致
                    if (lastTime > nowTime) { return "1007"; } // 这次时间和上一次操作时间不一样
                                                               // 更新注册表、数据库的注册码中操作时间
                    object[] keys2 = { DateTime.Now.ToString("yyyyMMdd") };
                    key = DllHelp.dllMethod(H_UtilE, H_UtilE_Namespace, Register2, keys2).ToString();//生成注册码
                    RegeditHelp.SetValue(@"SOFTWARE\Microsoft\Windows\", md5, key);// 存注册表
                    model.Value = key;
                    db_sys.Update(model);// 存数据库
                }
            }
            catch (Exception ex)
            {
                H_Util.Log.WriteException("授权验证异常");
                H_Util.Log.WriteException(ex.StackTrace);
                H_Util.Log.WriteException(ex.Source);
                H_Util.Log.WriteException(ex.Message);
                return "9999";
            }
            return "";// 正常
        }
    }
}
