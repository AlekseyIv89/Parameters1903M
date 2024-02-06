using Parameters1903M.Util;
using System;
using System.Collections.Generic;

namespace Parameters1903M.Model
{
    internal class ReportData : BaseModel
    {
        private string repConclusion;
        private string repConclusion2;
        private string repAssignment;
        private string repSecondName;
        private string repDate;

        public string RepConclusion
        {
            get => repConclusion;
            set
            {
                repConclusion = value;
                OnPropertyChanged();
            }
        }

        public string RepConclusion2
        {
            get => repConclusion2;
            set
            {
                repConclusion2 = value;
                OnPropertyChanged();
            }
        }

        public string RepAssignment
        {
            get => repAssignment;
            set
            {
                repAssignment = value;
                OnPropertyChanged();
            }
        }

        public string RepSecondName
        {
            get => repSecondName;
            set
            {
                repSecondName = value;
                OnPropertyChanged();
            }
        }

        public string RepDate
        {
            get => repDate;
            set
            {
                repDate = value;
                OnPropertyChanged();
            }
        }

        public List<Parameter> RepParameters { get; set; }

        public List<TableDeviceData> LeftTableDeviceData { get; set; }
        public List<TableDeviceData> RightTableDeviceData { get; set; }

        public string RepProgramVersion { get; set; }

        public ReportData()
        {
            RepParameters = new List<Parameter>();
            LeftTableDeviceData = new List<TableDeviceData>();
            RightTableDeviceData = new List<TableDeviceData>();

            RepProgramVersion = ProgramInfo.SoftwareNameWithVersionAndDate;
            RepDate = DateTime.Now.ToString("dd.MM.yyyy");
        }
    }
}
