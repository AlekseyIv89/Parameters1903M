using Parameters1903M.Model;
using Parameters1903M.View.TSE1903M;
using System.Linq;
using System.Windows;

namespace Parameters1903M.Service.TSE1903M
{
    internal class InputDialogWindowService : IWindowService
    {
        public InputDialogWindow GetProvWindow() => Application.Current.Windows.OfType<InputDialogWindow>().FirstOrDefault();

        public void Hide()
        {
            InputDialogWindow window = Application.Current.Windows.OfType<InputDialogWindow>().FirstOrDefault();
            if (window != null)
            {
                window.Hide();
            }
        }

        public void Close(object param)
        {
            InputDialogWindow window = Application.Current.Windows.OfType<InputDialogWindow>().FirstOrDefault();
            if (window != null)
            {
                window.Close();
            }
        }

        public void OpenDialog(object param)
        {
            InputDialogWindow window = Application.Current.Windows.OfType<InputDialogWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new InputDialogWindow((Parameter)param) { Owner = Application.Current.MainWindow };
            }
            window.ShowDialog();
        }

        public void OpenDialog(object param, string param1, string param2)
        {
            InputDialogWindow window = Application.Current.Windows.OfType<InputDialogWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new InputDialogWindow((Parameter)param, param1, param2) { Owner = Application.Current.MainWindow };
            }
            window.ShowDialog();
        }
    }
}
