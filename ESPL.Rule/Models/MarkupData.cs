using ESPL.Rule.Client;
using ESPL.Rule.Common;
using ESPL.Rule.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Models
{
    internal class MarkupData
    {
        public string ControlServerID
        {
            get;
            set;
        }

        public string PostBackFunctionName
        {
            get;
            set;
        }

        public ActionUrls MvcActions
        {
            get;
            set;
        }

        public ThemeManager ThemeFactory
        {
            get;
            set;
        }

        public Settings Settings
        {
            get;
            set;
        }

        public Pattern Pattern
        {
            get;
            set;
        }

        public bool IsLoadedRuleOfEvalType
        {
            get;
            set;
        }

        public RuleType Mode
        {
            get;
            set;
        }

        public MarkupData()
        {
            this.IsLoadedRuleOfEvalType = true;
            this.Mode = RuleType.Evaluation;
            this.Pattern = Pattern.None;
        }
    }
}
