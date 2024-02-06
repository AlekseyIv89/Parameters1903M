using Parameters1903M.ViewModel;
using System;
using System.Windows;

namespace Parameters1903M.View
{
    public partial class TimerWindow : Window
    {
        public TimerWindow(TimeSpan timeSpan)
        {
            DataContext = new TimerViewModel(timeSpan);
            InitializeComponent();
        }
    }
}
