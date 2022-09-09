using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace WPF_CSharp_Compiler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string DOTNET = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319";

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                await BatchFileCreation();
                var result = GetEnvironmentVariableValue("Path");
                if (!result.Contains(DOTNET))
                    SetEnvironmentVariableValue("Path", result + ";" + DOTNET);
            }
            catch (Exception) { }
        }

        private async Task BatchFileCreation()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filename = "\\Main.bat";
            string content = "@echo off"
                + Environment.NewLine + "set csfile=" + path + "\\Main.cs"
                + Environment.NewLine + "csc /nologo /out:" + path + "\\Main.exe" + " %csfile%"
                + Environment.NewLine + path + "\\Main.exe";

            if (File.Exists(path + filename))
            {
                File.Delete(path + filename);
                await File.WriteAllTextAsync(path + filename, content);
            }
            else
            {
                await File.WriteAllTextAsync(path + filename, content);
            }
        }

        public string GetEnvironmentVariableValue(string name, bool ExpandVariables = true)
        {
            if (ExpandVariables)
            {
                return Environment.GetEnvironmentVariable(name);
            }
            else
            {
                return (string)Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment\").GetValue(name, "", Microsoft.Win32.RegistryValueOptions.DoNotExpandEnvironmentNames);
            }
        }

        public void SetEnvironmentVariableValue(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Machine);
        }
    }
}
