/// <summary>
/// 功能说明: 调用UMC Web API创建和修改用户信息
/// </summary>
/// Author:    zhao.yu
/// Version:   1.0
/// Remark:    
/// History:   
/// Todo:
/// 
///
/// ---------------------------uaf solution studio 找用户和角色 web api
/// //找到全部用户
///http://172.21.81.17/sit-svc/engineering/odata/User
///找到用户下角色 -- 就是找到所有角色，再将角色中的userid与当前用户比对，只显示当前用户的角色
///http://172.21.81.17/sit-svc/engineering/odata/UserRoleAssociation?$expand=Role
///找到角色下的用户
///http://172.21.81.17/sit-svc/engineering/odata/UserRoleAssociation?$expand=Role&$filter=Role/Id%20eq%20057e1f0c-6fdd-45d3-a65a-66c80a50d206



using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Timers;
using Newtonsoft.Json;
using System.Linq;

namespace Siemens.Opcenter.Exfn.UMC
{

    public class UserManagement
    {
        private static HttpClient clientUMC;
        private const string LoginURI = "UMC/slwapi/login"; //登录地址

        private const string AddUserURI = "UMC/slwapi/users/add"; //添加用户
        private const string EditUserURI = "UMC/slwapi/users/updateinline"; //修改用户
        private const string ResetpswURI = "UMC/slwapi/resetpsw"; //重设密码
        private const string GetAllUsersURI = "UMC/slwapi/users"; //获取全部用户信息

        private const string S_GetUserDetails = "UMC/slwapi/users"; //获取用户信息
        private const string DeleteUsers = "UMC/slwapi/users/delete"; //删除用户

        //用户组相关信息
        private const string GetAllGroups = "UMC/slwapi/groups";//获取全部用户组信息
        private const string GetGroupDetails = "UMC/slwapi/groups/";//获取用户组的详细信息
        private const string CreateGroup = "UMC/slwapi/groups/add";//创建用户组
        private const string UpdateGroupBasic = "UMC/slwapi/groups/updateinline";//更新组信息（基础）
        private const string UpdateGroupFull = "UMC/slwapi/groups";//更新组信息，全部
        private const string DeleteGroups = "UMC/slwapi/groups/delete";//删除组的信息

        public static bool IsConnected
        {
            get;
            private set;
        }

        public static string BaseURL //网站url，本地为 http://localhost/
        {
            get;
            private set;
        }

        public static string AdminUserName
        {
            get;
            private set;
        }

        public static string AdminPassword
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
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>如果失败返回false</returns>
        public static ReturnMessage Initialize(string adminUserName, string adminPassword, string baseURL) 
        {
            ReturnMessage ret = new ReturnMessage
            {
                Succeed = false
            };

            IsConnected = false;

            AdminUserName = adminUserName;
            AdminPassword = adminPassword;
            BaseURL = baseURL;

            //初始化HttpClient
            HttpClientHandler handler = new HttpClientHandler() { UseCookies = true };
            clientUMC = new HttpClient(handler);
            clientUMC.Timeout = new TimeSpan(0, 0, 30);
            clientUMC.BaseAddress = new Uri(BaseURL);

            clientUMC.DefaultRequestHeaders.Accept.Clear();
            clientUMC.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            clientUMC.DefaultRequestHeaders.Connection.Add("keep-alive"); //长连接

            return Login(); //登录
        }

