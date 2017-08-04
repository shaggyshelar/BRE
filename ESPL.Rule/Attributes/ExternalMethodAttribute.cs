using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// Allows customization of external in-rule method
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public sealed class ExternalMethodAttribute : Attribute, IExternalAttribute
    {
        private Type t;

        private string a;

        private string n;

        private string m;

        private bool g;

        /// <summary>
        /// Gets the type that declares the method
        /// </summary>
        public Type Type
        {
            get
            {
                return this.t;
            }
        }

        /// <summary>
        /// Gets the fully qualified name of the assembly that declares the method type
        /// </summary>
        public string Assembly
        {
            get
            {
                return this.a;
            }
        }

        /// <summary>
        /// Gets the full name of the type that declares the method
        /// </summary>
        public string TypeName
        {
            get
            {
                return this.n;
            }
        }

        /// <summary>
        /// Gets the declared name of the method
        /// </summary>
        public string Method
        {
            get
            {
                return this.m;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the value of this
        /// external method appears in UI context menus that list available rule
        /// fields and in-rule methods for rule conditions.
        /// The default value is True
        /// </summary>
        public bool Gettable
        {
            get
            {
                return this.g;
            }
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="type">Type that declares the method</param>
        /// <param name="methodName">Declared name of the method</param>
        public ExternalMethodAttribute(Type type, string methodName)
        {
            this.t = type;
            this.m = methodName;
            this.g = true;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        public ExternalMethodAttribute(Type type, string methodName, bool gettable)
            : this(type, methodName)
        {
            this.g = gettable;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        /// <param name="assemblyName">Fully qualified name of the assembly that declares the method type</param>
        /// <param name="typeFullName">Full name of the type that declares the method</param>
        /// <param name="methodName">Declared name of the method</param>
        public ExternalMethodAttribute(string assemblyName, string typeFullName, string methodName)
        {
            this.a = assemblyName;
            this.n = typeFullName;
            this.m = methodName;
            this.g = true;
        }

        /// <summary>
        /// Public c-tor
        /// </summary>
        public ExternalMethodAttribute(string assemblyName, string typeFullName, string methodName, bool gettable)
            : this(assemblyName, typeFullName, methodName)
        {
            this.g = gettable;
        }
    }
}
