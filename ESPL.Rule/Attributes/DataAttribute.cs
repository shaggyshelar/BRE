using ESPL.Rule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPL.Rule.Attributes
{
    /// <summary>
    /// This attribute is used to define Dynamic Menu Data Sources.
    /// See Code Effects control online documentation for details.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public sealed class DataAttribute : Attribute
    {
        private Type t;

        private string a;

        private string n;

        private string m;

        private string z;

        internal FeatureLocation Location
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type that declares the data source method
        /// </summary>
        public Type Type
        {
            get
            {
                return this.t;
            }
        }

        /// <summary>
        /// Gets the assembly that declares the type with data source method
        /// </summary>
        public string Assembly
        {
            get
            {
                return this.a;
            }
        }

        /// <summary>
        /// Gets the name of the type that declares the data source method
        /// </summary>
        public string TypeName
        {
            get
            {
                return this.n;
            }
        }

        /// <summary>
        /// Gets the name of the data source method
        /// </summary>
        public string Method
        {
            get
            {
                return this.m;
            }
        }

        /// <summary>
        /// Gets the unique name of the data source that can be used to
        /// distinguish this method from other data source methods
        /// </summary>
        public string Name
        {
            get
            {
                return this.z;
            }
        }

        private DataAttribute(string name, FeatureLocation location, string methodName)
        {
            this.z = name;
            this.m = methodName;
            this.Location = location;
        }

        /// <summary>
        /// Use this constructor to define client-side data source functions.
        /// </summary>
        /// <param name="name">Unique name of the data source that can be used to
        /// distinguish this method from other data source methods</param>
        /// <param name="clientCallbackFunctionName">Client-side function name that is used as data source</param>
        public DataAttribute(string name, string clientCallbackFunctionName)
            : this(name, FeatureLocation.Client, clientCallbackFunctionName)
        {
        }

        /// <summary>
        /// Use this constructor to define server-side data source methods.
        /// </summary>
        /// <param name="name">Unique name of the data source that can be used to
        /// distinguish this method from other data source methods</param>
        /// <param name="type">Type that declares this data source method</param>
        /// <param name="methodName">Name of the data source method</param>
        public DataAttribute(string name, Type type, string methodName)
            : this(name, FeatureLocation.Server, methodName)
        {
            this.t = type;
        }

        /// <summary>
        /// Use this constructor to define server-side data source methods.
        /// </summary>
        /// <param name="name">Unique name of the data source that can be used to
        /// distinguish this method from other data source methods</param>
        /// <param name="assemblyName">Assembly that declares the type with data source method</param>
        /// <param name="typeFullName">Full name of the type that declares this data source method</param>
        /// <param name="methodName">Name of the data source method</param>
        public DataAttribute(string name, string assemblyName, string typeFullName, string methodName)
            : this(name, FeatureLocation.Server, methodName)
        {
            this.a = assemblyName;
            this.n = typeFullName;
        }
    }
}
