using System;

namespace Parameters1903M.Util.Multimeter
{
    internal class MeasureResult
    {
        public DateTime DateTime { get; }
        public double Value { get; }

        public MeasureResult(double result)
        {
            DateTime = DateTime.Now;
            Value = result;
        }
    }
}
