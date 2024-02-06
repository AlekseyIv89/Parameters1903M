using System;

namespace Parameters1903M.Model
{
    public class Parameter : BaseModel
    {
        private string num;
        private string name;
        private double val;
        private int digits;
        private string strValue;
        private string unit;

        /// <summary>
        /// Номер проверки
        /// </summary>
        public string Num
        {
            get => num;
            set => num = value;
        }

        /// <summary>
        /// Название проверки
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Значение рассчитанной величины
        /// </summary>
        public double Value
        {
            get => val;
            set
            {
                val = value;
                if (!double.IsNaN(val))
                {
                    StrValue = Math.Round(value, Digits, MidpointRounding.AwayFromZero).ToString($"F{Digits}");
                }
                else
                {
                    StrValue = default;
                }
            }
        }

        public int Digits
        {
            get => digits;
            set
            {
                digits = value;
            }
        }

        /// <summary>
        /// Строковое значение рассчитанной величины
        /// </summary>
        public string StrValue
        {
            get => strValue;
            set
            {
                strValue = value;
                if (value != default)
                {
                    IsMeasured = true;
                }
                else
                {
                    IsMeasured = false;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Единица измерения
        /// </summary>
        public string Unit
        {
            get => unit;
            set
            {
                unit = value;
            }
        }

        public bool IsMeasured { get; private set; }

        public Parameter AssociatedParameter { get; set; }

        public void ValueClean()
        {
            Value = double.NaN;
        }
    }
}
