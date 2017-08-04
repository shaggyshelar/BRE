using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Common
{
    public class RuleEditorMVCException : RuleException
    {
        public enum ErrorIds
        {
            OnlyOneStyleManagerIsAllowed = 100,
            OnlyOneScriptManagerIsAllowed,
            RuleEditorIdIsNull
        }

        internal RuleEditorMVCException(RuleEditorMVCException.ErrorIds error, params string[] parameters)
            : base("i" + (int)error, parameters)
        {
        }
    }
}
