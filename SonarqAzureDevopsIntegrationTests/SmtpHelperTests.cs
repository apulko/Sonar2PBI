using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sonar2PBI.Tests
{
    [TestClass()]
    public class SmtpHelperTests
    {
        [TestMethod()]
        [DataRow("Eposta Body", "[Sonar2PBI] EpostaGonderTest")]
        public void EpostaGonderTest(string body, string konu)
        {
            SmtpHelper.EpostaGonder(body, konu);
            Assert.IsTrue(true);
        }
    }
}