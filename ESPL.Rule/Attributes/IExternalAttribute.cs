using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    public interface IExternalAttribute
    {
        Type Type
        {
            get;
        }

        string Assembly
        {
            get;
        }

        string TypeName
        {
            get;
        }

        string Method
        {
            get;
        }
    }
}
