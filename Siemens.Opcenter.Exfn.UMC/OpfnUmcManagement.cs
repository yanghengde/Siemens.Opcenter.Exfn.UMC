using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Siemens.Opcenter.Exfn.UMC.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens.Opcenter.Exfn.UMC
{
    public class OpfnUmcManagement
    {
        private string _DefaultPassword;

        /// <summary>
        /// 初始化信息，主要是Token和Cookie的信息
        /// </summary>
        /// <param name="adminuser">管理员用户，有操作UMC和Studio的权限</param>
        /// <param name="adminpassword">密码</param>
        /// <param name="url">UAF的机器，对应于Studio的那台，比如本地：http://localhost</param>
        public OpfnUmcManagement(string adminuser, string adminpassword, string url,string defaultPassword)
        {
            _DefaultPassword = defaultPassword;
            //UserManagement.Initialize("OPFN30\\Administrator", "SwqaMe$1", "http://localhost");
            UserManagement.Initialize(adminuser, adminpassword, url);
            RoleManagement.Initialize(adminuser, adminpassword, url);
        }

        public ReturnMessage SyncUserRoles(string input)
        {
            ReturnMessage msg = new ReturnMessage
            {
                Succeed = false
            };
            try
            {
                UmcUser umc = JsonConvert.DeserializeObject<UmcUser>(input);

                ReturnMessage usermsg = this.CreateUser(umc);
                if (!usermsg.Succeed)
                {
                    msg.Message = "创建用户失败,详细信息：" + usermsg.Message;
                }

                ReturnMessage rolemsg = this.CreateRole(umc);
                if (!rolemsg.Succeed)
                {
                    msg.Message = "创建角色失败,详细信息：" + rolemsg.Message;
                }

                ReturnMessage userrolemsg = this.CreateUserRoleAssociation(umc);
                if (!userrolemsg.Succeed)
                {
                    msg.Message = "创建用户角色关联失败,详细信息：" + userrolemsg.Message;
                }

                Dictionary<string, object> errors = new Dictionary<string, object>();
                errors.Add("usermsg", usermsg.Result);
                errors.Add("rolemsg", rolemsg.Result);
                errors.Add("userrolemsg", userrolemsg.Result);

                msg.Succeed = true;
                msg.Result = errors;

                return msg;
            }
            catch(Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="umc">传入UMC对象</param>
        /// <returns></returns>
        private ReturnMessage CreateUser(UmcUser umc)
        {
            ReturnMessage msg = new ReturnMessage
            {
                Succeed = false
            };
            try
            {
               if(umc == null || umc.Users == null || umc.Users.Count == 0)
                {
                    msg.Message = "无传入过来的用户名，请查看接口信息";
                    return msg;
                }

               IList<string> errUserList = new List<string>();
               foreach(User entity in umc.Users)
                {
                    try
                    {
                        ReturnMessage gumsg = RoleManagement.GetUser(entity.Name);
                        if (!gumsg.Succeed)
                        {
                            errUserList.Add(string.Format("查询用户[{0}]失败,原因：[{1}]", entity.Name, gumsg.Message));
                            continue;
                        }

                        if (gumsg.Result != null)
                        {
                            JObject parsedJson1 = JObject.Parse(gumsg.Result.ToString());
                            JToken res_Users;
                            if ((parsedJson1.TryGetValue("value", out res_Users))) //找到用户列表
                            {
                                List<User> roles = (List<User>)JsonConvert.DeserializeObject(res_Users.ToString(), typeof(List<User>));
                                List<User> exists = roles.Where(p => p.Name == entity.Name).ToList<User>();
                                if (exists != null && exists.Count > 0)
                                {
                                    errUserList.Add(string.Format("用户[{0}]已存在,不允许创建重复用户", entity.Name));
                                    continue;
                                }
                            }
                        }

                        ReturnMessage ret = UserManagement.AddUser(entity.Name, _DefaultPassword, entity.FullName);
                        if (!ret.Succeed)
                        {
                            errUserList.Add(string.Format("创建用户[{0}]失败,原因：[{1}]", entity.Name, ret.Message));
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        errUserList.Add(string.Format("创建用户[{0}]失败,原因：[{1}]", entity.Name, ex.Message));
                    }
                    msg.Succeed = true;
                }
                msg.Result = errUserList;
            }
            catch (Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="umc">传入UMC对象</param>
        /// <returns></returns>
        private ReturnMessage CreateRole(UmcUser umc)
        {
            ReturnMessage msg = new ReturnMessage
            {
                Succeed = false
            };
            try
            {
                if (umc == null || umc.Roles == null || umc.Roles.Count == 0)
                {
                    msg.Message = "无传入过来的角色，请查看接口信息";
                    return msg;
                }

                ReturnMessage gumsg = RoleManagement.GetFullRoles();
                if (!gumsg.Succeed)
                {
                    msg.Message = string.Format("角色获取失败,原因：[{0}]", gumsg.Message);
                    return msg;
                }

                IList<string> errRoleList = new List<string>();
                foreach (Role entity in umc.Roles)
                {
                    try
                    {
                        JObject parsedJson = JObject.Parse(gumsg.Result.ToString());
                        JToken res_Roles;
                        if ((parsedJson.TryGetValue("value", out res_Roles))) //找到用户列表
                        {
                            List<Role> roles = (List<Role>)JsonConvert.DeserializeObject(res_Roles.ToString(), typeof(List<Role>));
                            List<Role> exists = roles.Where(p => p.Name == entity.Name).ToList<Role>();
                            if(exists != null && exists.Count > 0)
                            {
                                errRoleList.Add(string.Format("角色[{0}]已经存在,不允许重复创建",entity.Name));
                                continue;
                            }
                        }

                        ReturnMessage ret = RoleManagement.CreateRole(entity.Name,entity.Description);
                        if (!ret.Succeed)
                        {
                            errRoleList.Add(string.Format("创建角色[{0}]失败,原因：[{1}]", entity.Name, ret.Message));
                            continue;
                        }

                        //下面这个动作有bug，创建完成角色之后，在Stuido里自行进行设置@hengde 2019-09-21
                        //ReturnMessage ret_attr = RoleManagement.AddFunctionRightsToRole(entity.Name);
                        //if (!ret_attr.Succeed)
                        //{
                        //    errRoleList.Add(string.Format("角色[{0}]权限赋值失败,原因：[{1}]", entity.Name, ret_attr.Message));
                        //    continue;
                        //}
                        msg.Succeed = true;
                    }
                    catch (Exception ex)
                    {
                        errRoleList.Add(string.Format("创建角色[{0}]失败,原因：[{1}]", entity.Name, ex.Message));
                    }
                }
                msg.Result = errRoleList;
            }
            catch (Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }

        /// <summary>
        /// 关联角色与用户
        /// </summary>
        /// <param name="umc">传入UMC对象</param>
        /// <returns></returns>
        private ReturnMessage CreateUserRoleAssociation(UmcUser umc)
        {
            ReturnMessage msg = new ReturnMessage
            {
                Succeed = false
            };
            try
            {
                if (umc == null || umc.UserRoles == null || umc.UserRoles.Count == 0)
                {
                    msg.Message = "无用户与角色关联信息";
                    return msg;
                }

                ReturnMessage rolemsg = RoleManagement.GetFullRoles();
                if (!rolemsg.Succeed)
                {
                    msg.Message = string.Format("角色获取失败,原因：[{0}]", rolemsg.Message);
                    return msg;
                }

                IList<string> errUserRoleList = new List<string>();
                foreach (UserRole entity in umc.UserRoles)
                {
                    try
                    {
                        JObject parsedJson = JObject.Parse(rolemsg.Result.ToString());
                        JToken res_Roles;
                        if ((parsedJson.TryGetValue("value", out res_Roles))) //找到用户列表
                        {
                            List<Role> roles = (List<Role>)JsonConvert.DeserializeObject(res_Roles.ToString(), typeof(List<Role>));
                            List<Role> exists = roles.Where(p => p.Name == entity.RoleName).ToList<Role>();
                            if (exists == null || exists.Count == 0)
                            {
                                errUserRoleList.Add(string.Format("角色[{0}]不存在,不允许创建用户[{1}]关联", entity.RoleName,entity.UserName));
                                continue;
                            }
                        }

                        ReturnMessage gumsg = RoleManagement.GetUser(entity.UserName);
                        if (!gumsg.Succeed)
                        {
                            errUserRoleList.Add(string.Format("查询用户[{0}]失败,原因：[{1}]", entity.UserName, gumsg.Message));
                            continue;
                        }

                        JObject parsedJson1 = JObject.Parse(gumsg.Result.ToString());
                        JToken res_Users;
                        User relUser = null;
                        if ((parsedJson1.TryGetValue("value", out res_Users))) //找到用户列表
                        {
                            List<User> roles = (List<User>)JsonConvert.DeserializeObject(res_Users.ToString(), typeof(List<User>));
                            List<User> exists = roles.Where(p => p.Name == entity.UserName).ToList<User>();
                            if (exists == null || exists.Count == 0)
                            {
                                errUserRoleList.Add(string.Format("用户[{0}]不存在,不允许创建用户[{1}]关联",  entity.UserName, entity.RoleName));
                                continue;
                            }

                            relUser = exists[0];
                        }

                        if(relUser == null)
                        {
                            errUserRoleList.Add(string.Format("用户[{0}]不存在,不允许创建用户[{1}]关联", entity.RoleName, entity.UserName));
                            continue;
                        }

                        ReturnMessage relationmsg = RoleManagement.CreateUserRoleAssociation(entity.RoleName, new string[] {relUser.Id });
                        if (!relationmsg.Succeed)
                        {
                            errUserRoleList.Add(string.Format("创建用户[{0}]与角色关联[{1}]失败,原因：[{2}]", entity.UserName,entity.RoleName, relationmsg.Message));
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        errUserRoleList.Add(string.Format("创建用户[{0}]与角色关联[{1}]失败,原因：[{2}]", entity.UserName, entity.RoleName, ex.Message));
                    }
                    msg.Succeed = true;
                }
                msg.Result = errUserRoleList;
            }
            catch (Exception ex)
            {
                msg.Message = ex.Message;
            }
            return msg;
        }
    }
}
