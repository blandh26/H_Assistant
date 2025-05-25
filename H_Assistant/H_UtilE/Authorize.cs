using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace H_UtilE
{
    /// <summary>
    /// 生成和验证  注册码和授权码都是88位
    /// </summary>
    public class Authorize
    {
        /// <summary>
        /// 生成注册码（CPU、主板编号、第一次时间）
        /// 第一次自动提供7天试用授权（防止每次都重新安装）
        /// 时间限制到期就不可以用
        /// 1. MD5（机器识别码） + cpu（10）+ 时间（第一次运行时间）+ 默认是8个0（注册成功后是到期时间）+ 最后操作日期
        /// 2. 1信息加密+密钥混淆
        /// 3. 注册码
        /// </summary>
        /// <returns>返回注册码</returns>
        public string Register()
        {
            string fristData = DateTime.Now.ToString("yyyyMMdd");// 日期倒序
            string md5 = GetMD5(GetCPUID());
            string key = fristData+"00000000" ;// 第一次时间 
            key = ReverseE(key);//倒序
            string secret = GenerateStr(8);//生成8位随机码
            key = Des_Encrypt(key, secret);//加密
            key = GenerateStr(8)+secret.Substring(0, 4) + md5 + key + secret.Substring(4)+ GenerateStr(8);// 随机码8+密钥前四位+MD5+加密后文字+密钥后四位+随机码8
            return key;
        }
        /// <summary>
        /// 更新注册码中最后操作时间
        /// </summary>
        /// <param name="lastTime"></param>
        /// <returns></returns>
        public string Register2(string lastTime)
        {
            string fristData = DateTime.Now.ToString("yyyyMMdd");// 日期倒序
            string md5 = GetMD5(GetCPUID());
            string key = fristData + lastTime;// 第一次时间 + 操作时间
            key = ReverseE(key);//倒序
            string secret = GenerateStr(8);//生成8位随机码
            key = Des_Encrypt(key, secret);//加密
            key = GenerateStr(8) + secret.Substring(0, 4) + md5 + key + secret.Substring(4) + GenerateStr(8);// 随机码8+密钥前四位+MD5+加密后文字+密钥后四位+随机码8
            return key;
        }

        /// <summary>
        /// 生生授权码
        /// </summary>
        /// <param name="key">注册码</param>
        /// <param name="licenseTime">到期时间yyyyMMdd</param>
        /// <returns></returns>
        public string License(string key, string licenseTime)
        {
            string regkey = Decrypt(key);
            string likey = regkey.Substring(32,8) + licenseTime;// 第一次时间 + 操作时间
            likey = ReverseE(likey);//倒序
            string secret = GenerateStr(8);//生成8位随机码
            likey = Des_Encrypt(likey, secret);//加密
            key = GenerateStr(8) + secret.Substring(0, 4) + regkey.Substring(0, 32) + likey + secret.Substring(4) + GenerateStr(8);// 随机码和和加密后文字 固定格式混淆
            return key;
        }
        /// <summary>
        /// 验证没有问题
        /// </summary>
        /// <returns>返回结果码</returns>
        public string Decrypt(string key)
        {
            key = key.Substring(8);
            key = key.Substring(0, key.Length - 8);//剔除随机数
            string secret = key.Substring(0, 4)+ key.Substring(key.Length-4, 4);//密钥
            string md5 = key.Substring(4, 32);//获取md5
            key = key.Substring(36, key.Length - 40);// 加密后文字
            key = Des_Decrypt(key, secret);// 解密后文字
            return md5+ReverseE(key);// 倒叙后文字
        }
         
        /// <summary>
        /// 获得CPU编号
        /// </summary>
        /// <returns></returns>
        private string GetCPUID()
        {
            string cpuid = "";
            try
            {
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc) { cpuid = mo.Properties["ProcessorId"].Value.ToString(); }
            }
            catch { return ""; }
            return cpuid;
        }

        /// <summary>
        /// 生成随机码
        /// </summary>
        /// <param name="num">位数</param>
        /// <returns></returns>
        private string GenerateStr(int num)
        {
            string word = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            Random ra = new Random();
            string str = "";
            for (int j = 0; j < num; j++) { str += word[ra.Next(62)]; }
            return str;
        }

        /// <summary>
        /// 生成随机码
        /// </summary>
        /// <param name="num">位数</param>
        /// <returns></returns>
        private string GenerateNumber(int num)
        {
            string word = "1234567890";
            Random ra = new Random();
            string str = "";
            for (int j = 0; j < num; j++) { str += word[ra.Next(10)]; }
            return str;
        }

        /// <summary>
        /// 倒序字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string ReverseE(string text)
        {
            char[] cArray = text.ToCharArray();
            StringBuilder reverse = new StringBuilder();
            for (int i = cArray.Length - 1; i > -1; i--) { reverse.Append(cArray[i]); }
            return reverse.ToString();
        }

        /// <summary>
        /// 生成MD5
        /// </summary>
        /// <param name="sDataIn">加密内容</param>
        /// <returns></returns>
        private string GetMD5(string sDataIn)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = System.Text.Encoding.UTF8.GetBytes(sDataIn);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToLower();
        }
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="sKey">密钥</param>
        /// <returns></returns>
        private string Des_Encrypt(string str, string sKey)
        {
            byte[] inputByteArray = Encoding.Default.GetBytes(str);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = Encoding.UTF8.GetBytes(sKey);// 秘钥
                des.IV = Encoding.UTF8.GetBytes(sKey);// 初始化向量
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                    }
                    var retB = Convert.ToBase64String(ms.ToArray());
                    return retB;
                }
            }
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="sKey">密钥</param>
        /// <returns></returns>
        private string Des_Decrypt(string pToDecrypt, string sKey)
        {
            byte[] inputByteArray = Convert.FromBase64String(pToDecrypt);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = Encoding.UTF8.GetBytes(sKey);
                des.IV = Encoding.UTF8.GetBytes(sKey);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        // 如果两次秘钥不一样，这一步可能会引发异常
                        cs.FlushFinalBlock();
                    }
                    return Encoding.Default.GetString(ms.ToArray());
                }
            }
        }
    }

}