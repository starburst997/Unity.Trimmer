using System;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.Trimmer.Cli.Commands
{
    public static class Version
    {
        public static void Execute(bool json, bool showSystem)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assemblies = currentDomain.GetAssemblies();

            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var dict = new Dictionary<string, AssemblyName>();

            var info = assembly.GetName();
            if (info.Name != null) dict[info.Name] = info;
            
            GetAssemblies(assembly, dict, showSystem);

            if (json)
            {
                var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                Console.WriteLine($"{{\"success\": true, \"version\": \"{assemblyVersionAttribute?.InformationalVersion ?? assembly.GetName().Version.ToString()}\", \"versions\": [");
            }

            var first = true;
            foreach (var pair in dict)
            {
                var comma = first ? ' ' : ',';
                
                if (json)
                    Console.WriteLine($"{comma}{{\"name\": \"{pair.Key}\", \"version\": \"{pair.Value.Version}\"}}");
                else
                    Console.WriteLine($"{pair.Key} : {pair.Value.Version}");

                first = false;
            }
            
            if (json)
                Console.WriteLine("]}");
        }

        private static bool IsSystem(string name)
        {
            return name.StartsWith("Microsoft") ||
                   name.StartsWith("System") ||
                   name.StartsWith("netstandard") ||
                   name.StartsWith("mscorlib");
        }
        
        private static void GetAssemblies(Assembly assembly, Dictionary<string, AssemblyName> dict, bool allowSystem = false)
        {
            foreach (var child in assembly.GetReferencedAssemblies())
            {
                if (child.Name != null && (allowSystem || !IsSystem(child.Name)) && !dict.ContainsKey(child.Name))
                {
                    dict[child.Name] = child;

                    try
                    {
                        var childAssembly = Assembly.Load(child);
                        GetAssemblies(childAssembly, dict, allowSystem);
                    }
                    catch (Exception _)
                    {
                        // Nothing
                    }
                }
            }
        }
    }
}