using H_Assistant.Framework.Const;
using H_Assistant.Framework.liteDbModel;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Security.Cryptography;

namespace H_Assistant.Framework
{
    public class LiteDBHelper
    {
        public static string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "H_Assistant");
        public string connstr = Path.Combine(BasePath, "H_Assistant.db");//没有数据库会创建数据库
        public LiteDatabase db;
        public ConnectionString con;
        private static LiteDBHelper _instance;

        private static readonly string obj = "H_Assistant";
        public static LiteDBHelper GetInstance()
        {
            if (_instance == null)
            {
                lock (obj)  //加锁防止多线程
                {
                    if (_instance == null)
                    {
                        _instance = new LiteDBHelper();
                    }
                }
            }
            return _instance;
        }

        public LiteDBHelper()
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
            con = new ConnectionString(connstr);
            con.Password = GetMD5(GetCPUID());// 获得CPU编号 加密
            db = new LiteDatabase(con);

            //表已存在不会重复创建
            db.GetCollection<ConnectConfigs>();
            db.GetCollection<GroupInfo>();
            db.GetCollection<TagInfo>();
            db.GetCollection<GroupObjects>();
            db.GetCollection<TagObjects>();
            db.GetCollection<SystemSet>();
            db.GetCollection<ClipboardModel>();
            Init();
        }

        public List<SystemSet> InitValue = new List<SystemSet>
        {
            new SystemSet{Name = SysConst.Sys_IsGroup,Type = 1,Value = "true"},
            new SystemSet{Name = SysConst.Sys_IsMultipleTab,Type = 1,Value = "true"},
            new SystemSet{Name = SysConst.Sys_IsLikeSearch,Type = 1,Value = "true"},
            new SystemSet{Name = SysConst.Sys_IsContainsObjName,Type = 1,Value = "true"},
            new SystemSet{Name = SysConst.Sys_IsShowSaveWin,Type = 1,Value = "true"},
            new SystemSet{Name = SysConst.Sys_LeftMenuType,Type = 2,Value = "1"},
            new SystemSet{Name = SysConst.Sys_SelectedConnection,Type = 3,Value = ""},
            new SystemSet{Name = SysConst.Sys_SelectedDataBase,Type = 3,Value = ""},

            new SystemSet{Name = SysConst.Sys_ScreenCaptureLast,Type = 99,Value = ""},// 最后一次截图区域
            new SystemSet{Name = SysConst.Sys_ScreenCaptureTitle,Type = 99,Value = "Contrast${yyyy-MM-dd HH:mm:ss}"},// 对比窗体名称
            new SystemSet{Name = SysConst.Sys_ScreenCaptureOpacity,Type = 99,Value = "60"},// 对比窗体透明度
            new SystemSet{Name = SysConst.Sys_ScreenCaptureKey,Type = 99,Value = "0|1|0|Q"},// 截图快捷键
            new SystemSet{Name = SysConst.Sys_ScreenCaptureLastKey,Type = 99,Value = "0|1|0|W"},// 最后一次截图快捷键
            new SystemSet{Name = SysConst.Sys_ScreenCaptureDiffKey,Type = 99,Value = "0|1|0|E"},// 对比截图快捷键
            new SystemSet{Name = SysConst.Sys_ClipboardKey,Type = 99,Value = "1|0|0|S"},// 剪贴板快捷键
            new SystemSet{Name = SysConst.Sys_Language,Type = 99,Value = "cn"},// 语言设定
            new SystemSet{Name = SysConst.Sys_AutoStart,Type = 99,Value = "0"},// 开机自动启动 0 不启动 1 启动
            new SystemSet{Name = SysConst.Sys_Groups,Type = 99,Value = "5"},// 工具箱数
            new SystemSet{Name = SysConst.Sys_RegEx,Type = 99,Value = ""},// 正则表达式（用于统计代码行数）
            new SystemSet{Name = SysConst.Sys_TestKeyword,Type = 99,Value = ""},// 测试用关键字设置

        };

        public List<ExeModel> InitExe = new List<ExeModel>
        {
            new ExeModel{Id = 1,Order = 9999,Path = "System",Title = "UcTextInsertTitle",Icon = "/H_Assistant;component/Resources/Img/toolIcon/text-insert@2x.png"},
            new ExeModel{Id = 2,Order = 9999,Path = "System",Title = "UcBase64Title",Icon = "/H_Assistant;component/Resources/Img/toolIcon/base64@2x.png"},
            new ExeModel{Id = 3,Order = 9999,Path = "System",Title = "UcStrCountTitle",Icon = "/H_Assistant;component/Resources/Img/toolIcon/word-count@2x.png"},
            new ExeModel{Id = 4,Order = 9999,Path = "System",Title = "UcJWTTitle",Icon = "/H_Assistant;component/Resources/Img/toolIcon/jwt@2x.png"},
            new ExeModel{Id = 5,Order = 9999,Path = "System",Title = "UcBase64ToImgTitle",Icon = "/H_Assistant;component/Resources/Img/toolIcon/base64ToImg@2x.png"},
            new ExeModel{Id = 6,Order = 9999,Path = "System",Title = "UcHexTitle",Icon = "/H_Assistant;component/Resources/Img/toolIcon/hex-encoding@2x.png"},
            new ExeModel{Id = 7,Order = 9999,Path = "System",Title = "UcIcoTitle",Icon = "/H_Assistant;component/Resources/Img/toolIcon/ico@2x.png"},
        };
        private void Init()
        {
            var sysList = db.GetCollection<SystemSet>();
            InitValue.ForEach(x =>
            {
                if (sysList.Count(m => m.Name.Equals(x.Name)) == 0)
                {
                    sysList.Insert(x);
                }
            });
            var sysExeModel = db.GetCollection<ExeModel>();
            InitExe.ForEach(x =>
            {
                if (sysExeModel.Count(m => m.Id.Equals(x.Id)) == 0)
                {
                    sysExeModel.Insert(x);
                }
            });
        }

        public int Add<T>(T model)
        {
            var db_table = db.GetCollection<T>();
            return db_table.Insert(model);
        }

        public int Add<T>(List<T> model)
        {
            var db_table = db.GetCollection<T>();
            return db_table.Insert(model);
        }

        public bool Update<T>(T model)
        {
            var db_table = db.GetCollection<T>();
            return db_table.Update(model);
        }

        public bool Delete<T>(T model)
        {
            var db_table = db.GetCollection<T>();
            return db_table.Update(model);
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predExpr) where T : new()
        {
            var db_table = db.GetCollection<T>();
            return db_table.FindOne(predExpr);
        }

        public List<T> ToList<T>() where T : new()
        {
            var db_table = db.GetCollection<T>();
            return db_table.Query().ToList();
        }

        public List<T> ToList<T>(Expression<Func<T, bool>> predExpr) where T : new()
        {
            var db_table = db.GetCollection<T>();
            return db_table.Find(predExpr).ToList();
        }

        public List<T> Query<T>() where T : new()
        {
            var db_table = db.GetCollection<T>();
            return db_table.Query().ToList();
        }

        public bool IsAny<T>(Expression<Func<T, bool>> predExpr) where T : new()
        {
            var db_table = db.GetCollection<T>();
            return db_table.Count(predExpr) > 0;
        }

        public void SetSysValue(string name, object value)
        {
            var db_table = db.GetCollection<SystemSet>();
            var sysSet = db_table.FindOne(x => x.Name == name);
            if (sysSet == null)
            {
                return;
            }
            sysSet.Value = value.ToString();
            db_table.Update(sysSet);
        }

        public bool GetSysBool(string name)
        {
            var db_table = db.GetCollection<SystemSet>();
            var type = SysDataType.BOOL.GetHashCode();
            var sysSet = db_table.FindOne(x => x.Name.Equals(name) && x.Type == type);
            if (sysSet == null)
            {
                return false;
            }
            return Convert.ToBoolean(sysSet.Value);
        }

        public int GetSysInt(string name)
        {
            var db_table = db.GetCollection<SystemSet>();
            var type = SysDataType.INT.GetHashCode();
            var sysSet = db_table.FindOne(x => x.Name.Equals(name) && x.Type == type);
            if (sysSet == null)
            {
                return 0;
            }
            return Convert.ToInt32(sysSet.Value);
        }

        public string GetSysString(string name)
        {
            var db_table = db.GetCollection<SystemSet>();
            var type = SysDataType.STRING.GetHashCode();
            var sysSet = db_table.FindOne(x => x.Name.Equals(name) && x.Type == type);
            if (sysSet == null)
            {
                return "";
            }
            return sysSet.Value;
        }

        /// <summary>
        /// 获得CPU编号
        /// </summary>
        /// <returns></returns>
        public static string GetCPUID()
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
        /// 生成MD5
        /// </summary>
        /// <param name="sDataIn">加密内容</param>
        /// <returns></returns>
        public static string GetMD5(string sDataIn)
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
    }

    /// <summary>
    /// 系统设置数据类型
    /// </summary>
    public enum SysDataType
    {
        BOOL = 1,
        INT = 2,
        STRING = 3,
        JSON = 4
    }
}
