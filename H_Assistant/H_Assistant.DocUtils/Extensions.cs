using H_Assistant.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace H_Assistant.DocUtils
{
    public static class Extensions
    {
        public static string GetResourceContent(this Assembly assembly, string name)
        {
            var buffer = assembly.GetResourceBuffer(name);
            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        public static byte[] GetResourceBuffer(this Assembly assembly, string name)
        {
            var stream = assembly.GetManifestResourceStream(name);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Dispose();
            return buffer;
        }

        public static EM GetEnum<EM>(this string enumName)
            where EM : struct, Enum
        {
            if (!Enum.TryParse<EM>(enumName, out EM em))
            {
                throw new ArgumentException(LanguageHepler.GetLanguage("ExtensionsEnumerationConversionFailed")+"！", nameof(enumName));
            }
            return em;
        }

        public static Dictionary<string, string> ToDictionary(this NameValueCollection nvc)
        {
            var dict = new Dictionary<string, string>();
            foreach (var key in nvc.AllKeys)
            {
                dict.Add(key, nvc[key]);
            }
            return dict;
        }

        /// <summary>
        /// 截断异常信息，避免信息过长造成用户体验不好
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string ToMsg(this Exception exception)
        {
            return exception.Message.Length < 200 ? exception.Message : $"{exception.Message.Substring(0, 200)}...";
        }

        #region MarkDown

        public static string MarkDown<T>(this IEnumerable<T> objs, params string[] excludePropNames)
        {
            if (objs == null)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            var minus = 0;
            var type = typeof(T);
            var props = type.GetProperties();
            var lstTmp = new List<string>();

            sb.Append(" | ");
            foreach (var prop in props)
            {
                if (excludePropNames != null && excludePropNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                {
                    minus++;
                    continue;
                }
                var headName = ((prop.GetCustomAttributes(typeof(DisplayAttribute), false)?.FirstOrDefault() as DisplayAttribute)?.Name) ?? prop.Name;
                lstTmp.Add(headName);
            }
            sb.Append(string.Join(" | ", lstTmp));
            sb.Append(" | ");
            sb.AppendLine();

            lstTmp = new List<string>();
            sb.Append(" | ");
            for (int j = 0; j < props.Length - minus; j++)
            {
                if (props[j].Name == "TableName" || props[j].Name == "Comment")
                {
                    lstTmp.Add(":---");
                }
                else
                {
                    lstTmp.Add(":---:");
                }
            }
            sb.Append(string.Join(" | ", lstTmp));
            sb.Append(" | ");

            foreach (var obj in objs)
            {
                if (obj == null)
                {
                    continue;
                }
                sb.AppendLine();
                sb.Append(" | ");
                lstTmp = new List<string>();
                foreach (var prop in props)
                {
                    if (excludePropNames != null && excludePropNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    var value = (prop.GetValue(obj, null) ?? string.Empty).ToString();
                    lstTmp.Add(value);
                }
                sb.Append(string.Join(" | ", lstTmp));
                sb.Append(" | ");
            }
            var md = sb.ToString();
            return md;
        }

        public static string MarkDown(this DataTable data, params string[] excludeColNames)
        {
            if (data == null)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            var minus = 0;
            var lstTmp = new List<string>();

            sb.Append(" | ");
            foreach (DataColumn dc in data.Columns)
            {
                if (excludeColNames != null && excludeColNames.Contains(dc.ColumnName, StringComparer.OrdinalIgnoreCase))
                {
                    minus++;
                    continue;
                }
                lstTmp.Add(dc.ColumnName);
            }
            sb.Append(string.Join(" | ", lstTmp));
            sb.Append(" | ");
            sb.AppendLine();

            lstTmp = new List<string>();
            sb.Append(" | ");
            for (int j = 0; j < data.Columns.Count - minus; j++)
            {
                lstTmp.Add(":---:");
            }
            sb.Append(string.Join(" | ", lstTmp));
            sb.Append(" | ");

            foreach (DataRow dr in data.Rows)
            {
                sb.AppendLine();
                sb.Append(" | ");
                lstTmp = new List<string>();
                foreach (DataColumn dc in data.Columns)
                {
                    if (excludeColNames != null && excludeColNames.Contains(dc.ColumnName, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    var value = (dr[dc] ?? string.Empty).ToString();
                    lstTmp.Add(value);
                }
                sb.Append(string.Join(" | ", lstTmp));
                sb.Append(" | ");
            }
            var md = sb.ToString();
            return md;
        }

        #endregion

        #region Xml序列化/反序列化

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="xml">XML字符串</param>
        /// <returns></returns>
        public static object DeserializeXml(this Type type, string xml)
        {
            using (StringReader sr = new StringReader(xml))
            {
                //XmlSerializer xmldes = new XmlSerializer(type);
                XmlSerializer xmldes = XmlSerializer.FromTypes(new[] { type })[0];
                return xmldes.Deserialize(sr);
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string SerializeXml<T>(this T obj)
            where T : new()
        {
            MemoryStream Stream = new MemoryStream();
            //XmlSerializer xml = new XmlSerializer(obj.GetType());
            XmlSerializer xml = XmlSerializer.FromTypes(new[] { typeof(T) })[0];
            //序列化对象
            xml.Serialize(Stream, obj);
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();

            return str;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T DeserializeXml<T>(this T type, string xml) where T : new()
        {
            using (StringReader sr = new StringReader(xml))
            {
                XmlSerializer xmldes = XmlSerializer.FromTypes(new[] { typeof(T) })[0];
                return (T)xmldes.Deserialize(sr);
            }
        }
        #endregion
    }
}