        private static ReturnMessage Login() //登录
        {
            ReturnMessage msg = new ReturnMessage
            {
                Succeed = false
            };

            try
            {
                if ((BaseURL==null) || (BaseURL == ""))
                {
                    IsConnected = false;
                    msg.Message = "请先使用Initialize方法进行初始化.";
                    return msg;
                }

                string uri = string.Format("{0}?user={1}&password={2}", LoginURI, AdminUserName, AdminPassword);
                HttpResponseMessage response = clientUMC.GetAsync(uri).Result;

                //var cookies = response.Headers.GetValues("Set-Cookie");

                //读取结果
                string result = response.Content.ReadAsStringAsync().Result;

                if (result.IndexOf("<html>") != -1) //说明返回的不是json
                {
                    IsConnected = false;
                    msg.Message = "调用登录Web API出错:" + result;
                    return msg;
                }

                JObject parsedJson = JObject.Parse(result);
                JToken res_Operation;
                if ((parsedJson.TryGetValue("operation", out res_Operation))) //找到登录操作标记
                {
                    if (res_Operation.ToString()== "loginresult") //是登录返回
                    {
                        JToken res_code;
                        if ((parsedJson.TryGetValue("result", out res_code))) //找到结果标记
                        {
                            if (res_code.ToString()=="0") //0为成功
                            {
                                IsConnected = true;
                                msg.Succeed = true;
                                return msg;
                            }
                            else
                            {
                                switch (res_code.ToString())
                                {
                                    case "2":
                                        msg.Message = "登录失败,管理员用户 "+ AdminUserName + " 已被锁定,请至UMC中解除锁定";
                                        break;
                                    case "3":
                                        msg.Message = "登录失败,管理员用户 " + AdminUserName + " 已被停用,请至UMC中启用";
                                        break;
                                    case "4":
                                        msg.Message = "登录失败,管理员用户名 " + AdminUserName + " 或密码错误";
                                        break;
                                    case "6":
                                        msg.Message = "登录失败,必须至UMC更改管理员用户 " + AdminUserName + " 的密码";
                                        break;
                                    case "7":
                                        msg.Message = "登录失败,管理员用户 " + AdminUserName + " 的密码过期,请至UMC中重设密码";
                                        break;
                                    default:
                                        msg.Message = "登录失败,返回状态为:" + res_code.ToString();
                                        break;
                                }

                                IsConnected = false;
                                return msg;
                            }
                        }
                        else
                        {
                            IsConnected = false;
                            msg.Message = "未找到登录返回结果,返回内容为:" + result;
                            return msg;
                        }
                    }
                    else
                    {
                        IsConnected = false;
                        msg.Message = "返回的不是当前登录操作结果,返回内容为:" + result;
                        return msg;
                    }
                }
                else
                {
                    IsConnected = false;
                    msg.Message = "未找到操作返回信息,返回内容为:"+result;
                    return msg;
                }

            }
            catch (Exception ex)
            {
                IsConnected = false;
                msg.Message = GetAllErrorMsg(ex);
                return msg;
            }
        }

        private static string GetAllErrorMsg(Exception ex) //递归获取全部错误信息
        {
            string msg = "";
            if (ex.InnerException != null)
                msg = "\r\n" + GetAllErrorMsg(ex.InnerException);

            return ex.Message + msg;
        }

