using System;

namespace Parameters1903M.Util.Multimeter
{
    internal class MeasureResult
    {
        public DateTime DateTime { get; }
        public double Result { get; }

        public MeasureResult(double result)
        {
            DateTime = DateTime.Now;
            Result = result;
        }
    }
}
