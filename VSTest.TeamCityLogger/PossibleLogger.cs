using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace VSTest.TeamCityLogger
{
    class PossibleLogger
    {
        public Assembly Assembly { get; private set; }
        public Type TypeInAssembly { get; private set; }
        public FriendlyNameAttribute FriendlyName { get; private set; }
        public ExtensionUriAttribute ExtensionUri { get; private set; }

        public PossibleLogger(Assembly assembly, Type typeInAssembly)
        {
            Assembly = assembly;
            TypeInAssembly = typeInAssembly;

            ExtensionUri = typeInAssembly.GetCustomAttributes(typeof(ExtensionUriAttribute), false)
                            .OfType<ExtensionUriAttribute>()
                            .FirstOrDefault();
            FriendlyName = typeInAssembly.GetCustomAttributes(typeof(FriendlyNameAttribute), false)
                            .OfType<FriendlyNameAttribute>()
                            .FirstOrDefault();
        }
    }
}