using Min_Helpers;
using Min_Helpers.PrintHelper;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Windows;

namespace DeviceSimulationTool
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DependencyProperty
        public bool IsLoadingVisable
        {
            get { return (bool)GetValue(IsLoadingVisableProperty); }
            set { SetValue(IsLoadingVisableProperty, value); }
        }
        public static readonly DependencyProperty IsLoadingVisableProperty =
            DependencyProperty.Register("IsLoadingVisable", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public string AppVersion
        {
            get { return (string)GetValue(AppVersionProperty); }
            set { SetValue(AppVersionProperty, value); }
        }
        public static readonly DependencyProperty AppVersionProperty =
            DependencyProperty.Register("AppVersion", typeof(string), typeof(MainWindow), new PropertyMetadata(""));
        #endregion

        public static string PageName { get; } = "MainWindow";
        public static string MessageBoxTitle { get; } = $"{App.AppName}";

        public static Subject<bool> LoadingPage { get; set; } = new Subject<bool>();

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                Assembly assembly = Assembly.GetExecutingAssembly();
                this.AppVersion = $"v{FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion}";

                MainWindow.LoadingPage
                    .DistinctUntilChanged()
                    .Subscribe((x) =>
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.IsLoadingVisable = x;
                        }));
                    });
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = ex.Message;

                App.PrintService.Log($"{MainWindow.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, MainWindow.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                App.Current.Shutdown(1);
            }
        }
    }
}
