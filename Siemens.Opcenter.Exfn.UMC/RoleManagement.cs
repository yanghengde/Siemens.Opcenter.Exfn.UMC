using Newtonsoft.Json;
using Siemens.Opcenter.Exfn.Odata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens.Opcenter.Exfn.UMC
{
    public class RoleManagement
    {

        public static string Token
        {
            get;
            private set;
        }
        /// <summary>
        /// 初始化UMC静态类,并使用传入的管理员登录UMC。
        /// </summary>
        /// <param name="adminUserName">UMC管理员用户名</param>
        /// <param name="adminPassword">UMC管理员密码</param>
        /// <param name="baseURL">UMC服务器地址</param>
        /// <returns>如果失败返回false</returns>
        public static void Initialize(string adminUserName, string adminPassword, string baseURL)
        {
            try { 
                OAuth2Token.Initialize(adminUserName, adminPassword, "", baseURL);
                OData4.Initialize(baseURL);
                Token = OAuth2Token.Token;
               
            }catch(Exception ex)
            {
                Token = string.Empty;
                throw ex;
            }
        }

        /// <summary>
        /// 获取所有的角色信息
        /// </summary>
        /// <returns>返回ReturnMesasge对象，角色在Results中</returns>
        public static ReturnMessage GetFullRoles()
        {
            ReturnMessage msg = new ReturnMessage { Succeed = false };
            try
            {
                string result = OData4.QueryData(Token, "Role", "", DataScope.Engineering);
                if (!string.IsNullOrEmpty(result))
                {
                    msg.Succeed = true;
                    msg.Result = result;
                }
            }
            catch(Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }

        /// <summary>
        /// 获取所有的用户信息
        /// </summary>
        /// <returns>返回ReturnMesasge对象，角色在Results中</returns>
        public static ReturnMessage GetFullUsers()
        {
            ReturnMessage msg = new ReturnMessage { Succeed = false };
            try
            {
                string message = OData4.QueryData(Token, "User", "", DataScope.Engineering);
                if (!string.IsNullOrEmpty(message))
                {
                    msg.Succeed = true;
                    msg.Message = message;
                }
            }
            catch (Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }

        /// <summary>
        /// 获取单个用户信息
        /// </summary>
        /// <returns>返回ReturnMesasge对象，角色在Results中</returns>
        public static ReturnMessage GetUser(string username)
        {
            ReturnMessage msg = new ReturnMessage { Succeed = false };
            try
            {
                string result = OData4.QueryData(Token, "User", "$filter=(Name eq '"+username+"')", DataScope.Engineering);
                if (!string.IsNullOrEmpty(result))
                {
                    msg.Succeed = true;
                    msg.Result = result;
                }
            }
            catch (Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }

        //http://opfn30/sit-svc/engineering/odata/UserRoleAssociation?$expand=Role&$filter=Role/Id%20eq%205ec5e188-da5f-45a5-8d31-4ab0d866c9b1
        /// <summary>
        /// 获取所有的用户信息
        /// </summary>
        /// <returns>返回ReturnMesasge对象，角色在Results中</returns>
        public static ReturnMessage GetUserRoleAssociation(string userid,string rolename)
        {
            ReturnMessage msg = new ReturnMessage { Succeed = false };
            try
            {
                string message = OData4.QueryData(Token, "UserRoleAssociation", "$expand=Role&$filter=(Role/Name eq '" + rolename+"') And (UserID eq '"+ userid + "')", DataScope.Engineering);
                if (!string.IsNullOrEmpty(message))
                {
                    msg.Succeed = true;
                    msg.Message = message;
                }
            }
            catch (Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }


        /// <summary>
        /// 创建角色信息
        /// </summary>
        /// <param name="intput">{"command":{"Name":"ccc","Description":"ccc","IsSystemRole":false}}</param>
        /// <returns></returns>
        public static ReturnMessage CreateRole(string intput)
        {
            ReturnMessage msg = new ReturnMessage { Succeed = false };
            try
            {
                string message1 = OData4.SendCmd(Token, "CreateRoleCommand", intput, DataScope.Engineering);
                if (!string.IsNullOrEmpty(message1))
                {
                    msg.Succeed = true;
                    msg.Message = message1;
                }
            }
            catch (Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }

        /// <summary>
        /// 通过角色名称与描述创建角色信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static ReturnMessage CreateRole(string name, string description)
        {
            string input = "{\"command\":{\"Name\":\""+name+"\",\"Description\":\""+description+"\",\"IsSystemRole\":false}}";
            return CreateRole(input);
        }

        /// <summary>
        /// 创建完成角色之后，给角色赋调用权限，默认全部打开，若是关闭某些权限，请在Studio里操作
        /// </summary>
        /// <param name="intput">{"command":{"Securables":[{"SecurableCategoryName":"business_command","Operations":[{"Name":"invoke","SecurableObjectNames":["business_command_root"]}]},{"SecurableCategoryName":"business_entity","Operations":[{"Name":"read","SecurableObjectNames":["business_entity_root"]}]},{"SecurableCategoryName":"business_event","Operations":[{"Name":"fire","SecurableObjectNames":["business_event_root"]},{"Name":"subscribe","SecurableObjectNames":["business_event_root"]},{"Name":"unsubscribe","SecurableObjectNames":["business_event_root"]}]},{"SecurableCategoryName":"screen","Operations":[{"Name":"view","SecurableObjectNames":["screen_root"]}]},{"SecurableCategoryName":"business_signal","Operations":[{"Name":"subscribe","SecurableObjectNames":["business_signal_root"]}]},{"SecurableCategoryName":"business_reading_function","Operations":[{"Name":"execute","SecurableObjectNames":["business_reading_function_root"]}]}],"RoleName":"ccc"}}</param>
        /// <returns></returns>
        public static ReturnMessage AddFunctionRightsToRole_Before(string intput)
        {
            ReturnMessage msg = new ReturnMessage { Succeed = false };
            try
            {
                string message1 = OData4.SendCmd(Token, "AddFunctionRightsToRoleCommand", intput, DataScope.Engineering);
                if (!string.IsNullOrEmpty(message1))
                {
                    msg.Succeed = true;
                    msg.Message = message1;
                }
            }
            catch (Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }

        public static ReturnMessage AddFunctionRightsToRole(string rolename)
        {
            string input = "{\"command\":{\"Securables\":[{\"SecurableCategoryName\":\"business_command\",\"Operations\":[{\"Name\":\"invoke\",\"SecurableObjectNames\":[\"business_command_root\"]}]},{\"SecurableCategoryName\":\"business_entity\",\"Operations\":[{\"Name\":\"read\",\"SecurableObjectNames\":[\"business_entity_root\"]}]},{\"SecurableCategoryName\":\"business_event\",\"Operations\":[{\"Name\":\"fire\",\"SecurableObjectNames\":[\"business_event_root\"]},{\"Name\":\"subscribe\",\"SecurableObjectNames\":[\"business_event_root\"]},{\"Name\":\"unsubscribe\",\"SecurableObjectNames\":[\"business_event_root\"]}]},{\"SecurableCategoryName\":\"screen\",\"Operations\":[{\"Name\":\"view\",\"SecurableObjectNames\":[\"screen_root\"]}]},{\"SecurableCategoryName\":\"business_signal\",\"Operations\":[{\"Name\":\"subscribe\",\"SecurableObjectNames\":[\"business_signal_root\"]}]},{\"SecurableCategoryName\":\"business_reading_function\",\"Operations\":[{\"Name\":\"execute\",\"SecurableObjectNames\":[\"business_reading_function_root\"]}]}],\"RoleName\":\""+rolename+"\"}}";
            return AddFunctionRightsToRole_Before(input);
        }

        /// <summary>
        /// 角色与用户关联
        /// </summary>
        /// <param name="intput">{"command":{"UserIds":["32","28"],"RoleName":"ccc","DomainName":""}}</param>
        /// /// <returns></returns>
        public static ReturnMessage CreateUserRoleAssociation(string intput)
        {
            ReturnMessage msg = new ReturnMessage { Succeed = false };
            try
            {
                string message1 = OData4.SendCmd(Token, "CreateUserRoleAssociationCommand", intput, DataScope.Engineering);
                if (!string.IsNullOrEmpty(message1))
                {
                    msg.Succeed = true;
                    msg.Message = message1;
                }
            }
            catch (Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }

        /// <summary>
        /// 角色与用户关联
        /// </summary>
        /// <param name="intput">{"command":{"UserIds":["32","28"],"RoleName":"ccc","DomainName":""}}</param>
        /// /// <returns></returns>
        public static ReturnMessage CreateUserRoleAssociation(string rolename,string [] userids)
        {
            string input = "{\"command\":{\"UserIds\":"+JsonConvert.SerializeObject(userids)+",\"RoleName\":\""+rolename+"\",\"DomainName\":\"\"}}";
            return CreateUserRoleAssociation(input);
        }

    }

}
