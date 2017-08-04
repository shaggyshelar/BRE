using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    public interface ISettingsAttribute
    {
        long Max
        {
            get;
            set;
        }

        long Min
        {
            get;
            set;
        }

        string DateTimeFormat
        {
            get;
            set;
        }

        Type CollectionItemType
        {
            get;
            set;
        }

        string DataSourceName
        {
            get;
            set;
        }
    }
}
