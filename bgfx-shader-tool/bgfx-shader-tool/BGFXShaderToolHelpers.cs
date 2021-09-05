using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace bgfx_shader_tool
{
    public enum ShaderCompileType
    {
        Invalid,
        Vertex,
        Fragment,
        Compute
    }
    public struct CompileOptions
    {
        public bool BuildWindows;
        public bool BuildMac;
        public bool BuildLinux;
        public bool BuildEmbedded;
        public ShaderCompileType ShaderType;
        public string Path
        {
            get { return _path; }    
            set
            {  
                _path = value;
                ShaderRootPath = Directory.GetParent(_path).FullName;
            }
        }
        public string ShaderRootPath;
        private string _path;
    }

    public class BGFXShaderToolHelpers
    {
        public const string SHADERC_PATH = "/res/shaderc.exe";

        public static string GetFinalOutputShaderPath(string platformShaderPath, string shaderName, string profile, bool embedded)
        {
            string extension = "";
            if(embedded)
            {
                extension = ".h";
            }
            else
            {
                extension = ".bin";
            }

            return platformShaderPath + "/" + shaderName + $"_{profile}" + extension;
        }
        
        public static string GetTypeArgument(ShaderCompileType shaderType)
        {
            switch(shaderType)
            {
                case ShaderCompileType.Vertex:
                    return "--type v";
                    break;
                case ShaderCompileType.Fragment:
                    return "--type f";
                    break;
                case ShaderCompileType.Compute:
                    return "--type c";
                    break;
                case ShaderCompileType.Invalid:
                    return "";
                    break;
            }
            return "";
        }

        public static string GetShaderNameFromFilePath(string path)
        {
            string ret = "";

            if(path.Contains(".sc"))
            {
                ret = path.Replace(".sc", "");
            }

            int lastBackslash = -1;
            int lastForwardslash = -1;

            lastForwardslash = path.LastIndexOf("/");
            lastBackslash = path.LastIndexOf("\\");

            if(lastForwardslash == -1 && lastBackslash == -1)
            {
                return "";
            }

            if(lastBackslash > lastForwardslash)
            {
                return ret.Substring(lastBackslash + 1);
            }
            else
            {
                return ret.Substring(lastForwardslash + 1);
            }
        }
    }

    


}
