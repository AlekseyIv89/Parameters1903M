using Newtonsoft.Json;
using Parameters1903M.Service;
using Parameters1903M.Util;
using Parameters1903M.Util.Data;
using System;
using System.Collections.Generic;

namespace Parameters1903M.Model.TSE1903M
{
    internal class Prov10_Model : BaseModel, IProvModel
    {
        // Непараллельность ОЧ базовой плоскости
        private readonly Parameter sensitivityAxisNonparallelism;

        // Масштабный коэффициент
        private ScaleFactorCalculatedData scaleFactorCalculatedData;

        public List<SensitivityAxisNonparallelismInitialData> InitialData { get; set; }
        public SensitivityAxisNonparallelismCalculatedData CalculatedData { get; set; }

        public Prov10_Model(Parameter sensitivityAxisNonparallelism)
        {
            this.sensitivityAxisNonparallelism = sensitivityAxisNonparallelism;
            GetAngleSensorSlopeCalculatedData();
            ReadData();
        }

        private async void GetAngleSensorSlopeCalculatedData()
        {
            scaleFactorCalculatedData = await new Prov1_Model(new MainWindowService().GetParameterByName("Масштабный коэффициент, Ig")).GetScaleFactorCalculatedData();
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
            double iSum = 0.0;

            if (!string.IsNullOrWhiteSpace(InitialData[InitialData.Count - 1].I0ValueStr))
            {
                for (int i = 0; i < InitialData.Count; i++)
                {
                    iSum += InitialData[i].I0Value;
                }

                CalculatedData.I1Value = iSum / InitialData.Count;
                CalculatedData.Fi0Value = CalculatedData.I1Value / (5E-6 * (scaleFactorCalculatedData.IgValue * 1E3));
            }

            if (!string.IsNullOrWhiteSpace(InitialData[InitialData.Count - 1].I0ValueStr) 
                && !string.IsNullOrWhiteSpace(InitialData[InitialData.Count - 1].I180ValueStr))
            {
                iSum = 0.0;
                for (int i = 0; i < InitialData.Count; i++)
                {
                    iSum += InitialData[i].I180Value;
                }

                CalculatedData.I2Value = iSum / InitialData.Count;
                CalculatedData.Fi180Value = CalculatedData.I2Value / (5E-6 * (scaleFactorCalculatedData.IgValue * 1E3));

                sensitivityAxisNonparallelism.StrValue = $"{CalculatedData.Fi0Value:F0};{CalculatedData.Fi180Value:F0}";

                WriteData();
            }                
        }

        private async void ReadData()
        {
            InitialData = new List<SensitivityAxisNonparallelismInitialData>
            {
                new SensitivityAxisNonparallelismInitialData(),
                new SensitivityAxisNonparallelismInitialData(),
                new SensitivityAxisNonparallelismInitialData(),
                new SensitivityAxisNonparallelismInitialData(),
                new SensitivityAxisNonparallelismInitialData()
            };

            CalculatedData = new SensitivityAxisNonparallelismCalculatedData();

            if (!string.IsNullOrWhiteSpace(sensitivityAxisNonparallelism.StrValue))
            {
                string filePath = Data.CheckFullFileName(GlobalVars.SavePath, $@"{GlobalVars.DeviceNum}_{sensitivityAxisNonparallelism.Num}");

                string data = await Data.Read(filePath);

                string[] fileData = data.Split('\n');
                List<SensitivityAxisNonparallelismInitialData> listInitData = JsonConvert.DeserializeObject<List<SensitivityAxisNonparallelismInitialData>>(fileData[0]);
                SensitivityAxisNonparallelismCalculatedData listCalcData = JsonConvert.DeserializeObject<SensitivityAxisNonparallelismCalculatedData>(fileData[1]);

                for (int i = 0; i < listInitData.Count; i++)
                {
                    InitialData[i].I0Value = listInitData[i].I0Value;
                    InitialData[i].I180Value = listInitData[i].I180Value;
                }

                CalculatedData.I1Value = listCalcData.I1Value;
                CalculatedData.I2Value = listCalcData.I2Value;
                CalculatedData.Fi0Value = listCalcData.Fi0Value;
                CalculatedData.Fi180Value = listCalcData.Fi180Value;
            }
        }

        private void WriteData()
        {
            Data.Save(string.Join("\n", JsonConvert.SerializeObject(InitialData), JsonConvert.SerializeObject(CalculatedData))
                , $@"{GlobalVars.DeviceNum}_{sensitivityAxisNonparallelism.Num}", false);
        }
    }

    internal class SensitivityAxisNonparallelismInitialData : BaseModel, IProvData
    {
        private readonly int digits = 3;

        private double i0Value;
        private string i0ValueStr;

        private double i180Value;
        private string i180ValueStr;

        public double I0Value
        {
            get => i0Value;
            set
            {
                i0Value = value;
                I0ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I0ValueStr
        {
            get => i0ValueStr;
            private set
            {
                i0ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double I180Value
        {
            get => i180Value;
            set
            {
                i180Value = value;
                I180ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I180ValueStr
        {
            get => i180ValueStr;
            private set
            {
                i180ValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            I0Value = default;
            I0ValueStr = default;
            I180Value = default;
            I180ValueStr = default;
        }
    }

    internal class SensitivityAxisNonparallelismCalculatedData : BaseModel, IProvData
    {
        private double i1Value;
        private string i1ValueStr;

        private double i2Value;
        private string i2ValueStr;

        private double fi0Value;
        private string fi0ValueStr;

        private double fi180Value;
        private string fi180ValueStr;

        public double I1Value
        {
            get => i1Value;
            set
            {
                int digits = 3;
                i1Value = value;
                I1ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I1ValueStr
        {
            get => i1ValueStr;
            private set
            {
                i1ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double I2Value
        {
            get => i2Value;
            set
            {
                int digits = 3;
                i2Value = value;
                I2ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string I2ValueStr
        {
            get => i2ValueStr;
            private set
            {
                i2ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double Fi0Value
        {
            get => fi0Value;
            set
            {
                int digits = 0;
                fi0Value = value;
                Fi0ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Fi0ValueStr
        {
            get => fi0ValueStr;
            private set
            {
                fi0ValueStr = value;
                OnPropertyChanged();
            }
        }

        public double Fi180Value
        {
            get => fi180Value;
            set
            {
                int digits = 0;
                fi180Value = value;
                Fi180ValueStr = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString($"F{digits}");
            }
        }

        public string Fi180ValueStr
        {
            get => fi180ValueStr;
            private set
            {
                fi180ValueStr = value;
                OnPropertyChanged();
            }
        }

        public void Clear()
        {
            I1Value = default;
            I1ValueStr = default;
            I2Value = default;
            I2ValueStr = default;
            Fi0Value = default;
            Fi0ValueStr = default;
            Fi180Value = default;
            Fi180ValueStr = default;
        }
    }
}
