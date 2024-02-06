using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Parameters1903M.ViewModel
{
    internal class BaseViewModel : INotifyPropertyChanged
    {
        private string buttonContent;
        public string ButtonContent
        {
            get => buttonContent;
            set
            {
                buttonContent = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
