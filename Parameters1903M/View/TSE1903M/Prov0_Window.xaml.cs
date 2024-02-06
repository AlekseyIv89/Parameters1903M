using Parameters1903M.Model;
using Parameters1903M.ViewModel.TSE1903M;
using System.Windows;

namespace Parameters1903M.View.TSE1903M
{
    public partial class Prov0_Window : Window
    {
        public Prov0_Window(Parameter parameter)
        {
            DataContext = new Prov0_ViewModel(parameter);
            InitializeComponent();
        }
    }
}
