using System;

namespace Parameters1903M.Model
{
    public class DeviceData : BaseModel
    {
        private string deviceType;
        private string deviceNum;
        private string workspaceNum;
        private string prismNum;
        private string document;
        private string grafaTable;
        private string date;
        private string startProvDate;
        private string provAfter;
        private double rizm;
        private string rizmStr;

        public string DeviceType
        {
            get => deviceType;
            set 
            {
                deviceType = value;
                OnPropertyChanged();
            }
        }

        public string DeviceNum
        {
            get => deviceNum;
            set
            {
                deviceNum = value;
                OnPropertyChanged();
            }
        }

        public string WorkspaceNum
        {
            get => workspaceNum;
            set
            {
                workspaceNum = value;
                OnPropertyChanged();
            }
        }

        public string PrismNum
        {
            get => prismNum;
            set
            {
                prismNum = value;
                OnPropertyChanged();
            }
        }

        public string Document
        {
            get => document;
            set
            {
                document = value;
                OnPropertyChanged();
            }
        }

        public string GrafaTable
        {
            get => grafaTable;
            set
            {
                grafaTable = value;
                OnPropertyChanged();
            }
        }

        public string Date
        {
            get => date;
            set
            {
                date = value;
                OnPropertyChanged();
            }
        }

        public string StartProvDate
        {
            get => startProvDate;
            set
            {
                startProvDate = value;
                OnPropertyChanged();
            }
        }

        public string ProvAfter
        {
            get => provAfter;
            set
            {
                provAfter = value;
                OnPropertyChanged();
            }
        }

        public string Conclusion { get; set; }

        public double Rizm
        {
            get => rizm;
            set
            {
                rizm = value;

                int digits = 5;
                RizmStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }
        public string RizmStr
        {
            get => rizmStr;
            private set
            {
                rizmStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            DeviceType = default;
            DeviceNum = default;
            WorkspaceNum = default;
            PrismNum = default;
            Document = default;
            GrafaTable = default;
            Date = default;
            StartProvDate = default;
            ProvAfter = default;
            Conclusion = default;
            Rizm = default;
            RizmStr = default;
        }

    }
}
