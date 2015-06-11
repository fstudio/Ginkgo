using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.ComponentModel;
using System.Reflection;
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
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Ginkgo.View;
using Ginkgo.Modules;


namespace Ginkgo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private String currentFile
        {
            get;
            set;
        }
        private String OriginTitle = "Ginkgo";
        public MainWindow()
        {
            IHighlightingDefinition batchHighlighting;
            using (var s = Assembly.GetEntryAssembly().GetManifestResourceStream("Ginkgo.Syntax.Batch.xshd"))
            {
                if (s == null)
                {
                    throw new InvalidOperationException("Could not find embedded resource");
                }
                using (XmlReader reader = new XmlTextReader(s))
                {
                    batchHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                    HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting("Batch", new string[] { ".cmd", ".bat", ".nt" }, batchHighlighting);
            //textEditor.SyntaxHighlighting = batchHighlighting;
            InitializeComponent();
            ThemeManager.ChangeAppTheme(this, "BaseDark");
            //TextFileName.SetBinding(TextBox.TextProperty, new Binding("/Text") { Source = currentFile, Mode = BindingMode.OneWay });
        }

        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            View.AboutWindow about = new View.AboutWindow();
            about.ShowDialog();
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenPropertiesSettingWindow(object sender, RoutedEventArgs e)
        {
            View.PropertiesSetting vpropertiess = new View.PropertiesSetting();
            vpropertiess.Show();
        }

        private void OpenScriptFile(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".bat"; // Default file extension
            dlg.Filter = "Batch Script (*.bat;*.cmd;*.nt)|*.bat;*.cmd;*.nt|All Files(*.*)|*.*"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
               currentFile = dlg.FileName;
                using (StreamReader reader = File.OpenText(currentFile))
                {
                    this.textEditor.Text = reader.ReadToEnd();
                    this.Title =OriginTitle+ " - " + currentFile.Substring(currentFile.LastIndexOf("\\")+1);
                }

            }

        }

        private void SavaScriptFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".bat"; // Default file extension
            dlg.Filter = "Batch Script (*.bat;*.cmd;*.nt)|*.bat;*.cmd;*.nt|Other File|*.*"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
            }

        }
    }
}