        /// <summary>
        /// 发送数据，发送前后都做登录校验
        /// </summary>
        /// <param name="Uri">调用方法的URI</param>
        /// <param name="RequestBody">发送的参数</param>
        /// <param name="OperationResult">调用方法的返回关键字,用于比对结果</param>
        /// <param name="ResultMsg">返回0则成功，发送结果则resultMsg中，非0则为失败，错误信息则resultMsg中</param>
        /// <returns>返回0则成功，非0则为失败</returns>
        private static ReturnMessage SendJsonData(string Uri, StringContent RequestBody,string OperationResult)
        {
            ReturnMessage msg = new ReturnMessage
            {
                Succeed = false
            };

            try
            {
                if (!IsConnected) //如果没登录，则登录
                {
                    ReturnMessage msg1 = Login();
                    if (!msg1.Succeed)
                    { 
                        msg.Message = "登录UMC失败," + msg1.Message;
                        return msg;
                    }
                }

                //第一次发送数据
                HttpResponseMessage response = clientUMC.PostAsync(Uri, RequestBody).Result;

                //读取结果
                string result = response.Content.ReadAsStringAsync().Result;

                //处理结果
                if (result.IndexOf("/ipsimatic-logon\", \"nonce\":\"") != -1) //返回中含有【 /ipsimatic-logon","nonce":"】字符串说明登录超时，重新登录
                {
                    IsConnected = false;
                    ReturnMessage msg1 = Login();
                    if (!msg1.Succeed)
                    {
                        msg.Message = "登录UMC失败," + msg1.Message;
                        return msg;
                    }

                    //登录后再次发送数据和读取结果
                    response = clientUMC.PostAsync(Uri, RequestBody).Result;
                    result = response.Content.ReadAsStringAsync().Result;
                }

                if (result.IndexOf("<html>") != -1) //说明返回的不是json
                {
                    msg.Message = "调用登录Web API出错:" + result;
                    return msg;
                }

                JObject parsedJson = JObject.Parse(result);
                JToken res_Operation;
                if ((parsedJson.TryGetValue("operation", out res_Operation))) //找到操作标记
                {
                    if (res_Operation.ToString() == OperationResult) //是当前操作返回
                    {
                        JToken res_code;
                        if ((parsedJson.TryGetValue("result", out res_code))) //找到结果标记
                        {
                            if (res_code.ToString() == "0") //0为成功
                            {
                                msg.Succeed = true;
                                msg.Result = result; //返回成功的详细信息
                                return msg;
                            }
                            else
                            {
                                msg.Message = "操作失败,返回状态为:" + res_code.ToString();
                                return msg;
                            }

                        }
                        else
                        {
                            msg.Message = "未找当前操作返回结果,返回内容为:" + result;
                            return msg;
                        }
                    }
                    else
                    {
                        msg.Message = "返回的不是当前操作结果,返回内容为:" + result;
                        return msg;
                    }
                }
                else
                {
                    msg.Message = "未找到操作返回信息,返回内容为:" + result;
                    return msg;
                }
            }
            catch (System.Exception ex)
            {
                msg.Message = ex.Message;
                return msg;
            	
            }
        }

        /// <summary>
        /// 创建UMC用户 用户属性为(启用,可改密码,首次登录不改密码)  如果成功，返回新建的用户ID，如果返回的是负数，则为失败 
        /// </summary>
        /// <param name="UserName">用户名</param>
        /// <param name="Password">密码</param>
        /// <param name="FullName">全名</param>
        /// <returns>如果成功，返回新建的用户ID，如果返回的是负数，则为失败</returns>
        public static ReturnMessage AddUser(string UserName, string Password, string FullName)
        {
            return AddUser(UserName, Password, FullName, true, true, false);
        }

        /// <summary>
        /// 创建UMC用户 如果成功，返回新建的用户ID，如果返回的是负数，则为失败
        /// </summary>
        /// <param name="UserName">用户名</param>
        /// <param name="Password">密码</param>
        /// <param name="FullName">全名</param>
        /// <param name="Enabled">是否启用</param>
        /// <param name="CanChange">是否可以修改密码</param>
        /// <param name="MustChange">是否首次登录必须修改密码</param>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>如果成功，返回新建的用户ID，如果返回的是负数，则为失败</returns>
        private static ReturnMessage AddUser(string UserName, string Password, string FullName, bool Enabled, bool CanChange, bool MustChange) 
        {
            ReturnMessage msg = new ReturnMessage
            {
                Succeed = false
            };
            try
            {

                var user = new
                {
                    name= UserName,
                    password=Password,
                    fullname=FullName,
                    enabled= Enabled,
                    canchange= CanChange,
                    mustchange= MustChange,
                    locked=false
                };
                string strUser = JsonConvert.SerializeObject(user);

                StringContent RequestBody = new StringContent("["+ strUser + "]", Encoding.UTF8, "application/json"); //赋值
                ReturnMessage msg1 = SendJsonData(AddUserURI, RequestBody, "useraddresult"); //发送数据
                if (!msg1.Succeed)
                {
                    msg.Message = "发送出错," + msg1.Message;
                    return msg;
                }

                JObject parsedJson = JObject.Parse(msg1.Result.ToString());
                JToken res_Users;
                if ((parsedJson.TryGetValue("users", out res_Users))) //找到新建用户信息
                {
                    JToken fUser = res_Users.First; //取第一个用户
                    int res_code = fUser.Value<int>("result"); //取创建状态
                    if (res_code==0) //成功
                    {
                        int userID = fUser.Value<int>("id"); //取第一个用户的ID即可
                        msg.Succeed = true;
                        msg.Result = userID;
                        return msg; //返回新建用户id
                    }
                    else if (res_code == 13) //用户已存在
                    {
                        msg.Message = "创建失败,UMC中已存在此用户.";
                        return msg;
                    }
                    else
                    {
                        msg.Message = "创建失败,UMC错误码为:" + res_code.ToString();
                        return msg;
                    }
                }
                else
                {
                    msg.Message = "返回的信息中未找到新建用户信息,返回内容为:" + msg1.Result;
                    return msg;
                }
            }
            catch (System.Exception ex)
            {
                msg.Message = ex.Message;
            }

            return msg;
        }

