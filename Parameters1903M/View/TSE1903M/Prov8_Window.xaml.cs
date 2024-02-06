using Parameters1903M.Model;
using Parameters1903M.ViewModel.TSE1903M;
using System.Windows;

namespace Parameters1903M.View.TSE1903M
{
    public partial class Prov8_Window : Window
    {
        public Prov8_Window(Parameter parameter)
        {
            InitializeComponent();
            DataContext = new Prov8_ViewModel(parameter);
        }
    }
}
