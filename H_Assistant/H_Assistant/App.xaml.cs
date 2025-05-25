using H_Assistant.Framework;
using H_Assistant.Helper;
using HandyControl.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace H_Assistant
{
    public partial class App
    {
        public App()
        {

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //var splashScreen = new SplashScreen("/Resources/Img/Readme/Cover.png");
            //splashScreen.Show(true);
            //splashScreen.Close(new TimeSpan(0, 0, 23));
            //UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                //把 Handled 属性设为true，表示此异常已处理，程序可以继续运行，不会强制退出
                e.Handled = true;
                if (e.Exception is NotImplementedException)
                {
                    Oops.Oh(LanguageHepler.GetLanguage("StayTuned"));
                    return;
                }
                Growl.Warning(LanguageHepler.GetLanguage("ProgramException"));
            }
            catch (Exception)
            {
                Growl.Warning(LanguageHepler.GetLanguage("ProgramException"));
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //StringBuilder sbEx = new StringBuilder();
            //if (e.IsTerminating)
            //{
            //    sbEx.Append("非UI线程发生致命错误：");
            //}
            //sbEx.Append("非UI线程异常：");
            //if (e.ExceptionObject is Exception)
            //{
            //    sbEx.Append(((Exception)e.ExceptionObject).Message);
            //}
            //else
            //{
            //    sbEx.Append(e.ExceptionObject);
            //}
            Growl.WarningGlobal(LanguageHepler.GetLanguage("ProgramException"));
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            //task线程内未处理捕获
            //Growl.WarningGlobal("Task线程异常：" + e.Exception.Message);
            Growl.WarningGlobal(LanguageHepler.GetLanguage("ProgramException"));
            e.SetObserved();//设置该异常已察觉（这样处理后就不会引起程序崩溃）
        }
    }
}
