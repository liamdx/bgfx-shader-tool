using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bgfx_shader_tool
{
    public partial class MainWindow
    {
        private bool _generateEmbeddedShader
        {
            get
            {
                if (CreateEmbeddedShader.IsChecked.HasValue)
                {
                    return CreateEmbeddedShader.IsChecked.Value;
                }
                else
                {
                    return false;
                }

            }
        }

        private bool _generateWindows
        {
            get
            {
                if (BuildWindows.IsChecked.HasValue)
                {
                    return BuildWindows.IsChecked.Value;
                }
                else
                {
                    return false;
                }

            }
        }

        private bool _generateMac
        {
            get
            {
                if (BuildMac.IsChecked.HasValue)
                {
                    return BuildMac.IsChecked.Value;
                }
                else
                {
                    return false;
                }

            }
        }

        private bool _generateLinux
        {
            get
            {
                if (BuildLinux.IsChecked.HasValue)
                {
                    return BuildLinux.IsChecked.Value;
                }
                else
                {
                    return false;
                }

            }
        }
        
        public static string[] VALID_WINDOWS_PROFILES =
        {
            "s_3_0",
            "s_4_0",
            "s_5_0",
            "spirv",
            "410",
            "420",
            "430",
            "440",
        };

        public static string[] VALID_MAC_PROFILES =
        {
            "metal",
        };

        public static string[] VALID_LINUX_PROFILES =
        {
            "metal",
            "spirv",
            "330",
            "400",
            "410",
            "420",
            "430",
            "440",
            "100_es",
            "300_es",
            "310_es",
            "320_es"
        };

    }
}
