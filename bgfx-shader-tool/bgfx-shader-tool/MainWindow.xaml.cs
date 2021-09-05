using ICSharpCode.AvalonEdit;
using System.Diagnostics;
using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Text;
using static System.Windows.Forms.Design.AxImporter;

namespace bgfx_shader_tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _shaderFilePath;
        private string _shaderBinaryOutputPath;
        private string _shaderText;

        private List<string> _trackedEmbeddedShaderPaths = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PrintMessageToConsole(string message)
        {
            ConsoleOutput.Text += "\n" + message;
            ConsoleOutput.ScrollToEnd();
        }

        private ShaderCompileType GetShaderCompileType()
        {
            string text = ShaderTypeCombo.Text;

            if(text == "Vertex")
            {
                return ShaderCompileType.Vertex;
            }
            if(text == "Fragment")
            {
                return ShaderCompileType.Fragment; 
            }
            if(text == "Compute")
            {
                return ShaderCompileType.Compute;
            }
            return ShaderCompileType.Invalid;
        }

        private void ButtonCompileShader_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(_shaderFilePath))
            {
                PrintMessageToConsole("No shader file specified / invalid.");
                return;
            }
            CompileOptions c = new CompileOptions();
            c.BuildWindows = _generateWindows;
            c.BuildMac = _generateMac;
            c.BuildLinux = _generateLinux;
            c.BuildEmbedded = _generateEmbeddedShader;
            c.BuildIos = _generateIos;
            c.BuildAndroid = _generateAndroid;
            c.Path = _shaderFilePath;
            c.ShaderType = GetShaderCompileType();

            Compile(c);
        }

        private void ButtonLoadShaderTextFromPath_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(_shaderFilePath))
            {
                return;
            }

            _shaderText = File.ReadAllText(_shaderFilePath);
            ShaderTextEditor.Text = _shaderText;

        }

        private void ButtonBrowseShaderOutputPath_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    _shaderBinaryOutputPath = dialog.SelectedPath;
                    ShaderOutputText.Text = _shaderBinaryOutputPath;
                }
            }
        }

        private void ButtonBrowseShaderSourcePath_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    _shaderFilePath = dialog.FileName;
                    ShaderPathText.Text = _shaderFilePath;
                }
            }
        }

        private void Compile(CompileOptions options)
        {
            string shaderName = BGFXShaderToolHelpers.GetShaderNameFromFilePath(options.Path);
            string currentDir = Directory.GetCurrentDirectory();
            string shadercDir = currentDir + BGFXShaderToolHelpers.SHADERC_PATH;

            if (options.ShaderType == ShaderCompileType.Invalid)
            {
                PrintMessageToConsole("Invalid shader type.");
            }
            if(options.BuildWindows)
            {
                CompileWindows(options, shaderName, shadercDir, currentDir);
            }
            if(options.BuildMac)
            {
                CompileMac(options, shaderName, shadercDir, currentDir);
            }

            if(options.BuildLinux)
            {
                CompileLinux(options, shaderName, shadercDir, currentDir);
            }

            if (options.BuildAndroid)
            {
                CompileAndroid(options, shaderName, shadercDir, currentDir);
            }

            if (options.BuildIos)
            {
                CompileIos(options, shaderName, shadercDir, currentDir);
            }

            if (options.BuildEmbedded)
            {
                StringBuilder sb = new StringBuilder();

                foreach (string file in _trackedEmbeddedShaderPaths)
                {
                    if (File.Exists(file))
                    {
                        string fileText = File.ReadAllText(file);
                        if (!string.IsNullOrEmpty(fileText))
                        {
                            if(sb.ToString().Contains(fileText))
                            {
                                continue;
                            }
                            sb.AppendLine(fileText);
                            sb.AppendLine();
                        }
                        File.Delete(file);
                    }
                }
                string amalgamatedShader = sb.ToString();
                string amalgamatedShaderLocation = options.ShaderRootPath + "/" + shaderName + "_embedded.h";
                File.WriteAllText(amalgamatedShaderLocation, amalgamatedShader);
            }
        }
        private void CompilePlatform(CompileOptions options, string shadercDir, string shaderName, string platform, string platformDir, string currentDir)
        {
            foreach (string profile in BGFXShaderToolHelpers.GetPlatformProfiles(platform))
            {
                StringBuilder argBuilder = new StringBuilder();
                argBuilder.Append($"-f {options.Path} ");
                string outputFileName = BGFXShaderToolHelpers.GetFinalOutputShaderPath(platformDir, shaderName, profile, false);

                argBuilder.Append($"-o {outputFileName} ");
                argBuilder.Append(BGFXShaderToolHelpers.GetTypeArgument(options.ShaderType) + " ");
                argBuilder.Append($"--platform {platform} ");
                argBuilder.Append($"--profile {profile} ");
                //--varyingdef <file path>  Path to varying.def.sc file.
                argBuilder.Append($"--varyingdef {options.ShaderRootPath}/varying.def.sc");

                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = shadercDir;
                    p.StartInfo.CreateNoWindow = true;

                    p.StartInfo.Arguments = argBuilder.ToString();
                    p.StartInfo.RedirectStandardOutput = true;
                    p.Start();

                    ConsoleOutput.Text += p.StandardOutput.ReadToEnd();
                    ConsoleOutput.ScrollToEnd();
                    p.WaitForExit();
                }
            }

            if(options.BuildEmbedded)
            {
                foreach (string profile in BGFXShaderToolHelpers.GetPlatformProfiles(platform))
                {
                    StringBuilder argBuilder = new StringBuilder();
                    argBuilder.Append($"-f {options.Path} ");
                    string outputFileName = BGFXShaderToolHelpers.GetFinalOutputShaderPath(platformDir, shaderName, profile, options.BuildEmbedded);
                                        
                    _trackedEmbeddedShaderPaths.Add(outputFileName);
                    
                    argBuilder.Append($"-o {outputFileName} ");
                    argBuilder.Append(BGFXShaderToolHelpers.GetTypeArgument(options.ShaderType) + " ");
                    argBuilder.Append($"--platform {platform} ");
                    argBuilder.Append($"--profile {profile} ");
                    argBuilder.Append($"--bin2c {shaderName}_{profile}");
                    
                    using (Process p = new Process())
                    {
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = shadercDir;
                        p.StartInfo.CreateNoWindow = true;

                        p.StartInfo.Arguments = argBuilder.ToString();
                        p.StartInfo.RedirectStandardOutput = true;
                        p.Start();

                        ConsoleOutput.Text += p.StandardOutput.ReadToEnd();
                        ConsoleOutput.ScrollToEnd();
                        p.WaitForExit();
                    }
                }
            }
        }
        private void CompileWindows(CompileOptions options, string shaderName, string shadercDir, string currentDir)
        {           
            string windowsDir = options.ShaderRootPath + "/windows";
            CheckIfDirExistsAndCreateIfNot(windowsDir);
            CompilePlatform(options, shadercDir, shaderName, "windows", windowsDir, currentDir);        
        }

        private void CompileMac(CompileOptions options, string shaderName, string shadercDir, string currentDir)
        {
            string macDir = options.ShaderRootPath + "/mac";
            CheckIfDirExistsAndCreateIfNot(macDir);
            CompilePlatform(options, shadercDir, shaderName, "mac", macDir, currentDir);
        }

        private void CompileIos(CompileOptions options, string shaderName, string shadercDir, string currentDir)
        {
            string macDir = options.ShaderRootPath + "/ios";
            CheckIfDirExistsAndCreateIfNot(macDir);
            CompilePlatform(options, shadercDir, shaderName, "ios", macDir, currentDir);
        }

        private void CompileLinux(CompileOptions options, string shaderName, string shadercDir, string currentDir)
        {
            string linuxDir = options.ShaderRootPath + "/linux";
            CheckIfDirExistsAndCreateIfNot(linuxDir);
            CompilePlatform(options, shadercDir, shaderName, "linux", linuxDir, currentDir);
        }

        private void CompileAndroid(CompileOptions options, string shaderName, string shadercDir, string currentDir)
        {
            string linuxDir = options.ShaderRootPath + "/android";
            CheckIfDirExistsAndCreateIfNot(linuxDir);
            CompilePlatform(options, shadercDir, shaderName, "android", linuxDir, currentDir);
        }

        private void CheckIfDirExistsAndCreateIfNot(string path)
        {
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
