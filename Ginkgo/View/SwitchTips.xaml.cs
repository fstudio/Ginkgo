using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginkgo.View
{
    /// <summary>
    /// SwitchTips.xaml 的交互逻辑
    /// </summary>
    public partial class SwitchTips : UserControl
    {
        private String Filename;
        public SwitchTips()
        {
            //Filename = "Template.bat";
            InitializeComponent();
        }
        public void SetFileName(String filename)
        {
            if(filename!=null&&filename.LastIndexOf("\\")>0)
            {
                FileContentTextbox.Text = filename.Substring(filename.LastIndexOf("\\")+1);
                Filename = filename;
            }
        }
    }
}
