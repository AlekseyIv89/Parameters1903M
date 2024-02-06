using Parameters1903M.Service;
using Parameters1903M.Util;

namespace Parameters1903M.ViewModel
{
    internal class AboutViewModel : BaseViewModel
    {
        public string Version => $"{ProgramInfo.SoftwareNameWithVersion} от {ProgramInfo.CompileDate:d}";

        public string Developer => ProgramInfo.SoftwareDeveloper;

        public string WorkMode => new AboutWindowService().GetCommunicationInterfaceInfo();
    }
}
