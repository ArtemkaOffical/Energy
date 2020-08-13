using System;
using System.IO;
using Energy.Plugin;
using System.Collections.Generic;
using Energy.Extensions;
namespace Energy
{
    class Program
    {
        
        static void Main(string[] args)
        {
            PluginManager Manager = new PluginManager();
            Manager.Init();
            //foreach (var item in Manager.GetAllPlugins(new List<Plugin>()))
            //{
            //    Manager.PluginLoad(item.Name);
            //}

            //Manager.ReloadPlugin("Class1");
            Print("super");
            Print(123);

            //Manager.ReloadAllPlugins();
            Console.ReadLine();
        }


        public static  void Print(string text)
        {
            PluginManager.Call("Printed", new object[] {text});
            Console.WriteLine(text);
        }
        public static void Print(int text)
        {
            PluginManager.Call("Printedd", new object[] { text });
            Console.WriteLine(text);
        }
    }
}
