using Parameters1903M.Model;
using Parameters1903M.ViewModel.TSE1903M;
using System.Windows;

namespace Parameters1903M.View.TSE1903M
{
    public partial class Prov10_Window : Window
    {
        public Prov10_Window(Parameter parameter)
        {
            DataContext = new Prov10_ViewModel(parameter);
            InitializeComponent();
        }
    }
}
