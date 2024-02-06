using Parameters1903M.Model;
using Parameters1903M.ViewModel.TSE1903M;
using System.Windows;

namespace Parameters1903M.View.TSE1903M
{
    public partial class Prov18_Window : Window
    {
        public Prov18_Window(Parameter parameter)
        {
            DataContext = new Prov18_ViewModel(parameter);
            InitializeComponent();
        }
    }
}
