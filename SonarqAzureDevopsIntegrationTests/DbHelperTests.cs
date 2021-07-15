using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sonar2PBI.Tests
{
    [TestClass()]
    public class DbHelperTests
    {
        //[TestMethod()]
        //[DataRow("ArfleetMicroservicesPK")]
        //public void GetProjectsSingleProjectTest(string projectKey)
        //{
        //    DbHelper dbHelper = new DbHelper();
        //    var projects = dbHelper.GetProjects(projectKey);
        //    Assert.IsTrue(projects.Count == 1);
        //}
        //[TestMethod()]
        //public void GetProjectsTest()
        //{
        //    DbHelper dbHelper = new DbHelper();
        //    var projects = dbHelper.GetProjects(string.Empty);
        //    Assert.IsTrue(projects.Count > 1);
        //}

        [TestMethod()]
        public void GetProjectCountTest()
        {
            DbHelper dbHelper = new DbHelper();
            int count = dbHelper.GetProjectCount();
            Assert.IsTrue(count > 0);
        }
    }
}