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
        
        

    }
}
