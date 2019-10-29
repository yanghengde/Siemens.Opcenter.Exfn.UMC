using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Siemens.Opcenter.Exfn.UMC.DataModel;
using Newtonsoft.Json;

namespace Siemens.Opcenter.Exfn.UMC.UnitTest
{
    /// <summary>
    /// Summary description for OpfnUmcManagement
    /// </summary>
    [TestClass]
    public class OpfnUmcManagementTest
    {
        private OpfnUmcManagement umcm;
        public OpfnUmcManagementTest()
        {
            //初始化Token与Cookie的信息，这个是调用之前必须的
            umcm = new OpfnUmcManagement("OPFN30\\Administrator", "SwqaMe$1", "http://localhost","abc123");
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
        public void TestSyncUserRoles()
        {
            UmcUser uu = this.GetTestData();
            string input = JsonConvert.SerializeObject(uu);
            ReturnMessage ret = umcm.SyncUserRoles(input);
            Assert.IsTrue(ret.Succeed);
        }

        /// <summary>
        /// 测试数据，自己做的Json
        /// </summary>
        /// <returns>返回测试Json串</returns>
        public UmcUser GetTestData()
        {
            UmcUser umc = new UmcUser();
            User u1 = new User { Name = "V-001", FullName = "V-001" };
            User u2 = new User { Name = "V-002", FullName = "V-002" };
            umc.Users = new List<User>();
            umc.Users.Add(u1);
            umc.Users.Add(u2);

            Role r = new Role { Name = "VDEMO-01", Description = "VDEMO-01", IsSystemRole = false };
            umc.Roles = new List<Role>();
            umc.Roles.Add(r);

            UserRole ur1 = new UserRole { RoleName = "VDEMO-01", UserName = "V-001" };
            UserRole ur2 = new UserRole { RoleName = "VDEMO-01", UserName = "V-002" };
            umc.UserRoles = new List<UserRole>();
            umc.UserRoles.Add(ur1);
            umc.UserRoles.Add(ur2);

            return umc;
        }
    }
}
