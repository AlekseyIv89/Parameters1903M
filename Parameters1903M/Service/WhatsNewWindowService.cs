using Parameters1903M.View;
using System.Linq;
using System.Windows;

namespace Parameters1903M.Service
{
    internal class WhatsNewWindowService : IWindowService
    {
        public void Open()
        {
            WhatsNewWindow window = Application.Current.Windows.OfType<WhatsNewWindow>().FirstOrDefault();
            if (window == null)
            {
                window = new WhatsNewWindow();
            }
            if (!window.IsVisible)
            {
                window.Show();
                return;
            }
            if (!window.IsActive)
            {
                window.Activate();
            }
        }

        public void OpenDialog(object param)
        {

        }

        public void Close(object param)
        {

        }
    }
}
