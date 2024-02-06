using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parameters1903M.Util;

namespace Parameters1903MTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ConvertVoltToMilliAmpereTest()
        {
            double expected = -110.99889;
            double result = Converter.ConvertVoltToMilliAmpere(-1.11);

            Assert.AreEqual(expected.ToString("F5"), result.ToString("F5"));
        }
    }
}
