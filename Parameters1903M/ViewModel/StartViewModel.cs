using Parameters1903M.Service;
using Parameters1903M.Service.Command;
using Parameters1903M.Util;
using System.Windows.Input;

namespace Parameters1903M.ViewModel
{
    internal class StartViewModel : BaseViewModel
    {
        public string Title => ProgramInfo.SoftwareNameWithVersion;

        private readonly StartWindowService startWindowService;
        public ICommand CloseStartWindowCommand { get; }
        public ICommand ButtonsStartWindowCommand { get; }

        private readonly MainWindowService mainWindowService;
        public ICommand SettingsWindowOpenCommand { get; }
        public ICommand AboutProgramWindowOpenCommand { get; }

        public StartViewModel()
        {
            startWindowService = new StartWindowService();

            CloseStartWindowCommand = new RelayCommand(param => startWindowService.Close(param), x => true);
            ButtonsStartWindowCommand = new RelayCommand(param => startWindowService.ButtonClick(param), x => true);

            mainWindowService = new MainWindowService();
            SettingsWindowOpenCommand = new RelayCommand(param => mainWindowService.GetMainWindowViewModel().ChangeSettingsCommand.Execute(param), x => true);
            AboutProgramWindowOpenCommand = new RelayCommand(param => mainWindowService.GetMainWindowViewModel().AboutWindowShowDialogCommand.Execute(param), x => true);
        }
    }
}
