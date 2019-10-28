using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens.Opcenter.Exfn.UMC.DataModel
{
    public class Role
    {
        private string _Name;
        private string _Description;
        private bool _IsSystemRole;

        public string Name { get => _Name; set => _Name = value; }
        public string Description { get => _Description; set => _Description = value; }
        public bool IsSystemRole { get => _IsSystemRole; set => _IsSystemRole = value; }
    }
}
