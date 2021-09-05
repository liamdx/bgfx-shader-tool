using ICSharpCode.AvalonEdit;
using System.Diagnostics;
using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Text;

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
            if(options.ShaderType == ShaderCompileType.Invalid)
            {
                PrintMessageToConsole("Invalid shader type.");
            }
            if(options.BuildWindows)
            {
                CompileWindows(options);
            }
        }

        private void CompileWindows(CompileOptions options)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string finalShadercDir = currentDir + BGFXShaderToolHelpers.SHADERC_PATH;
            string windowsDir = options.ShaderRootPath + "/windows";
            CheckIfDirExistsAndCreateIfNot(windowsDir);

            string shaderName = BGFXShaderToolHelpers.GetShaderNameFromFilePath(options.Path);

            foreach (string profile in VALID_WINDOWS_PROFILES)
            {
                StringBuilder argBuilder = new StringBuilder();
                argBuilder.Append($"-f {options.Path} ");
                string outputFileName = BGFXShaderToolHelpers.GetFinalOutputShaderPath(windowsDir, shaderName, profile, options.BuildEmbedded);
                
                if(options.BuildEmbedded)
                {
                    _trackedEmbeddedShaderPaths.Add(outputFileName);
                }

                argBuilder.Append($"-o {outputFileName} ");
                argBuilder.Append(BGFXShaderToolHelpers.GetTypeArgument(options.ShaderType) + " ");
                argBuilder.Append("--platform windows ");
                argBuilder.Append($"--profile {profile} ");
                if(options.BuildEmbedded)
                {
                    argBuilder.Append($"--bin2c {shaderName}_{profile}");
                }
                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = finalShadercDir;
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
                StringBuilder sb = new StringBuilder();

                foreach(string file in _trackedEmbeddedShaderPaths)
                {
                    if (File.Exists(file))
                    {
                        string fileText = File.ReadAllText(file);
                        if (!string.IsNullOrEmpty(fileText))
                        {
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

        private void CheckIfDirExistsAndCreateIfNot(string path)
        {
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
