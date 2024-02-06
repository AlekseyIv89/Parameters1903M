using Parameters1903M.Model;
using Parameters1903M.ViewModel.TSE1903M;
using System.Windows;

namespace Parameters1903M.View.TSE1903M
{
    /// <summary>
    /// Логика взаимодействия для Prov5_Window.xaml
    /// </summary>
    public partial class Prov5_Window : Window
    {
        public Prov5_Window(Parameter parameter)
        {
            DataContext = new Prov5_ViewModel(parameter);
            InitializeComponent();
        }
    }
}
