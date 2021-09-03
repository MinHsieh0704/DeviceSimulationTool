using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeviceSimulationTool.Components
{
    /// <summary>
    /// DeviceListBoxItem.xaml 的互動邏輯
    /// </summary>
    public partial class DeviceListBoxItem : UserControl
    {
        #region DependencyProperty
        public int DeviceIndex
        {
            get { return (int)GetValue(DeviceIndexProperty); }
            set { SetValue(DeviceIndexProperty, value); }
        }
        public static readonly DependencyProperty DeviceIndexProperty =
            DependencyProperty.Register("DeviceIndex", typeof(int), typeof(DeviceListBoxItem), new PropertyMetadata(0));

        public string DeviceName
        {
            get { return (string)GetValue(DeviceNameProperty); }
            set { SetValue(DeviceNameProperty, value); }
        }
        public static readonly DependencyProperty DeviceNameProperty =
            DependencyProperty.Register("DeviceName", typeof(string), typeof(DeviceListBoxItem), new PropertyMetadata(""));

        public bool DeviceIsStart
        {
            get { return (bool)GetValue(DeviceIsStartProperty); }
            set { SetValue(DeviceIsStartProperty, value); }
        }
        public static readonly DependencyProperty DeviceIsStartProperty =
            DependencyProperty.Register("DeviceIsStart", typeof(bool), typeof(DeviceListBoxItem), new PropertyMetadata(false));

        public int DeviceSerialNumber
        {
            get { return (int)GetValue(DeviceSerialNumberProperty); }
            set { SetValue(DeviceSerialNumberProperty, value); }
        }
        public static readonly DependencyProperty DeviceSerialNumberProperty =
            DependencyProperty.Register("DeviceSerialNumber", typeof(int), typeof(DeviceListBoxItem), new PropertyMetadata(0));
        #endregion

        public DeviceListBoxItem()
        {
            InitializeComponent();
        }
    }
}