        /// <summary>
        /// 获取UMC用户列表,废弃的方法
        /// </summary>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>返回用户列表，如果失败返回null</returns>
        private static ReturnMessage GetUserDetails(string name)
        {
            ReturnMessage msg = new ReturnMessage
            {
                Succeed = false
            };
            try
            {
                StringContent RequestBody = new StringContent("", Encoding.UTF8, "application/json"); //赋值
                ReturnMessage msg1 = SendJsonData("UMC/slwapi/users/2", RequestBody, "usersresult"); //发送数据
                if (!msg1.Succeed)
                {
                    msg.Message = "获取用户失败," + msg1.Message;
                    return null;
                }

                JObject parsedJson = JObject.Parse(msg1.Result.ToString());
                msg.Result = parsedJson.ToString();
                msg.Succeed = true;
                return msg;
            }
            catch (System.Exception ex)
            {
                msg.Message = ex.Message;
            }

            return msg;
        }

        /// <summary>
        /// 获取UMC用户列表
        /// </summary>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>返回用户列表，如果失败返回null</returns>
        public static ReturnMessage GetAllUsers()
        {
            ReturnMessage msg = new ReturnMessage
            {
                Succeed = true
            };
            try
            {
                StringContent RequestBody = new StringContent("", Encoding.UTF8, "application/json"); //赋值
                ReturnMessage msg1 = SendJsonData(GetAllUsersURI, RequestBody, "usersresult"); //发送数据
                if (!msg1.Succeed)
                {
                    msg.Message = "获取用户失败," + msg1.Message;
                    return msg;
                }

                JObject parsedJson = JObject.Parse(msg1.Result.ToString());
                JToken res_Users;
                if ((parsedJson.TryGetValue("users", out res_Users))) //找到用户列表
                {
                    msg.Result = JsonConvert.DeserializeObject(res_Users.ToString(), typeof(List<UMC_UserInfo>));
                    msg.Succeed = true;
                    return msg;
                }
                else
                {
                    msg.Message = "返回的信息中未找到用户列表,返回内容为:" + msg1.Message;
                    return msg;
                }
            }
            catch (System.Exception ex)
            {
                msg.Message = ex.Message;
            }

            return msg;
        }

        /// <summary>
        /// 根据UMC用户ID修改用户信息,用户属性为(可改密码,首次登录不改密码) 
        /// </summary>
        /// <param name="UserID">UMC的用户ID</param>
        /// <param name="UserID">UMC的用户名</param>
        /// <param name="Password">密码,如果不需要修改密码，则传入""</param>
        /// <param name="FullName">全名</param>
        /// <param name="Enabled">是否启用</param>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>返回成功或失败</returns>
        public static ReturnMessage EditUser(int UserID, string UserName, string Password, string FullName, bool Enabled)
        {
            return EditUser(UserID, UserName, Password, FullName, Enabled, true, false);
        }

