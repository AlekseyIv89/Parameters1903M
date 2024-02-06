using Parameters1903M.Model;
using Parameters1903M.ViewModel.TSE1903M;
using System.Windows;

namespace Parameters1903M.View.TSE1903M
{
    public partial class Prov1_Window : Window
    {
        public Prov1_Window(Parameter parameter)
        {
            DataContext = new Prov1_ViewModel(parameter);
            InitializeComponent();
        }
    }
}
