using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// Allows customization of external rule actions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public sealed class ExternalActionAttribute : Attribute, IExternalAttribute
    {
        private Type t;

        private string a;

        private string n;

        private string m;

        /// <summary>
        /// Gets the type that declares the action
        /// </summary>
        public Type Type
        {
            get
            {
                return this.t;
            }
        }

        /// <summary>
        /// Gets the fully qualified name of the assembly that declares the action type
        /// </summary>
        public string Assembly
        {
            get
            {
                return this.a;
            }
        }

        /// <summary>
        /// Gets the full name of the type that declares the action
        /// </summary>
        public string TypeName
        {
            get
            {
                return this.n;
            }
        }

        /// <summary>
        /// Gets the declared name of the action
        /// </summary>
        public string Method
        {
            get
            {
                return this.m;
            }
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="type">Type that declares the action</param>
        /// <param name="methodName">Declared name of the action</param>
        public ExternalActionAttribute(Type type, string methodName)
        {
            this.t = type;
            this.m = methodName;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="assemblyName">Fully qualified name of the assembly that declares the action type</param>
        /// <param name="typeFullName">Full name of the type that declares the action</param>
        /// <param name="methodName">Declared name of the action</param>
        public ExternalActionAttribute(string assemblyName, string typeFullName, string methodName)
        {
            this.a = assemblyName;
            this.n = typeFullName;
            this.m = methodName;
        }
    }
}
