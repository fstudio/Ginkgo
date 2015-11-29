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
        private void UpdateTitle()
        {
            String title = "Ginkgo Batch Editor ";
            if (currentFile != null)
            {
                title += " - ";
                title += System.IO.Path.GetFileName(currentFile);
            }
            if (editorModify)
            {
                title += " *";
            }
            this.Title = title;
        }
        private bool editorModify;
        private bool loadFile;
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
            HighlightingManager.Instance.RegisterHighlighting("Batch", new string[] { ".cmd", ".bat", ".nt" }, batchHighlighting);
            InitializeComponent();
            ThemeManager.ChangeAppTheme(this, "BaseDark");
        }
        private static void WriteFile(String path, String contents)
        {
            StreamWriter FileWriter = null;
            try
            {
                FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                FileWriter = new StreamWriter(fileStream);
                FileWriter.Write(contents);
            }
            finally
            {
                if (FileWriter != null)
                {
                    FileWriter.Close();
                }
            }
        }
        private bool SaveFile()
        {
            if (currentFile == null)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.DefaultExt = ".bat";
                dlg.Filter = "Batch Script (*.bat;*.cmd;*.nt)|*.bat;*.cmd;*.nt|Other File|*.*";
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    currentFile = dlg.FileName;
                }
                else
                {
                    return false;
                }
            }
            WriteFile(currentFile, this.textEditor.Text);
            editorModify = false;
            UpdateTitle();
            return true;
        }
        private bool OpenFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".bat"; // Default file extension
            dlg.Filter = "Batch Script (*.bat;*.cmd;*.nt)|*.bat;*.cmd;*.nt|All Files(*.*)|*.*"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                currentFile = dlg.FileName;
                using (StreamReader reader = File.OpenText(currentFile))
                {
                    this.textEditor.Text = reader.ReadToEnd();
                    editorModify = false;
                    loadFile = true;
                    UpdateTitle();
                }
                return true;
            }
            return false;
        }
        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            View.AboutWindow about = new View.AboutWindow();
            about.ShowDialog();
        }
        private void OnExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void OnCloseWindow(object sender, CancelEventArgs e)
        {
            if (editorModify)
            {
                var v = MessageBox.Show("Do you want to save this file ?",
                    "Batch File is modify !",
                    MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (v == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
            }
        }
        private void OpenPropertiesSettingWindow(object sender, RoutedEventArgs e)
        {
            View.PropertiesSetting vpropertiess = new View.PropertiesSetting();
            vpropertiess.Show();
        }

        private async void BatchFileIsModifyShow(object sender, RoutedEventArgs e)
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Save",
                NegativeButtonText = "Discard",
                FirstAuxiliaryButtonText = "Cancel",
                ColorScheme = MetroDialogOptions.ColorScheme
            };
            MessageDialogResult result = await this.ShowMessageAsync("Batch File is modify",
                "Do your want to save file ,or Discard modify,or cancel",
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, mySettings);
            switch (result)
            {
                case MessageDialogResult.Affirmative:
                    SaveFile();
                    OpenFile();
                    break;
                case MessageDialogResult.Negative:
                    OpenFile();
                    break;
                case MessageDialogResult.FirstAuxiliary:
                    return;
                default:
                    break;
            }
        }
        private void MenuOpenEventMethod(object sender, RoutedEventArgs e)
        {
            if (currentFile != null && editorModify)
            {
                BatchFileIsModifyShow(sender, e);
                return;
            }
            OpenFile();
        }
        private void MenuSaveEventMethod(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void MenuSaveAsEventMethod(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".bat";
            dlg.Filter = "Batch Script (*.bat;*.cmd;*.nt)|*.bat;*.cmd;*.nt|Other File|*.*";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                WriteFile(dlg.FileName, this.textEditor.Text);
                currentFile = dlg.FileName;
                editorModify = false;
                UpdateTitle();
            }
            else
            {
                return;
            }
        }
        private void MenuCloseEventMethod(object sender, RoutedEventArgs e)
        {
            if (editorModify)
            {
                MenuSaveEventMethod(sender, e);
            }
            currentFile = null;
            this.textEditor.Text = "";
            UpdateTitle();
        }
        private void OpenCurrentFolder(object sender, RoutedEventArgs e)
        {
            var dir = System.IO.Path.GetDirectoryName(currentFile);
            if (dir != null)
                System.Diagnostics.Process.Start("Explorer.exe", dir);
        }

        private void EditorTextChanged(object sender, EventArgs e)
        {
            if (loadFile)
            {
                loadFile = false;
                return;
            }
            if (!editorModify)
            {
                editorModify = true;
                UpdateTitle();
            }
        }
        private async void BatchFileRunAsyn(bool isKeep)
        {
            if (textEditor.Text.Length == 0)
                return;
            if (currentFile == null || editorModify)
            {
                var mySettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Save and Run",
                    NegativeButtonText = "Run Older",
                    FirstAuxiliaryButtonText = "Cancel",
                    ColorScheme = MetroDialogOptions.ColorScheme
                };
                MessageDialogResult result = await this.ShowMessageAsync("Batch File is modify",
                    "Please select your run mode",
                    MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, mySettings);
                switch (result)
                {
                    case MessageDialogResult.Affirmative:
                        SaveFile();
                        break;
                    case MessageDialogResult.Negative:
                        break;
                    case MessageDialogResult.FirstAuxiliary:
                        return;
                    default:
                        break;
                }
            }
            if (isKeep)
            {
                System.Diagnostics.Process.Start("cmd.exe", "/k " + currentFile);
            }
            else
            {
                System.Diagnostics.Process.Start("cmd.exe", "/c " + currentFile);
            }
           
        }
        private void MenuRunEventMethod(object sender, RoutedEventArgs e)
        {
            BatchFileRunAsyn(true);
        }
        private void MenuRunStopEventMethod(object sender, RoutedEventArgs e)
        {
            BatchFileRunAsyn(false);
        }
        private void MenuDebuggerEventMethod(object sender, RoutedEventArgs e)
        {
            ////
        }

        private void MenuGithubViewEventMethod(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/fstudio/Ginkgo");
        }


    }
}