        /// <summary>
        /// 根据UMC用户ID修改用户信息
        /// </summary>
        /// <param name="UserID">UMC的用户ID</param>
        /// <param name="UserID">UMC的用户名</param>
        /// <param name="Password">密码,如果不需要修改密码，则传入""</param>
        /// <param name="FullName">全名</param>
        /// <param name="Enabled">是否启用</param>
        /// <param name="CanChange">是否可以修改密码</param>
        /// <param name="MustChange">是否首次登录必须修改密码</param>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>返回成功或失败</returns>
        private static ReturnMessage EditUser(int UserID, string UserName, string Password, string FullName, bool Enabled, bool CanChange, bool MustChange) 
        {
            ReturnMessage msg = new ReturnMessage
            {
                Succeed = true
            };
            try
            {
                var user = new
                {
                    id = UserID,
                    name = UserName,
                    password = Password,
                    fullname = FullName,
                    enabled = Enabled,
                    canchange = CanChange,
                    mustchange = MustChange,
                    locked = false
                };
                string strUser = JsonConvert.SerializeObject(user);

                StringContent RequestBody = new StringContent("[" + strUser + "]", Encoding.UTF8, "application/json"); //赋值
                ReturnMessage msg1 = SendJsonData(EditUserURI, RequestBody, "userupdateresult"); //发送数据
                if (!msg1.Succeed)
                {
                    msg.Message = "编辑出错," + msg1.Message;
                    return msg;
                }

                JObject parsedJson = JObject.Parse(msg1.Result.ToString());
                JToken res_Users;
                if ((parsedJson.TryGetValue("users", out res_Users))) //找到修改用户信息
                {
                    JToken fUser = res_Users.First; //取第一个用户
                    int res_code = fUser.Value<int>("result"); //取创建状态
                    if (res_code == 0) //成功
                    {
                        msg.Succeed = true;
                        return msg; //返回成功
                    }
                    else if (res_code == 273) //用户不存在
                    {
                        msg.Message = "修改失败,UMC中不存在此用户.";
                        return msg;
                    }
                    else
                    {
                        msg.Message = "修改失败,UMC错误码为:" + res_code.ToString();
                        return msg;
                    }
                }
                else
                {
                    msg.Message = "返回的信息中未找到修改完成信息,返回内容为:" + msg1.Result;
                    return msg;
                }
            }
            catch (Exception ex)
            {
                msg.Message = ex.Message;
                return msg;
            }
        }

        /// <summary>
        /// 根据UMC用户名修改用户信息,用户属性为(可改密码,首次登录不改密码) --比直接用用户ID修改效率低很多(需要先去获取所有UMC用户，再根据用户名搜索用户ID，再去用用户ID修改信息)
        /// </summary>
        /// <param name="UserID">UMC的用户名</param>
        /// <param name="Password">密码,如果不需要修改密码，则传入""</param>
        /// <param name="FullName">全名</param>
        /// <param name="Enabled">是否启用</param>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>返回成功或失败</returns>
        public static ReturnMessage EditUser(string UserName, string Password, string FullName, bool Enabled)
        {
            return EditUser(UserName, Password, FullName, Enabled, true, false);
        }

        /// <summary>
        /// 根据UMC用户名修改用户信息 --比直接用用户ID修改效率低很多(需要先去获取所有UMC用户，再根据用户名搜索用户ID，再去用用户ID修改信息)
        /// </summary>
        /// <param name="UserName">UMC的用户名</param>
        /// <param name="Password">密码,如果不需要修改密码，则传入""</param>
        /// <param name="FullName">全名</param>
        /// <param name="Enabled">是否启用</param>
        /// <param name="CanChange">是否可以修改密码</param>
        /// <param name="MustChange">是否首次登录必须修改密码</param>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>返回成功或失败</returns>
        private static ReturnMessage EditUser(string UserName, string Password, string FullName, bool Enabled, bool CanChange, bool MustChange)
        {
            ReturnMessage msg = new ReturnMessage { Succeed = false };
            try
            {
                //int userID = GetUserIDbyName(UserName, out ErrorMsg); //根据名字获取ID
                //if (userID==-1)
                //{
                //    ErrorMsg = "获取用户ID出错," + ErrorMsg;
                //    return false;
                //}

                ////修改用户信息
                //return EditUser(userID, UserName, Password, FullName, Enabled, CanChange, MustChange, out ErrorMsg);

                return msg;
            }
            catch (System.Exception ex)
            {
                msg.Message = ex.Message;
                return msg;
            }
        }


