using Parameters1903M.Model;
using Parameters1903M.View;
using System;
using System.Linq;
using System.Windows;

namespace Parameters1903M.Service
{
    internal class DeviceDataInputWindowService : IWindowService
    {
        public DeviceDataInputWindow GetProvWindow() => Application.Current.Windows.OfType<DeviceDataInputWindow>().FirstOrDefault();

        public void Close(object param)
        {
            Window window = Application.Current.Windows.OfType<DeviceDataInputWindow>().FirstOrDefault();
            if (window != null)
            {
                new StartWindowService().Close(param);
                window.Close();
            }
        }

        public void OpenDialog(object param)
        {
            ProverkaType proverkaType = (ProverkaType)Enum.Parse(typeof(ProverkaType), param.ToString());
            DeviceDataInputWindow window = new DeviceDataInputWindow(proverkaType) { Owner = Application.Current.MainWindow };
            window.ShowDialog();
        }
    }
}
