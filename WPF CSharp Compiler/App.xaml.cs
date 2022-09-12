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
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            await BatchFileCreation();
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
    }
}
