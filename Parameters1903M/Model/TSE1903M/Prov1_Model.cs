using Newtonsoft.Json;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;
using System.Collections.Generic;
using log4net;
using System.Threading.Tasks;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov1_Model : BaseModel, IProvModel
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Масштабный коэффициент
        private readonly Parameter scaleFactor;

        public List<ScaleFactorInitialData> InitialData { get; set; }
        public ScaleFactorCalculatedData CalculatedData { get; set; }

        public Prov1_Model(Parameter scaleFactor)
        {
            log4net.Config.XmlConfigurator.Configure();

            this.scaleFactor = scaleFactor;
            ReadData();
        }

        public void ClearAllData()
        {
            for (int i = 0; i < InitialData.Count; i++)
            {
                InitialData[i].Clear();
            }

            CalculatedData.Clear();
        }

        public void CalculateData()
        {
            if (!string.IsNullOrWhiteSpace(InitialData[InitialData.Count - 1].ScaleFactorValue1Str))
            {
                CalculatedData.Ioc1Value = Ios1(InitialData);
            }

            if (!string.IsNullOrWhiteSpace(InitialData[InitialData.Count - 1].ScaleFactorValue2Str))
            {
                CalculatedData.Ioc2Value = Ios2(InitialData);
                CalculatedData.IgValue = Ig(CalculatedData);

                scaleFactor.Value = CalculatedData.IgValue;

                log.Debug("Полученные в результате расчета данные:");
                log.Debug($"    I1 [мА] = {CalculatedData.Ioc1Value}");
                log.Debug($"    I2 [мА] = {CalculatedData.Ioc2Value}");
                log.Debug($"    Ig [мА] = {CalculatedData.IgValue}");

                WriteData();
            }
        }

        private double Ios1(List<ScaleFactorInitialData> initialData)
        {
            double sum = 0;
            for (int i = 1; i < initialData.Count; i++)
            {
                sum += initialData[i].ScaleFactorValue1;
            }
            return sum / 4; // ListScaleFactorInitialData.Count - 1 = 4 
        }

        private double Ios2(List<ScaleFactorInitialData> initialData)
        {
            double sum = 0;
            for (int i = 1; i < initialData.Count; i++)
            {
                sum += initialData[i].ScaleFactorValue2;
            }
            return sum / 4; // ListScaleFactorInitialData.Count - 1 = 4 
        }

        private double Ig(ScaleFactorCalculatedData calculatedData)
        {
            return (Math.Abs(calculatedData.Ioc1Value) + Math.Abs(calculatedData.Ioc2Value)) / (2.0 * Math.Sin(4 * Math.PI / 180));
        }

        private async void ReadData()
        {
            InitialData = new List<ScaleFactorInitialData>
            {
                new ScaleFactorInitialData(),
                new ScaleFactorInitialData(),
                new ScaleFactorInitialData(),
                new ScaleFactorInitialData(),
                new ScaleFactorInitialData()
            };
            CalculatedData = new ScaleFactorCalculatedData();

            if (!string.IsNullOrWhiteSpace(scaleFactor.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{scaleFactor.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                List<ScaleFactorInitialData> listInitData = JsonConvert.DeserializeObject<List<ScaleFactorInitialData>>(fileData[0]);
                ScaleFactorCalculatedData calcData = JsonConvert.DeserializeObject<ScaleFactorCalculatedData>(fileData[1]);

                for (int i = 0; i < listInitData.Count; i++)
                {
                    InitialData[i].ScaleFactorValue1 = listInitData[i].ScaleFactorValue1;
                    InitialData[i].ScaleFactorValue2 = listInitData[i].ScaleFactorValue2;
                }

                CalculatedData.IgValue = calcData.IgValue;
                CalculatedData.Ioc1Value = calcData.Ioc1Value;
                CalculatedData.Ioc2Value = calcData.Ioc2Value;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{scaleFactor.Num}", false);
        }

        public async Task<ScaleFactorCalculatedData> GetScaleFactorCalculatedData()
        {
            CalculatedData = new ScaleFactorCalculatedData();
            if (!string.IsNullOrWhiteSpace(scaleFactor.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{scaleFactor.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                ScaleFactorCalculatedData calcData = JsonConvert.DeserializeObject<ScaleFactorCalculatedData>(fileData[1]);

                CalculatedData.IgValue = calcData.IgValue;
                CalculatedData.Ioc1Value = calcData.Ioc1Value;
                CalculatedData.Ioc2Value = calcData.Ioc2Value;
            }

            return CalculatedData;
        }
    }

    internal class ScaleFactorInitialData : BaseModel, IProvData
    {
        private readonly int digits = 3;

        private double scaleFactorValue1;
        private string scaleFactorValue1Str;

        private double scaleFactorValue2;
        private string scaleFactorValue2Str;

        public double ScaleFactorValue1
        {
            get => scaleFactorValue1;
            set
            {
                scaleFactorValue1 = value;
                ScaleFactorValue1Str = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string ScaleFactorValue1Str
        {
            get => scaleFactorValue1Str;
            private set
            {
                scaleFactorValue1Str = value;
                OnPropertyChanged();
            }
        }

        public double ScaleFactorValue2
        {
            get => scaleFactorValue2;
            set
            {
                scaleFactorValue2 = value;
                ScaleFactorValue2Str = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string ScaleFactorValue2Str
        {
            get => scaleFactorValue2Str;
            private set
            {
                scaleFactorValue2Str = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            ScaleFactorValue1 = default;
            ScaleFactorValue1Str = default;
            ScaleFactorValue2 = default;
            ScaleFactorValue2Str = default;
        }
    }


    internal class ScaleFactorCalculatedData : BaseModel, IProvData
    {
        private double ioc1Value;
        private string ioc1ValueStr;

        private double ioc2Value;
        private string ioc2ValueStr;

        private double igValue;
        private string igValueStr;

        public double Ioc1Value
        {
            get => ioc1Value;
            set
            {
                ioc1Value = value;
                int digits = 3;
                Ioc1ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Ioc1ValueStr
        {
            get => ioc1ValueStr;
            private set
            {
                ioc1ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double Ioc2Value
        {
            get => ioc2Value;
            set
            {
                ioc2Value = value;
                int digits = 3;
                Ioc2ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Ioc2ValueStr
        {
            get => ioc2ValueStr;
            private set
            {
                ioc2ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double IgValue
        {
            get => igValue;
            set
            {
                igValue = value;
                int digits = 1;
                IgValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string IgValueStr
        {
            get => igValueStr;
            private set
            {
                igValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            Ioc1Value = default;
            Ioc1ValueStr = default;
            Ioc2Value = default;
            Ioc2ValueStr = default;
            IgValue = default;
            IgValueStr = default;
        }
    }
}
