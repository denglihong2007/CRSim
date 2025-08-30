using System.Reflection;
using System.Runtime.Loader;

namespace CRSim.Core.Utils
{
    internal class PluginLoadContext(string pluginPath) : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}
