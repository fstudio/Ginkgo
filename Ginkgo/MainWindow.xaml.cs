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
using System.Text.RegularExpressions;


namespace Ginkgo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
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
            HighlightingManager.Instance.RegisterHighlighting("Batch", new string[] { ".cmd", ".bat", ".nt", ".btm" }, batchHighlighting);
            var Args = Environment.GetCommandLineArgs();
            if (Args.Length > 1)
            {
                currentFile = Args[1];
                TextEditorLoadFile();
            }
            this.textEditor.Document.PropertyChanged += GinkgoDocumentPropertyChanged;
            this.textEditor.TextArea.TextEntering += GinkgoTextEntering;
            this.textEditor.TextArea.TextEntered += GinkgoTextEntered;
            //ThemeManager.ChangeAppTheme(this, "BaseDark");
        }
        private String currentFile;

        public String CurrentFile
        {
            get { return currentFile; }
            set { currentFile = value; }
        }
        private void UpdateTitle()
        {
            String title = "Ginkgo Batch Editor ";
            if (currentFile != null)
            {
                title += " - ";
                title += System.IO.Path.GetFileName(currentFile);
            }
            if (this.textEditor.IsModified)
            {
                title += " *";
            }
            this.Title = title;
        }
        CompletionWindow completionWindow;
        private void GinkgoTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                // open code completion after the user has pressed dot:
                completionWindow = new CompletionWindow(textEditor.TextArea);
                // provide AvalonEdit with the data:

                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new GinkgoCompletionData("set"));
                data.Add(new GinkgoCompletionData("Item2"));
                data.Add(new GinkgoCompletionData("Item3"));
                data.Add(new GinkgoCompletionData("Another item"));
                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
        }

        private void GinkgoTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void GinkgoDocumentPropertyChanged(object sender, EventArgs e)
        {
            this.lineCounts.Text = "Lines: " + this.textEditor.LineCount.ToString();
            this.fileSize.Text = "Length: " + this.textEditor.Document.TextLength.ToString();
            //this.textEditor.IsModified = this.textEditor.Document.UndoStack.IsOriginalFile;
            UpdateTitle();
            //UpdateTitle();
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
            this.textEditor.Save(currentFile);
            UpdateTitle();
            return true;
        }
        private void TextEditorLoadFile()
        {
            this.textEditor.Load(currentFile);
            this.textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(currentFile));
            this.fileLanguage.Text = this.textEditor.SyntaxHighlighting.Name;
            this.fileEncoding.Text = this.textEditor.Encoding.HeaderName.ToUpper();
            this.fileSize.Text = "Length: " + this.textEditor.Text.Length.ToString();
            UpdateTitle();
        }
        private bool OpenFileWithWindow()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".bat"; // Default file extension
            dlg.Filter = "Batch Script (*.bat;*.cmd;*.nt)|*.bat;*.cmd;*.nt|All Files(*.*)|*.*"; // Filter files by extension
            var result = dlg.ShowDialog();
            if (result == true)
            {
                currentFile = dlg.FileName;
                this.textEditor.Load(currentFile);
                this.textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(currentFile));
                this.fileLanguage.Text = this.textEditor.SyntaxHighlighting.Name;
                this.fileEncoding.Text = this.textEditor.Encoding.HeaderName.ToUpper();
                this.fileSize.Text = "Length: " + this.textEditor.Text.Length.ToString();
                UpdateTitle();
                return true;
            }
            return false;
        }
        private void ShowAbout(object sender, RoutedEventArgs e)
        {
           this.ShowMessageAsync("About Ginkgo",
                "Copyrigth \xA9 2016, Force Charlie. All Rights Reserved.",
                MessageDialogStyle.Affirmative);
            //View.AboutWindow about = new View.AboutWindow();
            //about.ShowDialog();
        }
        private void OnExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void OnCloseWindow(object sender, CancelEventArgs e)
        {
            if (this.textEditor.IsModified)
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
                    OpenFileWithWindow();
                    break;
                case MessageDialogResult.Negative:
                    OpenFileWithWindow();
                    break;
                case MessageDialogResult.FirstAuxiliary:
                    return;
                default:
                    break;
            }
        }
        private void MenuOpenEventMethod(object sender, RoutedEventArgs e)
        {
            if (currentFile != null && this.textEditor.IsModified)
            {
                BatchFileIsModifyShow(sender, e);
                return;
            }
            OpenFileWithWindow();
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
            var result = dlg.ShowDialog();
            if (result == true)
            {
                this.textEditor.Save(dlg.FileName);
                currentFile = dlg.FileName;
                Regex reg = new Regex("\\.(bat|cmd|nt)");
                if (reg.IsMatch(currentFile))
                {
                    this.fileLanguage.Text = "Batch File";
                }
                else
                {
                    var filename = System.IO.Path.GetFileName(currentFile);
                    if (filename.LastIndexOf(".") > 0)
                        this.fileLanguage.Text = filename.Substring(filename.LastIndexOf(".") + 1).ToUpper() + " File";
                    else
                        this.fileLanguage.Text = filename;
                }
                UpdateTitle();
            }
            else
            {
                return;
            }
        }
        private void MenuCloseEventMethod(object sender, RoutedEventArgs e)
        {
            if (this.textEditor.IsModified)
            {
                MenuSaveEventMethod(sender, e);
            }
            currentFile = null;
            this.textEditor.Clear();
            this.textEditor.IsModified = false;
            this.fileLanguage.Text = "";
            this.fileEncoding.Text = "";
            this.fileSize.Text = "";
            UpdateTitle();
        }
        private void OpenCurrentFolder(object sender, RoutedEventArgs e)
        {
            var dir = System.IO.Path.GetDirectoryName(currentFile);
            if (dir != null)
                System.Diagnostics.Process.Start("Explorer.exe", dir);
        }

        private async void BatchFileRunAsyn(bool isKeep)
        {
            if (textEditor.Text.Length == 0)
                return;
            if (currentFile == null || this.textEditor.IsModified)
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

        private void UpdateTextChanged(object sender, EventArgs e)
        {
            UpdateTitle();
        }


    }
}
