using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule
{
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
    internal class Resource
    {
        private static ResourceManager resourceMan;

        private static CultureInfo resourceCulture;

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(Resource.resourceMan, null))
                {
                    ResourceManager resourceManager = new ResourceManager("CodeEffects.Rule.Resource", typeof(Resource).Assembly);
                    Resource.resourceMan = resourceManager;
                }
                return Resource.resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return Resource.resourceCulture;
            }
            set
            {
                Resource.resourceCulture = value;
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version="1.0" encoding="utf-8" ?&gt;
        ///             &lt;codeeffects&gt;
        ///             	&lt;values&gt;
        ///             		&lt;s100&gt;In order to load source data, the following must be true: a.) The property "SourceXml" must contain valid source XML, or b.) The "SourceXmlFile" must contain the virtual path to an existing source XML file, or c.) The properties "SourceType" and "SourceAssembly" must contain full names of the assembly and type of your source object.&lt;/s100&gt;
        ///             		&lt;s101&gt;Assembly or type names cannot be null references or empty strings.&lt;/s101&gt;
        ///             		&lt;s102&gt;The [rest of string was truncated]";.
        /// </summary>
        internal static string Errors
        {
            get
            {
                return Resource.ResourceManager.GetString("Errors", Resource.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version="1.0" encoding="utf-8" ?&gt;
        ///             &lt;codeeffects&gt;
        ///             	&lt;help&gt;
        ///             		&lt;i101&gt;Click inside of the Filter Area to begin a new filter&lt;/i101&gt;
        ///             		&lt;i102&gt;Click anywhere inside of the Filter Area to modify the filter&lt;/i102&gt;
        ///             		&lt;i103&gt;Select a field or parenthesis from the menu; hit Space Bar if it's hidden&lt;/i103&gt;
        ///             		&lt;i104&gt;Select an operator from the menu; hit Space Bar if it's hidden&lt;/i104&gt;
        ///             		&lt;i105&gt;Type the value; use Backspace to delete, Enter or Right Arrow to complete&lt;/i105&gt;
        ///             		&lt;i106&gt;Select calculation elements from [rest of string was truncated]";.
        /// </summary>
        internal static string FilterHelp
        {
            get
            {
                return Resource.ResourceManager.GetString("FilterHelp", Resource.resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version="1.0" encoding="utf-8" ?&gt;
        ///             &lt;codeeffects&gt;
        ///             	&lt;help&gt;
        ///             		&lt;i101&gt;Click inside of the Rule Area to begin a new rule&lt;/i101&gt;
        ///             		&lt;i102&gt;Click anywhere inside of the Rule Area to modify the rule&lt;/i102&gt;
        ///             		&lt;i103&gt;Select a field or parenthesis from the menu; hit Space Bar if it's hidden&lt;/i103&gt;
        ///             		&lt;i104&gt;Select an operator from the menu; hit Space Bar if it's hidden&lt;/i104&gt;
        ///             		&lt;i105&gt;Type the value; use Backspace to delete, Enter or Right Arrow to complete&lt;/i105&gt;
        ///             		&lt;i106&gt;Select calculation elements from the men [rest of string was truncated]";.
        /// </summary>
        internal static string RuleHelp
        {
            get
            {
                return Resource.ResourceManager.GetString("RuleHelp", Resource.resourceCulture);
            }
        }

        internal Resource()
        {
        }
    }
}
