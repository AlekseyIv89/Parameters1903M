using Parameters1903M.Model;
using Parameters1903M.ViewModel.TSE1903M;
using System.Windows;

namespace Parameters1903M.View.TSE1903M
{
    public partial class Prov6_Window : Window
    {
        public Prov6_Window(Parameter parameter)
        {
            InitializeComponent();
            DataContext = new Prov6_ViewModel(parameter);
        }
    }
}