        //根据用户ID和用户名启用或停用用户
        private static ReturnMessage SetUserEnabled(int UserID, string UserName, string FullName, bool isEnabled)
        {
            ReturnMessage msg = new ReturnMessage { Succeed = false};
            try
            {
                return EditUser(UserID, UserName, "", FullName, isEnabled); //直接调用修改方法
            }
            catch (Exception ex)
            {
                msg.Message = ex.Message;
                return msg;
            }
        }

        //根据用户名启用或停用用户 --比直接用用户ID修改效率低很多(需要先去获取所有UMC用户，再根据用户名搜索用户ID，再去用用户ID修改信息)
        private static ReturnMessage SetUserEnabled(string UserName, bool isEnabled) 
        {
            ReturnMessage msg = new ReturnMessage { Succeed = false };
            try
            {
                //UMC_UserInfo uinfo = GetUserBaseInfo(UserName, out ErrorMsg); //根据用户名获取用户信息
                //if (uinfo == null)
                //{
                //    ErrorMsg = "获取用户信息出错," + ErrorMsg;
                //    return false;
                //}

                //return EditUser(uinfo.ID, UserName, "", uinfo.FullName, isEnabled, out ErrorMsg); //调用修改方法
                return msg;
            }
            catch (System.Exception ex)
            {
                msg.Message = ex.Message;
                return msg;
            }
        }

        /// <summary>
        /// 根据用户ID和用户名启用UMC的用户
        /// </summary>
        /// <param name="UserID">要启用的用户ID</param>
        /// <param name="UserName">要启用的用户名</param>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>返回成功或失败</returns>
        public static ReturnMessage EnableUser(int UserID, string UserName, string FullName)
        {
            return SetUserEnabled(UserID, UserName, FullName, true);
        }

        /// <summary>
        /// 根据用户名启用UMC的用户 --比直接用用户ID修改效率低很多(需要先去获取所有UMC用户，再根据用户名搜索用户ID，再去用用户ID修改信息)
        /// </summary>
        /// <param name="UserName">要启用的用户名</param>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>返回成功或失败</returns>
        public static ReturnMessage EnableUser(string UserName)
        {
            return SetUserEnabled(UserName,true);
        }

        /// <summary>
        /// 根据用户ID和用户名停用UMC的用户
        /// </summary>
        /// <param name="UserID">要停用的用户ID</param>
        /// <param name="UserName">要停用的用户名</param>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>返回成功或失败</returns>
        public static ReturnMessage DisableUser(int UserID, string UserName, string FullName)
        {
            return SetUserEnabled(UserID, UserName, FullName, false);
        }

        /// <summary>
        /// 根据用户名停用UMC的用户 --比直接用用户ID修改效率低很多(需要先去获取所有UMC用户，再根据用户名搜索用户ID，再去用用户ID修改信息)
        /// </summary>
        /// <param name="UserName">要停用的用户名</param>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns>返回成功或失败</returns>
        public static ReturnMessage DisableUser(string UserName)
        {
            return SetUserEnabled(UserName, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="NewPassword"></param>
        /// <param name="ErrorMsg">如果失败,返回的错误信息</param>
        /// <returns></returns>
        public static ReturnMessage ResetPassword(string UserName,string NewPassword) //重设用户密码
        {
            ReturnMessage msg = new ReturnMessage { Succeed = false};
            try
            {

                var ResetPsw = new
                {
                    usertoreset = UserName,
                    pswtoreset = NewPassword
                };
                string strResetPsw = JsonConvert.SerializeObject(ResetPsw);

                StringContent RequestBody = new StringContent("[" + strResetPsw + "]", Encoding.UTF8, "application/json"); //赋值
                ReturnMessage msg1 = SendJsonData(ResetpswURI, RequestBody, "pswresetresult"); //发送数据
                if (!msg1.Succeed)
                {
                    msg.Message = "重置密码出错," + msg1.Message;
                    return msg;
                }
                else {
                    msg.Succeed = true;
                    return msg; //更改成功
                }
            }
            catch (System.Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }
    }

    //用户信息
    public class UMC_UserInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public int ObjVer { get; set; }
        public string Comment { get; set; }
        public int UserFlags { get; set; }
    }

}
