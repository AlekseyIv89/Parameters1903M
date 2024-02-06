using Parameters1903M.Model;
using Parameters1903M.ViewModel.TSE1903M;
using System.Windows;

namespace Parameters1903M.View.TSE1903M
{
    public partial class InputDialogWindow : Window
    {
        public InputDialogWindow(Parameter parameter)
        {
            InitializeComponent();
            DataContext = new InputDialogViewModel(parameter);
        }

        public InputDialogWindow(Parameter parameter, string param1, string param2)
        {
            InitializeComponent();
            DataContext = new InputDialogViewModel(parameter, param1, param2);
        }
    }
}
