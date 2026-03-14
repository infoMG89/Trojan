using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;

namespace LineProperties;

public class PluginInit : IExtensionApplication
{
    public void Initialize()
    {
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    public void Terminate() { }

    private static Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        var asmName = new AssemblyName(args.Name);
        var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location) ?? "";
        var path = Path.Combine(pluginDir, asmName.Name + ".dll");
        return File.Exists(path) ? Assembly.LoadFrom(path) : null;
    }
}
