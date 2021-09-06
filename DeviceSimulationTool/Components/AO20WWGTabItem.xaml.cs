﻿using DeviceSimulationTool.Helpers;
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
    public partial class AO20WWGTabItem : TabItem
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
        }

        public class IDevice
        {
            public IConfig config { get; set; }

            public DeviceListBoxItem item { get; set; }

            public List<string> stdouts { get; set; }
        }
        #endregion

        #region DependencyProperty
        public bool IsSelectedDevice
        {
            get { return (bool)GetValue(IsSelectedDeviceProperty); }
            set { SetValue(IsSelectedDeviceProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedDeviceProperty =
            DependencyProperty.Register("IsSelectedDevice", typeof(bool), typeof(AO20WWGTabItem), new PropertyMetadata(false));

        public bool IsStart
        {
            get { return (bool)GetValue(IsStartProperty); }
            set { SetValue(IsStartProperty, value); }
        }
        public static readonly DependencyProperty IsStartProperty =
            DependencyProperty.Register("IsStart", typeof(bool), typeof(AO20WWGTabItem), new PropertyMetadata(false));

        public bool IsDeviceFull
        {
            get { return (bool)GetValue(IsDeviceFullProperty); }
            set { SetValue(IsDeviceFullProperty, value); }
        }
        public static readonly DependencyProperty IsDeviceFullProperty =
            DependencyProperty.Register("IsDeviceFull", typeof(bool), typeof(AO20WWGTabItem), new PropertyMetadata(false));

        public string Stdout
        {
            get { return (string)GetValue(StdoutProperty); }
            set { SetValue(StdoutProperty, value); }
        }
        public static readonly DependencyProperty StdoutProperty =
            DependencyProperty.Register("Stdout", typeof(string), typeof(AO20WWGTabItem), new PropertyMetadata(""));
        #endregion

        public static string PageName { get; } = "AO-20W WG";
        public static string MessageBoxTitle { get; } = $"{App.AppName}";

        private int DeviceLimit { get; } = 10;

        public ObservableCollection<DeviceListBoxItem> DeviceItems { get; } = new ObservableCollection<DeviceListBoxItem>();
        public ObservableCollection<IDevice> Devices { get; } = new ObservableCollection<IDevice>();

        public AO20WWGTabItem()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = ex.Message;

                App.PrintService.Log($"{AO20WWGTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, AO20WWGTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);

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

                App.PrintService.Log($"{AO20WWGTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, AO20WWGTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
                }
                else
                {
                    IDevice device = this.Devices[index];

                    this.ConfigName.Text = device.config.name;
                    this.ConfigPort.Text = device.config.port.ToString();

                    this.IsStart = device.item.DeviceIsStart;
                    this.Stdout = string.Join("\r\n", device.stdouts);
                    this.IsSelectedDevice = true;
                }
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{AO20WWGTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, AO20WWGTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
                    port = Convert.ToInt32(this.ConfigPort.Text)
                };
                try
                {
                    Validate.TryValidateObject(data);
                    App.PrintService.Log($"{AO20WWGTabItem.PageName}", Print.EMode.success);
                }
                catch (Exception ex)
                {
                    string message = Community.ConvertDataValidateExceptionToMessage(ex);

                    App.PrintService.Log($"{AO20WWGTabItem.PageName}, {message.Replace("\n", " ")}", Print.EMode.error);

                    MessageBox.Show($"{message}", AO20WWGTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }

                int index = this.DeviceList.SelectedIndex;
                IDevice device = this.Devices[index];
                device.config = data;
                device.item.DeviceName = data.name;
                device.item.DeviceIsStart = true;

                this.IsStart = true;
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{AO20WWGTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, AO20WWGTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopService(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = this.DeviceList.SelectedIndex;
                IDevice device = this.Devices[index];
                device.item.DeviceIsStart = false;

                this.IsStart = false;
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{AO20WWGTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, AO20WWGTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
                    port = 12345
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

                App.PrintService.Log($"{AO20WWGTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, AO20WWGTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveDevice(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = this.DeviceList.SelectedIndex;
                this.DeviceItems.RemoveAt(index);
                this.Devices.RemoveAt(index);
                this.DeviceList.SelectedIndex = -1;

                for (int i = 0; i < this.Devices.Count(); i++)
                {
                    IDevice device = this.Devices[i];
                    device.item.DeviceIndex = i + 1;
                }

                this.IsDeviceFull = false;
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                string message = $"{ex.Message}";

                App.PrintService.Log($"{AO20WWGTabItem.PageName}, {message}", Print.EMode.error);

                MessageBox.Show(message, AO20WWGTabItem.MessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}