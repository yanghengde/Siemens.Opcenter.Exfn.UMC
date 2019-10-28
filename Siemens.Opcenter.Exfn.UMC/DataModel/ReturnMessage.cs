using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens.Opcenter.Exfn.UMC
{
    public class ReturnMessage
    {
        private string _Message;
        private bool _Succeed;
        private object _Result;

        public string Message { get => _Message; set => _Message = value; }
        public bool Succeed { get => _Succeed; set => _Succeed = value; }
        public object Result { get => _Result; set => _Result = value; }
    }
}
