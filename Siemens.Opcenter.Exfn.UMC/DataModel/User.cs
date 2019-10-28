using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens.Opcenter.Exfn.UMC.DataModel
{
    public class User
    {
        private string _FullName;
        private string _Id;
        private string _Name;

        public string FullName { get => _FullName; set => _FullName = value; }
        public string Id { get => _Id; set => _Id = value; }
        public string Name { get => _Name; set => _Name = value; }
    }
}
