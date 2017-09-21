using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Wangsu.WcsLib.Utility
{
    // UMU: https://www.codeproject.com/tips/353819/get-all-assembly-information
    public class AssemblyInfo
    {
        public AssemblyInfo(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            this.assembly = assembly;
        }

        /// <summary>
        /// Gets the title property
        /// </summary>
        public string Title
        {
            get
            {
                // UMU: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-operator
                return GetAttributeValue<AssemblyTitleAttribute>(a => a.Title, Path.GetFileNameWithoutExtension(assembly.CodeBase));
            }
        }

        /// <summary>
        /// Gets the application's version
        /// </summary>
        public string Version
        {
            get
            {
                Version version = assembly.GetName().Version;
                if (version != null)
                {
                    return version.ToString();
                }
                else
                {
                    return "1.0.0.0";
                }
            }
        }

        /// <summary>
        /// Gets the description about the application.
        /// </summary>
        public string Description
        {
            get { return GetAttributeValue<AssemblyDescriptionAttribute>(a => a.Description); }
        }

        /// <summary>
        ///  Gets the product's full name.
        /// </summary>
        public string Product
        {
            get { return GetAttributeValue<AssemblyProductAttribute>(a => a.Product); }
        }

        /// <summary>
        /// Gets the copyright information for the product.
        /// </summary>
        public string Copyright
        {
            get { return GetAttributeValue<AssemblyCopyrightAttribute>(a => a.Copyright); }
        }

        /// <summary>
        /// Gets the company information for the product.
        /// </summary>
        public string Company
        {
            get { return GetAttributeValue<AssemblyCompanyAttribute>(a => a.Company); }
        }

        protected string GetAttributeValue<T>(Func<T, string> resolveFunc, string defaultResult = null) where T : Attribute
        {
            object[] attributes = assembly.GetCustomAttributes(typeof(T), false);
            if (attributes.Length > 0)
            {
                return resolveFunc((T)attributes[0]);
            }
            else
            {
                return defaultResult;
            }
        }

        private readonly Assembly assembly;
    }
}
