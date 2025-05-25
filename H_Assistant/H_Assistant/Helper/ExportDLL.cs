using H_Assistant.Framework;
using H_Assistant.Framework.liteDbModel;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Helper;
using H_Util;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace H_Assistant.DocUtils
{
    public static class ExportDLL
    {
        /// <summary>
        /// 数据库ip_数据库名_数据库账号.dll
        /// </summary>
        /// <param name="selectDatabase"></param>
        /// <param name="SelectedConnection"></param>
        public static void CsharpExportDLLick(DataBase selectDatabase, ConnectConfigs SelectedConnection)
        {
            var dbInstance = ExporterFactory.CreateInstance(SelectedConnection.DbType, SelectedConnection.SelectedDbConnectString(selectDatabase.DbName), selectDatabase.DbName);
            List<DbTableInfo> list = dbInstance.GetTableInfoList();
            string path = AppDomain.CurrentDomain.BaseDirectory + "dll\\";
            #region 模板字符串
            string classText = @"using System;
using System.Text;
using SqlSugar;
namespace Models
{";
            string classTable = @"    ///<summary>
    /// {tableDesc}
    ///</summary>
    [SugarTable(""{tableNname}"")]
    public partial class {tableNname}
    {";
            string classField = @"           /// <summary>
           /// Desc:{Desc}
           /// Default:{Default}
           /// Nullable:{Nullable}
           /// </summary>
           {pk}
           public {type} {field} {get;set;}";
            string ServerAddress = SelectedConnection.ServerAddress;
            string UserName = SelectedConnection.UserName;
            #endregion
            Task.Factory.StartNew(() =>
            {
                //判断文档路径是否存在
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    classText += Environment.NewLine;
                    classText += classTable
                    //.Replace("{tableDesc}", list[i].Description == null ? "" : list[i].Description.Replace("\r\n", ""))
                    .Replace("{tableNname}", list[i].Name);
                    classText += Environment.NewLine;
                    Columns col = dbInstance.GetColumnInfoById(list[i].Name);
                    foreach (var item in col)
                    {
                        classText += classField
                            .Replace("{pk}", item.Value.IsPrimaryKey == true ? "[SugarColumn(IsPrimaryKey = true)]" : "")
                            //.Replace("{Desc}", item.Value.Comment == null ? "" : item.Value.Comment.Replace("\r\n", ""))
                            //.Replace("{Default}", item.Value.DefaultValue == null ? "" : item.Value.DefaultValue.Replace("\r\n", ""))
                            //.Replace("{Nullable}", item.Value.IsNullable.ToString())
                            .Replace("{type}", item.Value.CSharpType)
                            .Replace("{field}", item.Value.DisplayName);
                        classText += Environment.NewLine;
                    }
                    classText += "    }";
                    classText += Environment.NewLine;
                }
                classText += "}";
                TestHelper.DllName = ServerAddress + "_" + selectDatabase.DbName + "_" + UserName + ".dll";
                string dllPath = path + TestHelper.DllName;
                bool fileExist = File.Exists(dllPath);
                if (fileExist) { File.Delete(dllPath); }
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(classText);
                DllHelp.debugRun(classText, dllPath);
                //Dispatcher.Invoke(() =>
                //{
                //    Oops.God("生成数据库有关DLL成功，用于测试辅助。");
                //});
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 返回实体类语句
        /// </summary>
        /// <param name="selectDatabase">当前数据库</param>
        /// <param name="SelectedConnection">当前连接</param>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public static string CsharpEntity(DataBase selectDatabase, ConnectConfigs SelectedConnection, string tableName)
        {
            try
            {
                var dbInstance = ExporterFactory.CreateInstance(SelectedConnection.DbType, SelectedConnection.SelectedDbConnectString(selectDatabase.DbName), selectDatabase.DbName);
                List<DbTableInfo> list = dbInstance.GetTableInfoList();
                list = list.Where(x => x.Name == tableName).ToList();
                string path = AppDomain.CurrentDomain.BaseDirectory + "dll\\";
                #region 模板字符串
                string classText = @"using System;" + Environment.NewLine;
                classText += @"using System.Text;" + Environment.NewLine;
                classText += @"using System.ComponentModel.DataAnnotations;" + Environment.NewLine;
                classText += @"using System.ComponentModel.DataAnnotations.Schema;" + Environment.NewLine;
                classText += @"namespace ModelsNamespace {" + Environment.NewLine;
                string classTable = @"    ///<summary>" + Environment.NewLine;
                classTable += "    /// {tableDesc}" + Environment.NewLine;
                classTable += "    ///</summary>" + Environment.NewLine;
                classTable += "    [Table({tableNname1})]" + Environment.NewLine;
                classTable += "    public class {tableNname}" + Environment.NewLine;
                classTable += "    {" + Environment.NewLine;
                string classField = @"           /// <summary>
           /// Desc:{Desc}
           /// Default:{Default}
           /// Nullable:{Nullable}
           /// </summary>{attribute}
           public {type} {field} {get;set;}";
                string ServerAddress = SelectedConnection.ServerAddress;
                string UserName = SelectedConnection.UserName;
                #endregion
                for (int i = 0; i < list.Count; i++)
                {
                    classText += Environment.NewLine;
                    classText += classTable
                    .Replace("{tableDesc}", list[i].Description == null ? "" : list[i].Description.Replace("\r\n", ""))
                    .Replace("{tableNname}", list[i].Name)
                    .Replace("{tableNname1}", "\"" + list[i].Name + "\"");
                    classText += Environment.NewLine;
                    Columns col = dbInstance.GetColumnInfoById(list[i].Name);
                    int order = 0;// 主键顺序
                    foreach (var item in col)
                    {
                        classText += classField
                            .Replace("{Desc}", item.Value.Comment == null ? "" : item.Value.Comment.Replace("\r\n", ""))
                            .Replace("{Default}", item.Value.DefaultValue == null ? "" : item.Value.DefaultValue.Replace("\r\n", ""))
                            .Replace("{Nullable}", item.Value.IsNullable.ToString())
                            .Replace("{type}", item.Value.CSharpType)
                            .Replace("{field}", item.Value.DisplayName);
                        string attribute = "";//属性
                        if (item.Value.IsPrimaryKey)
                        {
                            attribute = "[Column(\"{Desc}\",TypeName =\"{type}\",Order ={Order})"
                            .Replace("{Desc}", item.Value.DisplayName)
                            .Replace("{type}", item.Value.Comment == null ? "" : item.Value.DataType.Replace("\r\n", "").Trim())
                            .Replace("{Order}", order.ToString());
                            order++;
                        }
                        else
                        {
                            attribute = "[Column(\"{Desc}\")"
                            .Replace("{Desc}", item.Value.DisplayName);
                        }
                        if (item.Value.IsPrimaryKey) { attribute += ",Key"; }
                        else if (!item.Value.IsNullable) { attribute += ",Required"; }
                        if (item.Value.CSharpType.ToLower() == "string") { attribute += $",StringLength({item.Value.LengthName.Replace("(","").Replace(")", "")})"; }
                        attribute += "]";
                        classText = classText.Replace("{attribute}", Environment.NewLine + "           " + attribute);
                        classText += Environment.NewLine;
                        classText += Environment.NewLine;
                    }
                    classText += "    }";
                    classText += Environment.NewLine;
                }
                classText += "}";
                return classText;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
