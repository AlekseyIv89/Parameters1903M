using Parameters1903M.Util;
using Parameters1903M.Util.Multimeter;
using Parameters1903M.View;
using Parameters1903M.ViewModel;
using System.Linq;
using System.Windows;

namespace Parameters1903M.Service
{
    internal class AboutWindowService : IWindowService
    {
        public void Close(object param)
        {

        }

        public void OpenDialog(object param)
        {
            AboutWindow window = Application.Current.Windows.OfType<AboutWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new AboutWindow() { Owner = Application.Current.MainWindow };
            }
            window.ShowDialog();
        }

        public string GetCommunicationInterfaceInfo()
        {
            CommunicationInterface communicationInterface = ((MainViewModel)Application.Current.MainWindow.DataContext).CommunicationInterface;
            return EnumInfo.GetDescription(communicationInterface);
        }
    }
}
