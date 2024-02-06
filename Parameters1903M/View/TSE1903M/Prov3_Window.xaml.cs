using Parameters1903M.Model;
using Parameters1903M.ViewModel.TSE1903M;
using System.Windows;

namespace Parameters1903M.View.TSE1903M
{
    public partial class Prov3_Window : Window
    {
        public Prov3_Window(Parameter parameter)
        {
            InitializeComponent();
            DataContext = new Prov3_ViewModel(parameter);
        }
    }
}
