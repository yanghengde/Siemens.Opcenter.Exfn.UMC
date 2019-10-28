using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens.Opcenter.Exfn.UMC.DataModel
{
    public class UserRole
    {
        private string _UserName;
        private string _RoleName;

        public string UserName { get => _UserName; set => _UserName = value; }
        public string RoleName { get => _RoleName; set => _RoleName = value; }
    }
}
