using DeviceSimulationTool.Helpers;
using Min_Helpers;
using Min_Helpers.AttributeHelper;
using Min_Helpers.PrintHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeviceSimulationTool.Components
{
    /// <summary>
    /// BaseTabItem.xaml 的互動邏輯
    /// </summary>
    public partial class HttpCommandTabItem : TabItem
    {
        #region Interface
        public class IConfig
        {
            [Display(Name = "<Name>")]
            [Required]
            public string name { get; set; }

            [Display(Name = "<Port>")]
            [Required]
            [Port]
            public int port { get; set; }

            [Display(Name = "<Enable Auth>")]
            [Required]
            public bool isEnableAuth { get; set; }

            [Display(Name = "<Account>")]
            [RequiredIf("isEnableAuth", true)]
            public string account { get; set; }

            [Display(Name = "<Password>")]
            [RequiredIf("isEnableAuth", true)]
            public string password { get; set; }
        }

        public class IDevice
        {
            public IConfig config { get; set; }

            public DeviceListBoxItem item { get; set; }

            public List<string> stdouts { get; set; }

            public HttpCommand server { get; set; }
        }
        #endregion

        #region DependencyProperty
        public bool IsSelectedDevice
        {
            get { return (bool)GetValue(IsSelectedDeviceProperty); }
            set { SetValue(IsSelectedDeviceProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedDeviceProperty =
            DependencyProperty.Register("IsSelectedDevice", typeof(bool), typeof(HttpCommandTabItem), new PropertyMetadata(false));

        public bool IsUseAuth
        {
            get { return (bool)GetValue(IsUseAuthProperty); }
            set { SetValue(IsUseAuthProperty, value); }
        }
        public static readonly DependencyProperty IsUseAuthProperty =
            DependencyProperty.Register("IsUseAuth", typeof(bool), typeof(HttpCommandTabItem), new PropertyMetadata(false));

        public bool IsStart
        {
            get { return (bool)GetValue(IsStartProperty); }
            set { SetValue(IsStartProperty, value); }
        }
        public static readonly DependencyProperty IsStartProperty =
            DependencyProperty.Register("IsStart", typeof(bool), typeof(HttpCommandTabItem), new PropertyMetadata(false));

        public bool IsDeviceFull
        {
            get { return (bool)GetValue(IsDeviceFullProperty); }
            set { SetValue(IsDeviceFullProperty, value); }
        }
        public static readonly DependencyProperty IsDeviceFullProperty =
            DependencyProperty.Register("IsDeviceFull", typeof(bool), typeof(HttpCommandTabItem), new PropertyMetadata(false));

        public string Stdout
        {
            get { return (string)GetValue(StdoutProperty); }
            set { SetValue(StdoutProperty, value); }
        }
        public static readonly DependencyProperty StdoutProperty =
            DependencyProperty.Register("Stdout", typeof(string), typeof(HttpCommandTabItem), new PropertyMetadata(""));
        #endregion

        public static string PageName { get; } = "Http Command";
        public static string MessageBoxTitle { get; } = $"{App.AppName}";

        private int DeviceLimit { get; } = 10;

        private int SelectedDeviceSerialNumber = -1;

        public ObservableCollection<DeviceListBoxItem> DeviceItems { get; } = new ObservableCollection<DeviceListBoxItem>();
        public ObservableCollection<IDevice> Devices { get; } = new ObservableCollection<IDevice>();

        public HttpCommandTabItem()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = ex.Message;

                App.PrintService.Log($"{HttpCommandTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, HttpCommandTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                App.Current.Shutdown(1);
            }
        }

        private void ConfigPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{HttpCommandTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, HttpCommandTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigEnableAuth_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                if (checkBox == null)
                {
                    return;
                }

                this.IsUseAuth = true;
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{HttpCommandTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, HttpCommandTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigEnableAuth_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                if (checkBox == null)
                {
                    return;
                }

                this.IsUseAuth = false;
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{HttpCommandTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, HttpCommandTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListBox listBox = sender as ListBox;
                if (listBox == null)
                {
                    return;
                }

                int index = listBox.SelectedIndex;
                if (index == -1)
                {
                    this.IsSelectedDevice = false;
                    this.SelectedDeviceSerialNumber = -1;
                    this.Stdout = "";
                }
                else
                {
                    IDevice device = this.Devices[index];

                    this.SelectedDeviceSerialNumber = device.item.DeviceSerialNumber;

                    this.ConfigName.Text = device.config.name;
                    this.ConfigPort.Text = device.config.port.ToString();
                    this.ConfigEnableAuth.IsChecked = device.config.isEnableAuth;
                    this.ConfigAccount.Text = device.config.account;
                    this.ConfigPassword.Text = device.config.password;

                    this.IsStart = device.item.DeviceIsStart;
                    this.Stdout = string.Join("\r\n", device.stdouts);
                    this.IsSelectedDevice = true;
                }
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{HttpCommandTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, HttpCommandTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartService(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ConfigPort.Text.Length == 0)
                {
                    this.ConfigPort.Text = "0";
                }

                IConfig data = new IConfig()
                {
                    name = this.ConfigName.Text,
                    port = Convert.ToInt32(this.ConfigPort.Text),
                    isEnableAuth = this.ConfigEnableAuth.IsChecked == true,
                    account = this.ConfigAccount.Text,
                    password = this.ConfigPassword.Text
                };
                try
                {
                    Validate.TryValidateObject(data);
                    App.PrintService.Log($"{HttpCommandTabItem.PageName}", Print.EMode.success);
                }
                catch (Exception ex)
                {
                    string message = Community.ConvertDataValidateExceptionToMessage(ex);

                    App.PrintService.Log($"{HttpCommandTabItem.PageName}, {message.Replace("\n", " ")}", Print.EMode.error);

                    MessageBox.Show($"{message}", HttpCommandTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }

                int index = this.DeviceList.SelectedIndex;
                IDevice device = this.Devices[index];
                this.StartServer(device);

                device.config = data;
                device.item.DeviceName = data.name;
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{HttpCommandTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, HttpCommandTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopService(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = this.DeviceList.SelectedIndex;
                IDevice device = this.Devices[index];
                this.StopServer(device);
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{HttpCommandTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, HttpCommandTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateDevice(object sender, RoutedEventArgs e)
        {
            try
            {
                IDevice device = this.Devices.LastOrDefault();
                int index = device == null ? 0 : device.item.DeviceSerialNumber;

                IConfig config = new IConfig() 
                { 
                    name = $"Device {index + 1}", 
                    port = 12345,
                    isEnableAuth = false,
                    account = "",
                    password = ""
                };
                DeviceListBoxItem item = new DeviceListBoxItem() 
                { 
                    DeviceIndex = this.Devices.Count() + 1, 
                    DeviceSerialNumber = index + 1, 
                    DeviceName = config.name, 
                    DeviceIsStart = false 
                };
                device = new IDevice()
                {
                    config = config,
                    item = item,
                    stdouts = new List<string>()
                };
                this.DeviceItems.Add(item);
                this.Devices.Add(device);

                this.DeviceList.SelectedIndex = this.Devices.Count() - 1;

                this.ConfigName.Text = config.name;
                this.ConfigPort.Text = config.port.ToString();

                if (this.Devices.Count() == this.DeviceLimit)
                {
                    this.IsDeviceFull = true;
                }
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{HttpCommandTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, HttpCommandTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveDevice(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = this.DeviceList.SelectedIndex;
                IDevice device = this.Devices[index];
                this.StopServer(device);

                this.DeviceItems.RemoveAt(index);
                this.Devices.RemoveAt(index);

                this.DeviceList.SelectedIndex = -1;

                this.ReflashDevices();

                this.IsDeviceFull = false;
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{HttpCommandTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, HttpCommandTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReflashDevices()
        {
            try
            {
                for (int i = 0; i < this.Devices.Count(); i++)
                {
                    IDevice device = this.Devices[i];
                    device.item.DeviceIndex = i + 1;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void StartServer(IDevice device)
        {
            MainWindow.LoadingPage.OnNext(true);

            try
            {
                HttpCommand server = new HttpCommand();
                server.port = device.config.port;
                if (device.config.isEnableAuth)
                {
                    server.account = device.config.account;
                    server.password = device.config.password;
                }

                device.server = server;

                server.onMessage.Subscribe((x) =>
                {
                    device.stdouts.Add($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ---> {x}");

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (this.SelectedDeviceSerialNumber == device.item.DeviceSerialNumber)
                        {
                            this.Stdout += $"\r\n{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ---> {x}";
                        }
                    }));
                });
                server.onError.Subscribe((x) =>
                {
                    x = ExceptionHelper.GetReal(x);

                    device.stdouts.Add($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ---> {x}");

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (this.SelectedDeviceSerialNumber == device.item.DeviceSerialNumber)
                        {
                            this.Stdout += $"\r\n{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ---> {x}";
                        }

                        this.StopServer(device);
                    }));
                });

                server.Connect();

                device.stdouts.Add($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ---> {device.config.name} is Start");
                if (this.SelectedDeviceSerialNumber == device.item.DeviceSerialNumber)
                {
                    if (this.Stdout != "")
                    {
                        this.Stdout += $"\r\n";
                    }
                    this.Stdout += $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ---> {device.config.name} is Start";
                }

                device.item.DeviceIsStart = true;
                if (this.SelectedDeviceSerialNumber == device.item.DeviceSerialNumber)
                {
                    this.IsStart = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                MainWindow.LoadingPage.OnNext(false);
            }
        }

        private void StopServer(IDevice device)
        {
            MainWindow.LoadingPage.OnNext(true);

            try
            {
                if (!device.item.DeviceIsStart)
                {
                    return;
                }

                device.server.Dispose();
                device.server = null;

                device.stdouts.Add($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ---> {device.config.name} was Stop");
                device.stdouts.Add($"");
                if (this.SelectedDeviceSerialNumber == device.item.DeviceSerialNumber)
                {
                    if (this.Stdout != "")
                    {
                        this.Stdout += $"\r\n";
                    }
                    this.Stdout += $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ---> {device.config.name} was Stop";
                    this.Stdout += $"\r\n";
                }

                device.item.DeviceIsStart = false;
                if (this.SelectedDeviceSerialNumber == device.item.DeviceSerialNumber)
                {
                    this.IsStart = false;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                MainWindow.LoadingPage.OnNext(false);
            }
        }
    }
}
