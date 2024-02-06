using Parameters1903M.Model;
using Parameters1903M.ViewModel;
using System.Windows;

namespace Parameters1903M.View
{
    public partial class DeviceDataInputWindow : Window
    {
        public DeviceDataInputWindow(ProverkaType proverkaType)
        {
            InitializeComponent();
            DataContext = new DeviceDataInputViewModel(proverkaType);
        }
    }
}
