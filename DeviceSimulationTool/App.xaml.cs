﻿using Min_Helpers;
using Min_Helpers.LogHelper;
using Min_Helpers.PrintHelper;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DeviceSimulationTool
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        public static Print PrintService { get; set; } = null;

        public static Log LogService { get; set; } = null;

        public static string CommonPath { get; } = AppDomain.CurrentDomain.BaseDirectory;
        public static string AppName { get; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        public static int AppProcessId { get; } = Process.GetCurrentProcess().Id;

        private static DirectoryInfo _logPath = new DirectoryInfo($"{App.CommonPath}\\logs");
        public static DirectoryInfo LogPath
        {
            get
            {
                _logPath.Refresh();

                if (!_logPath.Exists)
                {
                    _logPath.Create();
                }

                return _logPath;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

            try
            {
                LogService = new Log();
                LogService.LogPath = LogPath.FullName;

                PrintService = new Print(LogService);

                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
                this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;

                LogService.Write("");
                PrintService.Log("App, Start", Print.EMode.info);

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                PrintService.Log($"App, Error, {ex.Message}", Print.EMode.info);

                App.Current.Shutdown(1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            PrintService.Log("App, Exit", Print.EMode.info);

            base.OnExit(e);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = ExceptionHelper.GetReal(e.Exception);
            PrintService.Log($"Current_DispatcherUnhandledException: {ex.ToString()}", Print.EMode.warning);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = ExceptionHelper.GetReal(e.ExceptionObject as Exception);
            PrintService.Log($"CurrentDomain_UnhandledException: {ex.ToString()}", Print.EMode.warning);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var ex = ExceptionHelper.GetReal(e.Exception);
            PrintService.Log($"TaskScheduler_UnobservedTaskException: {ex.ToString()}", Print.EMode.warning);
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = ExceptionHelper.GetReal(e.Exception);
            PrintService.Log($"Dispatcher_UnhandledException: {ex.ToString()}", Print.EMode.warning);
        }
    }
}
