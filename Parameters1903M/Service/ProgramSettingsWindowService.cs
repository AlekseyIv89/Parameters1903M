using Parameters1903M.View;
using System.Linq;
using System.Windows;

namespace Parameters1903M.Service
{
    internal class ProgramSettingsWindowService : IWindowService
    {
        public ProgramSettingsWindow GetWindow() => Application.Current.Windows.OfType<ProgramSettingsWindow>().FirstOrDefault();

        public void Close(object param)
        {
            ProgramSettingsWindow window = Application.Current.Windows.OfType<ProgramSettingsWindow>().FirstOrDefault();
            if (window != null)
            {
                window.Close();
            }
        }

        public void OpenDialog(object param)
        {
            ProgramSettingsWindow window = Application.Current.Windows.OfType<ProgramSettingsWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new ProgramSettingsWindow() { Owner = Application.Current.MainWindow };
            }
            window.ShowDialog();
        }
    }
}
