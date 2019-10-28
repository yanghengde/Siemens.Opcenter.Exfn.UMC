using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Siemens.Opcenter.Exfn.UMC.UnitTest
{
    /// <summary>
    /// Summary description for RoleManagementTest
    /// </summary>
    [TestClass]
    public class RoleManagementTest
    {
        public RoleManagementTest()
        {
            RoleManagement.Initialize("OPFN30\\Administrator", "SwqaMe$1", "http://localhost");
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestGetFullRoles()
        {
            ReturnMessage msg = RoleManagement.GetFullRoles();
            Assert.IsTrue(msg.Succeed);
        }

        [TestMethod]
        public void TestGetUser()
        {
            ReturnMessage msg = RoleManagement.GetUser("a");
            Assert.IsTrue(msg.Succeed);
        }

        [TestMethod]
        public void TestGetUserRoleAssociation()
        {
            ReturnMessage msg = RoleManagement.GetUserRoleAssociation("2","ccc");
            Assert.IsTrue(msg.Succeed);
        }

        [TestMethod]
        public void TestCreateRole()
        {
            ReturnMessage msg = RoleManagement.CreateRole("test", "test");
            Assert.IsTrue(msg.Succeed);
        }

        [TestMethod]
        public void TestCreateUserRoleAssociation()
        {
            string[] arr = new string[] {"28","32"};
            ReturnMessage msg = RoleManagement.CreateUserRoleAssociation("ccc", arr);
            Assert.IsTrue(msg.Succeed);
        }

    }
}
