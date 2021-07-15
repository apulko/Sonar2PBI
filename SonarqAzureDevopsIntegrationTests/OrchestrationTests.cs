using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sonar2PBI.Tests
{
    [TestClass()]
    public class OrchestrationTests
    {
        [TestMethod()]
        [DataRow(@"Projects", "Projects")]
        [DataRow(@"Projects\Arma 1", "Projects")]
        [DataRow(@"Projects\Arma 1\spint23", @"Projects\Arma 1")]
        [DataRow(@"Projects\YNA", "Projects")]
        [DataRow(@"YNA", "YNA")]
        [DataRow(@"", "")]
        public void GetProjectNameTest(string area, string expected)
        {
            string actual = Orchestration.GetUpperPath(area);
            Assert.AreEqual(expected, actual);
        }
    }
}