using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Serilog;
using System.Configuration;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Wesky.Quant.Client.Domain.Statics;
using Wesky.Quant.Client.Infrastructure.Tools;
using Wesky.Quant.Client.QuantExtensionServices;
using Wesky.Quant.Client.ViewModels;
using Wesky.Quant.Client.Views;

namespace Wesky.Quant.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        public App()
        {
            Startup += AppStartup;
            Exit += AppExit;
        }


        private void AppExit(object sender, ExitEventArgs e)
        {
            //
        }

        private void AppStartup(object sender, StartupEventArgs e)
        {
            //UI线程未捕获异常处理事件
            DispatcherUnhandledException += App_DispatcherUnhandledException; ;
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException; ;
            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; ;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                string msg;
                if (e.ExceptionObject is Exception ex)
                {
                    msg = ExceptionToString(ex, "非UI线程");
                }
                else
                {
                    msg = $"发生了一个错误！信息:{e.ExceptionObject}";
                }
                Log.Error(msg);
            }
            catch (Exception ex)
            {
                string msg = ExceptionToString(ex, "非UI线程 处理函数");
                Log.Error(msg);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                string msg = ExceptionToString(e.Exception, "Task线程");
                Log.Error(msg);
                e.SetObserved(); //设置该异常已察觉（这样处理后就不会引起程序崩溃）
            }
            catch (Exception ex)
            {
                string msg = ExceptionToString(ex, "Task线程 处理函数");
                Log.Error(msg);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                string msg = ExceptionToString(e.Exception, "UI线程");
                Log.Error(msg);
            }
            catch (Exception ex)
            {
                string msg = ExceptionToString(ex, "UI线程 处理函数");
                Log.Error(msg);
            }
        }


        /// <summary>
        /// 提取异常信息
        /// </summary>
        private static string ExceptionToString(Exception ex, string info)
        {
            StringBuilder str = new StringBuilder($"{DateTime.Now}, {info}发生了一个错误！{Environment.NewLine}");
            if (ex.InnerException == null)
            {
                str.Append($"【对象名称】：{ex.Source}{Environment.NewLine}");
                str.Append($"【异常类型】：{ex.GetType().Name}{Environment.NewLine}");
                str.Append($"【详细信息】：{ex.Message}{Environment.NewLine}");
                str.Append($"【堆栈调用】：{ex.StackTrace}");
            }
            else
            {
                str.Append($"【对象名称】：{ex.InnerException.Source}{Environment.NewLine}");
                str.Append($"【异常类型】：{ex.InnerException.GetType().Name}{Environment.NewLine}");
                str.Append($"【详细信息】：{ex.InnerException.Message}{Environment.NewLine}");
                str.Append($"【堆栈调用】：{ex.InnerException.StackTrace}");
            }
            return str.ToString();
        }


        static volatile int currentMainThreadID = Thread.CurrentThread.ManagedThreadId;

        //这个属性表示当前执行线程是否在主线程中运行
        public static bool IsRunInMainThread { get { return Thread.CurrentThread.ManagedThreadId == currentMainThreadID; } }

        protected override Window CreateShell()
        {
            return Container.Resolve<QuantMainView>();
        }

        private static Mutex instance;
        protected override void InitializeShell(Window shell)
        {

            instance = new Mutex(true, "WeskyQuantClient", out bool createdNew); // 防止同一台电脑上多开

            if (createdNew)
            {
                instance.ReleaseMutex();
                base.InitializeShell(shell);

            }
            else
            {
                MessageBox.Show("客户端已经启动，请勿重复启动");
                System.Windows.Application.Current?.Shutdown();
            }

        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            /// 全局对象注册
            containerRegistry.Register<Dispatcher>(() => System.Windows.Application.Current.Dispatcher);
            //  注册弹出窗
            containerRegistry.RegisterDialogWindow<DialogWindow>();

            // 注册系统服务
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(new ReadJsonHelper());
            serviceCollection.AddSerilogConfiguration(); // 初始化日志配置信息（写入本地文件）
            serviceCollection.AddHttpClient();
            var provide = serviceCollection.BuildServiceProvider();
            QuantServiceProviderStatic.QuantServiceProvider = provide;
             // 注册编码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


        }
    }

}
