using Parameters1903M.Model;
using Parameters1903M.ViewModel.TSE1903M;
using System.Windows;

namespace Parameters1903M.View.TSE1903M
{
    /// <summary>
    /// Логика взаимодействия для Prov9_Window.xaml
    /// </summary>
    public partial class Prov9_Window : Window
    {
        public Prov9_Window(Parameter parameter)
        {
            DataContext = new Prov9_ViewModel(parameter);
            InitializeComponent();
        }
    }
}
