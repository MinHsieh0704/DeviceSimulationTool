using System.Windows;
using System.Windows.Controls;

namespace DeviceSimulationTool.Components
{
    /// <summary>
    /// CircleButton.xaml 的互動邏輯
    /// </summary>
    public partial class CircleButton : Button
    {
        #region DependencyProperty
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CircleButton), new PropertyMetadata(new CornerRadius(30)));
        #endregion

        public CircleButton()
        {
            InitializeComponent();
        }
    }
}
