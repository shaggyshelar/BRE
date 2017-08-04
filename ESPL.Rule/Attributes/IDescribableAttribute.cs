using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    public interface IDescribableAttribute : IDisplayableAttribute
    {
        string Description
        {
            get;
        }
    }
}
