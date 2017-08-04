using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Asp
{
    /// <summary>
    /// Defines the type of the SaveRule public event
    /// of the CodeEffects.Rule.Asp.RuleEditor class.
    /// </summary>
    /// /// <param name="sender">Instance of the Asp.RuleEditor class</param>
    /// <param name="e">Event data</param>
    public delegate void SaveEventHandler(object sender, SaveEventArgs e);
}
