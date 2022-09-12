using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using System.ComponentModel;

namespace WPF_CSharp_Compiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DOTNET = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319";
        private int OutLabelClick = 0;
        private BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PrimaryCode();
            worker.DoWork += worker_DoWork;
        }

        private void PrimaryCode()
        {
            Code.AppendText(Environment.NewLine + "using System;" + Environment.NewLine + Environment.NewLine + "public class HelloWorld");
            Code.AppendText(Environment.NewLine + "{" + Environment.NewLine + "    public static void Main(string[] args)");
            Code.AppendText(Environment.NewLine + "    {" + Environment.NewLine + "        Console.WriteLine (\"Hello World\");");
            Code.AppendText(Environment.NewLine + "    }" + Environment.NewLine + "}");
        }

        private void ClearCode()
        {
            Code.SelectAll();
            Code.Selection.Text = "";
        }

        private void ClearResult()
        {
            Result.SelectAll();
            Result.Selection.Text = "";
        }

        [Obsolete]
        private async void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _loader.Visibility = Visibility.Visible;
                await Content("Loading..");
                await Task.Delay(400);
                var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (File.Exists(path + "\\Main.cs"))
                    File.Delete(path + "\\Main.cs");
                if (File.Exists(path + "\\Main.exe"))
                    File.Delete(path + "\\Main.exe");
                string richText = new TextRange(Code.Document.ContentStart, Code.Document.ContentEnd).Text;
                await File.WriteAllTextAsync(path + "\\Main.cs", richText);
                await Content("Compiling...");
                var res = await Task.Run(() => RunningCommandPromot(path + "\\Main.bat", "", ""));
                ClearResult();
                RemoveFilePath(ref res, path);
                if (!res.Contains("error"))
                {
                    await Task.Delay(300);
                    await Content("Compiled");
                    await Task.Delay(300);
                }
                else
                {
                    await Task.Delay(300);
                    await Content("Compiled with Errors");
                    await Task.Delay(300);
                }
                Result.AppendText(res);
            }
            catch (Exception ex)
            { System.Windows.Forms.MessageBox.Show(ex.Message); }
            _loader.Visibility = Visibility.Collapsed;
        }

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearCode();
            PrimaryCode();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearResult();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = "";
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    path = fbd.SelectedPath + "\\Main.cs";
                    string richText = new TextRange(Code.Document.ContentStart, Code.Document.ContentEnd).Text;
                    File.WriteAllText(path, richText);
                    string argument = "/select, \"" + path + "\"";
                    Process.Start("explorer.exe", argument);
                }
            }
        }
        private string RunningCommandPromot(string filename, string args, string path)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = args,
                    WorkingDirectory = path,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            return output;
        }

        private new async Task Content(string cotent)
        {
            await this.Dispatcher.BeginInvoke((Action)(() =>
            {
                _loaderCtn.Content = cotent;
            }));
        }

        private void RemoveFilePath(ref string res, string path)
        {
            string newpath = path;
            newpath = "c" + newpath.Substring(1) + "\\";
            res = res.Replace(newpath, "");
        }

        private void OutPutLabel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OutLabelClick++;
            if (OutLabelClick == 6)
            {
                OutLabelClick = 0;
                SetEnvirable.Visibility = Visibility.Visible;
            }
        }

        private void EnvironmentVariable_Click(object sender, RoutedEventArgs e)
        {
            var result = (System.Windows.Controls.Button)sender;
            if (result.Content.ToString() == "Close")
            {
                SetEnvirable.Visibility = Visibility.Collapsed;
                SetEnvButtonArea.Visibility = Visibility.Visible;
                SetEnvLoadingArea.Visibility = Visibility.Collapsed;
            }
            else if (result.Content.ToString() == "Set Environment Variable")
            {
                SetEnvButtonArea.Visibility = Visibility.Collapsed;
                SetEnvLoadingArea.Visibility = Visibility.Visible;
                worker.RunWorkerAsync();
            }
        }

        private async void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            await this.Dispatcher.BeginInvoke((Action)(async () =>
           {
               _loaderCtn2.Content = "Loading...";
               for (int i = 0; i <= 100; i++)
               {
                   Percent.Content = i + " %";
                   Progress.Value = i;
                   await Task.Delay(70);

                   if (i > 20 && i < 45)
                       _loaderCtn2.Content = "Setting Environment Variable";
                   if (i > 45 && i < 70)
                       _loaderCtn2.Content = "Applying Environment Variable";
                   if (i == 77)
                   {
                       try
                       {
                           _loaderCtn2.Content = "Applied";
                           var result = GetEnvironmentVariableValue("Path");
                           if (!result.Contains(DOTNET))
                               SetEnvironmentVariableValue("Path", result + ";" + DOTNET);
                           else
                           {
                               _loaderCtn2.Content = "Variable Already Exist";
                               Progress.Value = 100;
                               Percent.Content = 100 + " %";
                               break;
                           }
                       }
                       catch (Exception)
                       {
                           _loaderCtn2.Content = "Setting Environment Variable Error Run as Admin and Continue";
                           break;
                       }
                   }
                   if (i == 100)
                       _loaderCtn2.Content = "Completed";
               }
           }));
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