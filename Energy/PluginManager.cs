using Energy.Extensions;
using Energy.LoadPlugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Energy.Extensions.Additions;
using System.Threading;

namespace Energy
{
    class PluginManager
    {
        private string PluginPath = $"{Environment.CurrentDirectory}" + "\\Plugins\\";
        private BindingFlags Flags = BindingFlags.Instance| BindingFlags.GetProperty | BindingFlags.SetProperty| BindingFlags.GetField| BindingFlags.SetField| BindingFlags.NonPublic| BindingFlags.Public;
        private Compiler compiler = new Compiler();
        private List<string> FilesFromDirectory = new List<string>();
        private static Dictionary<string, Plugin> Plugins = new Dictionary<string, Plugin>();
        private static Dictionary<Plugin, List<string>> PluginHooks = new Dictionary<Plugin, List<string>>();
        public static Plugin GetPlugin(string name)
        {     
            Plugins.TryGetValue(name, out Plugin result);
            return result;
        }
        public void ReloadPlugin(string name) 
        {
           PluginUnload(name);
           AddPlugin(PluginPath + $"{name}.cs");
           PluginLoad(name);
        }
        public IEnumerable<Plugin> GetAllPlugins(List<Plugin> list)=> list = Plugins.Values.ToList();
        
        public void ReloadAllPlugins() 
        {
            foreach (var item in GetAllPlugins(new List<Plugin>()))
            {
                ReloadPlugin(item.Name);            
            }
        }
        public void Init()
        {
            
            if (!Directory.Exists($"{Environment.CurrentDirectory}" + "\\Plugins"))
            {
                Directory.CreateDirectory($"{Environment.CurrentDirectory}" + "\\Plugins");
            }
            GetFilesFromDirectory(PluginPath);
            foreach (var item in FilesFromDirectory)
            {
                AddPlugin(item);
                
            }
            foreach (var item in GetAllPlugins(new List<Plugin>()))
            {
                PluginHooks.Add(item,GetAllHooksFromThePlugin(item.Name));
            }
        }
        private List<string> GetAllHooksFromThePlugin(string name) 
        {
            List<string> hooks = new List<string>();
            for (int i = 0; i < GetPlugin(name).GetType().GetMethods(Flags).Length; i++)
            {
                hooks.Add(GetPlugin(name).GetType().GetMethods(Flags)[i].Name);                          
            }
            return hooks;
        }
        public void PluginUnload(string name)
        {
            var plugin = GetPlugin(name);
            if (plugin == null)
            {
                Console.WriteLine($"This plugin was not found");
                return;
            }
            RemovePlugin(PluginPath + $"{name}.cs");
            Console.WriteLine($"Plugin {plugin.Name} has been unloaded");
        }
        public static object Call(string Name, params object[] args)
        {
            Plugin plugin =  PluginHooks.FirstOrDefault(z => z.Value.Find(y => y == Name) == Name).Key;
            if (PluginHooks.Any((x) => x.Key == plugin))
            {
                return GetPlugin(plugin.Name).CallHook(Name, args);
            }
            else return false;
        }   
        public void PluginLoad(string name)
        {
            var plugin = GetPlugin(name);
            if (plugin == null)
            {
                Console.WriteLine($"This plugin was not found");
                return;
            }
            plugin.GetInfoPlugin();
            Console.WriteLine($"'{plugin.Name}' -> Plugin loaded {plugin.Title}. By {plugin.Author}: {plugin.Desc}. V {plugin.Version}");        
        }
        private void GetFilesFromDirectory(string Directory) 
        {
            foreach (var item in new DirectoryInfo(Directory).GetFiles("*"))
            {
                if (item.Extension == ".cs") 
                {
                    if(!FilesFromDirectory.Contains(item.FullName))
                        FilesFromDirectory.Add(item.FullName);                   
                }
            }
        }       
        private bool AddPlugin(string path) 
        {
            if (!Plugins.ContainsKey(path.GetFileNameFromPath()))
            {
                Plugins.Add(path.GetFileNameFromPath(), compiler.CompilePlugin(path));
                if (!FilesFromDirectory.Contains(path)) FilesFromDirectory.Add(path);
                return true;
            }
            else
            {
                Console.WriteLine($"This plugin({GetPlugin(path.GetFileNameFromPath()).Name}) has been added");
                return false;
            }
        }
        private bool RemovePlugin(string path)
        {
            if (Plugins.ContainsKey(path.GetFileNameFromPath()))
            {
                Plugins.Remove(path.GetFileNameFromPath());
                FilesFromDirectory.Remove(PluginPath + $"{ path.GetFileNameFromPath()}.cs");
                return true;
            }
            return false;
        }
        
    }
    
}
