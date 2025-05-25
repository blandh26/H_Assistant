using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H_Util
{
   public static class RegeditHelp
    {
        /// <summary>
        /// 设置开机自动启动
        /// </summary>
        /// <param name="key">应用名称</param>
        /// <param name="key">路径</param>
        /// <returns></returns>
        public static void AutoStart(string key,string value)
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", true);
            regKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\");
            regKey.SetValue(key, value);
        }

        /// <summary>
        /// 设置注册表值
        /// </summary>
        /// <param name="url">注册表地址</param>
        /// <param name="key">key</param>
        /// <param name="key">value</param>
        /// <returns></returns>
        public static void SetValue(string url,string key, string value)
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(url, true);
            regKey = Registry.LocalMachine.CreateSubKey(url);
            regKey.SetValue(key, value);
        }

        /// <summary>
        /// 获取注册表值
        /// </summary>
        /// <param name="url">注册表地址</param>
        /// <param name="key">key</param>
        /// <param name="key">value</param>
        /// <returns></returns>
        public static object GetValue(string url, string key)
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(url, true);
            regKey = Registry.LocalMachine.CreateSubKey(url);
           return regKey.GetValue(key);
        }
    }
}
