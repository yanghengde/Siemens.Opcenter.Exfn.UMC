using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens.Opcenter.Exfn.UMC.DataModel
{
    public class UmcUser
    {
        private IList<User> _Users;
        private IList<Role> _Roles;
        private IList<UserRole> _UserRoles;

        public IList<User> Users { get => _Users; set => _Users = value; }
        public IList<Role> Roles { get => _Roles; set => _Roles = value; }
        public IList<UserRole> UserRoles { get => _UserRoles; set => _UserRoles = value; }

    }
}
