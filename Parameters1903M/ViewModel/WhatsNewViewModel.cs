using Parameters1903M.Util;
using System;

namespace Parameters1903M.ViewModel
{
    internal class WhatsNewViewModel : BaseViewModel
    {
        public string Title => "Что нового";

        public string Info
        {
            get
            {
                string info = "";

                for (int i = 0; i < ProgramInfo.AllVersionsInfo.Count; i++)
                {
                    info += $"Версия {ProgramInfo.AllVersionsInfo[i].Version.Substring(2)} от {ProgramInfo.AllVersionsInfo[i].CompiledDate:dd.MM.yyyy}" + Environment.NewLine;

                    info += ProgramInfo.AllVersionsInfo[i].Changes;
                    if (i < ProgramInfo.AllVersionsInfo.Count - 1)
                    {
                        info += Environment.NewLine;
                    }
                }
                return info;
            }
        }


    }
}
