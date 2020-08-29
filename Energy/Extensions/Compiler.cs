using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Energy.Extensions.Additions;


namespace Energy.Plugin
{
    class Compiler
    {
        private CSharpCodeProvider _codeProvider = new CSharpCodeProvider();
        private List<string> _neededFiles = new List<string>()
        {
            "Energy.Plugin.dll",
            "System.dll",
            "System.Xml.dll",
            "System.Data.dll",
            "System.Core.dll",
            "System.Xml.Linq.dll"
        };
        public Plugin CompilePlugin(string path) 
        {
            Plugin plugin = null;
            var param = new CompilerParameters
            {
               
                GenerateExecutable = false,
                IncludeDebugInformation = false,
                GenerateInMemory = true,
            };
            foreach (var item in _neededFiles) {param.ReferencedAssemblies.Add(item);}
            var DllFile = _codeProvider.CompileAssemblyFromFile(param, path);
            if (DllFile.Errors.HasErrors)
            {
                foreach (var error in DllFile.Errors) { Console.WriteLine(error); }
                return null;
            }
            else
            {
                var CompiledFile = DllFile.CompiledAssembly;
                Assembly assembly = CompiledFile;
                Type[] type = assembly.GetTypes();
                if (type[0].Name == type[0].Name.GetFileNameFromPath() /*Path.GetFileNameWithoutExtension(path)*/)
                {
                    Type FileClass = assembly.GetType($"{type[0].Namespace}.{Path.GetFileNameWithoutExtension(path)}");
                    object obj = FileClass.GetConstructor(new Type[0]).Invoke(new object[0]);
                    plugin = (Plugin)Activator.CreateInstance(obj.GetType());
                }
                else if (type[0].Name != Path.GetFileNameWithoutExtension(path))
                {
                    Console.WriteLine($"File name should be {type[0].Name}");
                    return null;
                }
            }
            return plugin;
        }
    }
}
