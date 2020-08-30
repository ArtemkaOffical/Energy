using Energy.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Energy.Extensions.Additions;
using System.Threading;

namespace Energy.Plugin
{
    public class PluginManager
    {
        private string _pluginPath = $"{Environment.CurrentDirectory}" + "\\Plugins\\";
        private BindingFlags _flags = BindingFlags.Instance| BindingFlags.GetProperty | BindingFlags.SetProperty| BindingFlags.GetField| BindingFlags.SetField| BindingFlags.NonPublic| BindingFlags.Public;
        private Compiler _compiler = new Compiler();
        private List<string> _filesFromDirectory = new List<string>();
        private static Dictionary<string, Plugin> _plugins = new Dictionary<string, Plugin>();
        private static Dictionary<Plugin, List<string>> _pluginHooks = new Dictionary<Plugin, List<string>>();
        public static Dictionary<Plugin, List<string>> _pluginCommands = new Dictionary<Plugin, List<string>>();
       
        private FileSystemWatcher _fsw;
        public static Plugin GetPlugin(string name)
        {     
            _plugins.TryGetValue(name, out Plugin result);
            return result;
        }
        public void ReloadPlugin(string name) 
        {
           PluginUnload(name);
           AddPlugin(_pluginPath + $"{name}.cs");
           PluginLoad(name);
        }
        public IEnumerable<Plugin> GetAllPlugins(List<Plugin> list)=> list = _plugins.Values.ToList();
        
        public void ReloadAllPlugins() 
        {
            foreach (var item in GetAllPlugins(new List<Plugin>()))
            {
                ReloadPlugin(item.Name);            
            }
        }
        public void Init()
        {
            _fsw = new FileSystemWatcher(_pluginPath,"*.cs");
            _fsw.EnableRaisingEvents = true;
            if (!Directory.Exists($"{Environment.CurrentDirectory}" + "\\Plugins"))
            {
                Directory.CreateDirectory($"{Environment.CurrentDirectory}" + "\\Plugins");
            }
            GetFilesFromDirectory(_pluginPath);
            foreach (var item in _filesFromDirectory)
            {
                AddPlugin(item);             
            }
        }
        private List<string> GetAllHooksFromThePlugin(string name) 
        {
            List<string> hooks = new List<string>();
            for (int i = 0; i < GetPlugin(name).GetType().GetMethods(_flags).Length; i++)
            {
                hooks.Add(GetPlugin(name).GetType().GetMethods(_flags)[i].Name);                          
            }
            return hooks;
        }
        private List<string> GetAllCommandsFromThePlugin(string name)
        {
            List<string> commands = new List<string>();
            for (int i = 0; i < GetPlugin(name).GetType().GetMethods().Length; i++)
            {
                for (int j = 0; j < GetPlugin(name).GetType().GetMethods()[i].GetCustomAttributes(false).Count(); j++)
                {
                    if(GetPlugin(name).GetType().GetMethods()[i].GetCustomAttributes(false)[j] is CustomAttributes.Command)
                    commands.Add((GetPlugin(name).GetType().GetMethods()[i].GetCustomAttributes(false)[j] as CustomAttributes.Command).ChatCommand);
                }
            }
            return commands;
        }
        public void PluginUnload(string name)
        {
            var plugin = GetPlugin(name);
            if (plugin == null)
            {
                Console.WriteLine($"This plugin was not found");
                return;
            }
            RemovePlugin(_pluginPath + $"{name}.cs");
            Console.WriteLine($"Plugin {plugin.Name} has been unloaded");
        }
        public static Plugin GetPluginCommand(string Command) 
        {
            return _pluginCommands.FirstOrDefault(z => z.Value.Find(y => y == Command.ToString()) == Command.ToString()).Key;
        }
        public static object Call(string Name,string command, params object[] args)
        {
            Plugin plugin = GetPluginCommand(command); 
        
            if (_pluginHooks.Any((x) => x.Key == plugin))
            {
                return GetPlugin(plugin.Name).CallHook(Name, args);
            }
            else return false;
        }
        public static object Call(string Name, params object[] args)
        {
            Plugin plugin = _pluginHooks.FirstOrDefault(z => z.Value.Find(y => y == Name) == Name).Key; ;

            if (_pluginHooks.Any((x) => x.Key == plugin))
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
            _fsw.Created += new FileSystemEventHandler( Fsw_Created);
            _fsw.Deleted += new FileSystemEventHandler( Fsw_Deleted);
            foreach (var item in new DirectoryInfo(Directory).GetFiles("*"))
            {
                if (item.Extension == ".cs") 
                {
                    if(!_filesFromDirectory.Contains(item.FullName))
                        _filesFromDirectory.Add(item.FullName);                   
                }
            }
        }

        private void Fsw_Deleted(object sender, FileSystemEventArgs e)
        {
            PluginUnload(e.FullPath.GetFileNameFromPath());
        }
        private void Fsw_Created(object sender, FileSystemEventArgs e)
        {
            AddPlugin(e.FullPath);
          
        }
        private bool AddPlugin(string path) 
        {
            
            if (!_plugins.ContainsKey(path.GetFileNameFromPath()))
            {
                _plugins.Add(path.GetFileNameFromPath(), _compiler.CompilePlugin(path));
                _pluginHooks.Add(GetPlugin(path.GetFileNameFromPath()), GetAllHooksFromThePlugin(path.GetFileNameFromPath()));
                _pluginCommands.Add(GetPlugin(path.GetFileNameFromPath()), GetAllCommandsFromThePlugin(path.GetFileNameFromPath()));
               
                if (!_filesFromDirectory.Contains(path)) _filesFromDirectory.Add(path);
                PluginLoad(path.GetFileNameFromPath());
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
            if (_plugins.ContainsKey(path.GetFileNameFromPath()))
            {
                _pluginHooks.Remove(GetPlugin(path.GetFileNameFromPath()));
                _pluginCommands.Remove(GetPlugin(path.GetFileNameFromPath()));
              
                _plugins.Remove(path.GetFileNameFromPath());
                _filesFromDirectory.Remove(_pluginPath + $"{ path.GetFileNameFromPath()}.cs");
                
                return true;
            }
            return false;
        }
    } 
}
