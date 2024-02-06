using Parameters1903M.Service;
using Parameters1903M.Service.Command;
using Parameters1903M.Util.Multimeter;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows.Input;

namespace Parameters1903M.ViewModel
{
    internal class ProgramSettingsViewModel : BaseViewModel
    {
        public string Title => "Настройки";

        public List<string> ComPorts { get; }
        public List<string> Voltmeters { get; }

        private readonly ProgramSettingsWindowService programSettingsWindowService;
        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand SavePathFolderDialogCommand { get; }

        public ProgramSettingsViewModel()
        {
            ComPorts = SerialPort.GetPortNames().ToList();
            Voltmeters = Enum.GetValues(typeof(CommunicationInterface))
                .Cast<CommunicationInterface>()
                .Select(_enum => _enum.ToString())
                .ToList();

            programSettingsWindowService = new ProgramSettingsWindowService();
            SaveCommand = new RelayCommand(param => SaveProperties(), x => true);
            CloseCommand = new RelayCommand(param => ClosePropertiesWithoutSaving(), x => true);
            SavePathFolderDialogCommand = new RelayCommand(param => SavePathFolderDialogOpen(), x => true);
        }

        private void SaveProperties()
        {
            Properties.Settings.Default.Save();
            programSettingsWindowService.Close(true);
        }

        private void ClosePropertiesWithoutSaving()
        {
            Properties.Settings.Default.Reload();
            programSettingsWindowService.Close(true);
        }

        private void SavePathFolderDialogOpen()
        {
            
        }
    }
}
