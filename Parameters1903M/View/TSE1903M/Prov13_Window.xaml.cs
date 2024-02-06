using Parameters1903M.Model;
using Parameters1903M.ViewModel.TSE1903M;
using System.Windows;

namespace Parameters1903M.View.TSE1903M
{
    public partial class Prov13_Window : Window
    {
        public Prov13_Window(Parameter parameter)
        {
            DataContext = new Prov13_ViewModel(parameter);
            InitializeComponent();
        }
    }
}
