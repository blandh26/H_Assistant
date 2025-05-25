using H_Assistant.DocUtils.Dtos;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace H_Assistant.DocUtils
{
    public static class RazorTpl
    {
        static RazorTpl()
        {
            var config = new TemplateServiceConfiguration();
            config.Language = Language.CSharp;
            config.EncodedStringFactory = new RawStringFactory();
            config.DisableTempFileLocking = true;
            config.ReferenceResolver = new RazorReferenceResolver();
            //config.EncodedStringFactory = new HtmlEncodedStringFactory();
            var service = RazorEngineService.Create(config);
            Engine.Razor = service;
        }

        public static string RazorRender(this FileInfo tpl_file, object model, string encoding = "utf-8")
        {
            try
            {
                var tpl_text = File.ReadAllText(tpl_file.FullName, System.Text.Encoding.GetEncoding(encoding));

                return Engine.Razor.RunCompile(tpl_text, Md5(tpl_text), null, model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string RazorRender(this string tpl_text, DBDto model)
        {
            try
            {
                return Engine.Razor.RunCompile(tpl_text, Md5(tpl_text), typeof(DBDto), model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string RazorRender(this string tpl_text, TableDto model)
        {
            try
            {
                return Engine.Razor.RunCompile(tpl_text, Md5(tpl_text), typeof(TableDto), model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string RazorRender(this string tpl_text, SqlCode model)
        {
            try
            {
                return Engine.Razor.RunCompile(tpl_text, Md5(tpl_text), typeof(SqlCode), model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string RazorRender(this string tpl_text, ChmHHP model)
        {
            try
            {
                return Engine.Razor.RunCompile(tpl_text, Md5(tpl_text), typeof(ChmHHP), model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 字符串的Md5值
        /// </summary>
        private static string Md5(string value)
        {
            if (value == null)
                return null;
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}
