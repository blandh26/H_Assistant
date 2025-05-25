using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace H_Util
{
   public class DllHelp
    {
        /// <summary>
        /// 动态编译并执行代码
        /// </summary>
        /// <param name="code">代码</param>
        /// <param name="newPath">输出dll的路径</param>
        /// <returns>返回输出内容</returns>
        public static CompilerResults debugRun(string code, string newPath)
        {
            CSharpCodeProvider complier = new CSharpCodeProvider();
            string SqlSugar = AppDomain.CurrentDomain.BaseDirectory + "SqlSugar.dll";
            //设置编译参数
            CompilerParameters paras = new CompilerParameters();
            //引入第三方dll
            paras.ReferencedAssemblies.Add(@"System.dll");
            paras.ReferencedAssemblies.Add(@"System.Data.dll");
            paras.ReferencedAssemblies.Add(SqlSugar);
            //paras.ReferencedAssemblies.Add(@"System.configuration.dll");
            //paras.ReferencedAssemblies.Add(@"System.Management.dll");
            //paras.ReferencedAssemblies.Add(@"System.Web.dll");
            //paras.ReferencedAssemblies.Add(@"System.Xml.dll");
            //paras.ReferencedAssemblies.Add(@"F:\AuthorizationService\Lib\Newtonsoft.Json\Net20\Newtonsoft.Json.dll");
            //引入自定义dll
            //paras.ReferencedAssemblies.Add(@"D:\自定义方法\自定义方法\bin\LogHelper.dll");
            //是否内存中生成输出
            paras.GenerateInMemory = false;
            //是否生成可执行文件
            paras.GenerateExecutable = false;     //编译成exe还是dll
            paras.GenerateInMemory = false;       //是否写入内存,不写入内存就写入磁盘
            paras.OutputAssembly = newPath;       //输出路径
            paras.IncludeDebugInformation = false;//是否产生pdb调试文件      默认是false
            //命名空间定义结束
            //System.Diagnostics.Debug.WriteLine(code); // 调试用。注释掉
            CompilerResults result;
            try
            {
                //编译代码
                result = complier.CompileAssemblyFromSource(paras, code);
                //Assembly assembly = result.CompiledAssembly;//  获取编译后的程序集。
                foreach (var item in result.Errors)
                {
                    // 错误信息
                    Console.WriteLine(item.ToString());
                }
            }
            catch (Exception ex){throw;}
            return result;
        }
        /// <summary>
        /// 调用dll 函数
        /// </summary>
        /// <param name="dll">dll路径</param>
        /// <param name="namespaceStr">命名空间</param>
        /// <param name="functionName">函数名称</param>
        /// <param name="paramsList">参数</param>
        /// <returns></returns>
        public static object dllMethod(string dll, string namespaceStr, string functionName,object[] paramsList) {
            Assembly outerAsm = Assembly.LoadFrom(dll);
            Type type = outerAsm.GetType(namespaceStr);//调用类型
            object className = Activator.CreateInstance(type);//创建指定类型实例
            MethodInfo method = type.GetMethod(functionName);//调用方法
            object obj = method.Invoke(className, paramsList);//Invoke调用方法
            return obj;
        }
    }
}
