using Parameters1903M.Util;
using Parameters1903M.View;
using System.Linq;
using System.Windows;

namespace Parameters1903M.Service
{
    internal class StartWindowService : IWindowService
    {
        public void OpenDialog(object param)
        {
            if (GlobalVars.StartWindowClosed)
            {
                // Запрет повторного создания стартового окна
                return;
            }

            Window window = Application.Current.Windows.OfType<StartWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new StartWindow() { Owner = Application.Current.MainWindow };
            }
            window.ShowDialog();
        }

        public void Close(object param)
        {
            string prm = param != null
                ? param.ToString()
                : "";

            if (prm.Equals("StartWindowClose"))
            {
                Window window = Application.Current.Windows.OfType<StartWindow>().FirstOrDefault();
                if (window != null)
                {
                    GlobalVars.StartWindowClosed = true;
                    window.Close();
                }
            }
            else if (string.IsNullOrEmpty(prm) && !GlobalVars.StartWindowClosed)
            {
                Application.Current.MainWindow.Close();
            }
        }

        private void Hide()
        {
            Window window = Application.Current.Windows.OfType<StartWindow>().FirstOrDefault();
            if (window != null)
            {
                window.Hide();
            }
        }

        public void ButtonClick(object param)
        {
            Hide();
            DeviceDataInputWindowService deviceDataInputWindowService = new DeviceDataInputWindowService();
            deviceDataInputWindowService.OpenDialog(param);
        }
    }
}
