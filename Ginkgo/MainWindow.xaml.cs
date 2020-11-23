using System;
using System.Collections.Generic;
using System.Xml;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Ginkgo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static readonly string Prefix = "Ginkgo Batch Editor ";
        CompletionWindow completionWindow;
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
                CurrentFile = Args[1];
                TextEditorLoadFile();
            }
            textEditor.Document.PropertyChanged += GinkgoDocumentPropertyChanged;
            textEditor.TextArea.TextEntering += GinkgoTextEntering;
            textEditor.TextArea.TextEntered += GinkgoTextEntered;
            //ThemeManager.ChangeAppTheme(this, "BaseDark");
        }

        public String CurrentFile { get; set; }
        private void UpdateTitle()
        {
            if (CurrentFile != null)
            {
                if (textEditor.IsModified)
                {
                    Title = Prefix + " - " + Path.GetFileName(CurrentFile) + " *";
                    return;
                }
                Title = Prefix + " - " + Path.GetFileName(CurrentFile);
                return;
            }
        }

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
            lineCounts.Text = "Lines: " + textEditor.LineCount.ToString();
            fileSize.Text = "Length: " + textEditor.Document.TextLength.ToString();
            //textEditor.IsModified = textEditor.Document.UndoStack.IsOriginalFile;
            UpdateTitle();
            //UpdateTitle();
        }
        private bool SaveFile()
        {
            if (CurrentFile == null)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                {
                    DefaultExt = ".bat",
                    Filter = "Batch Script (*.bat;*.cmd;*.nt)|*.bat;*.cmd;*.nt|Other Files|*.*"
                };
                if (dlg.ShowDialog() != true)
                {
                    return false;
                }
                CurrentFile = dlg.FileName;
            }
            textEditor.Save(CurrentFile);
            UpdateTitle();
            return true;
        }
        private void TextEditorLoadFile()
        {
            textEditor.Load(CurrentFile);
            textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(CurrentFile));
            fileLanguage.Text = textEditor.SyntaxHighlighting.Name;
            fileEncoding.Text = textEditor.Encoding.HeaderName.ToUpper();
            fileSize.Text = "Length: " + textEditor.Text.Length.ToString();
            UpdateTitle();
        }
        private bool OpenFileWithWindow()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".bat", // Default file extension
                Filter = "Batch Script (*.bat;*.cmd;*.nt)|*.bat;*.cmd;*.nt|All Files(*.*)|*.*" // Filter files by extension
            };
            var result = dlg.ShowDialog();
            if (result == true)
            {
                CurrentFile = dlg.FileName;
                textEditor.Load(CurrentFile);
                textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(CurrentFile));
                fileLanguage.Text = textEditor.SyntaxHighlighting.Name;
                fileEncoding.Text = textEditor.Encoding.HeaderName.ToUpper();
                fileSize.Text = "Length: " + textEditor.Text.Length.ToString();
                UpdateTitle();
                return true;
            }
            return false;
        }
        private async void ShowAbout(object sender, RoutedEventArgs e)
        {
            await this.ShowMessageAsync("About Ginkgo", string.Format("Copyrigth \xA9 {0}, Force Charlie. All Rights Reserved.", DateTime.Now.Year), MessageDialogStyle.Affirmative);
        }
        private void OnExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private async void OnCloseWindow(object sender, CancelEventArgs e)
        {
            if (textEditor.IsModified)
            {
                await whenBatchFileModified();
            }
        }
        private async Task whenBatchFileModified()
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
                    break;
                default:
                    break;
            }
        }

        private void OpenPropertiesSettingWindow(object sender, RoutedEventArgs e)
        {
            View.PropertiesSetting vpropertiess = new View.PropertiesSetting();
            vpropertiess.Show();
        }

        private async void MenuOpenEventMethod(object sender, RoutedEventArgs e)
        {
            if (CurrentFile != null && textEditor.IsModified)
            {
                await whenBatchFileModified();
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
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".bat",
                Filter = "Batch Script (*.bat;*.cmd;*.nt)|*.bat;*.cmd;*.nt|Other File|*.*"
            };
            if (dlg.ShowDialog() != true)
            {
                return;
            }

            textEditor.Save(dlg.FileName);
            CurrentFile = dlg.FileName;
            Regex reg = new Regex("\\.(bat|cmd|nt)");
            if (reg.IsMatch(CurrentFile))
            {
                fileLanguage.Text = "Batch File";
            }
            else
            {
                var filename = System.IO.Path.GetFileName(CurrentFile);
                if (filename.LastIndexOf(".") > 0)
                    fileLanguage.Text = filename.Substring(filename.LastIndexOf(".") + 1).ToUpper() + " File";
                else
                    fileLanguage.Text = filename;
            }
            UpdateTitle();
        }
        private void MenuCloseEventMethod(object sender, RoutedEventArgs e)
        {
            if (textEditor.IsModified)
            {
                MenuSaveEventMethod(sender, e);
            }
            CurrentFile = null;
            textEditor.Clear();
            textEditor.IsModified = false;
            fileLanguage.Text = "";
            fileEncoding.Text = "";
            fileSize.Text = "";
            UpdateTitle();
        }
        private void OpenCurrentFolder(object sender, RoutedEventArgs e)
        {
            if (CurrentFile == null)
            {
                return;
            }
            var dir = System.IO.Path.GetDirectoryName(CurrentFile);
            if (dir != null)
            {
                System.Diagnostics.Process.Start("Explorer.exe", dir);
            }

        }

        private async void BatchFileRunAsyn(bool isKeep)
        {
            if (textEditor.Text.Length == 0)
                return;
            if (CurrentFile == null || textEditor.IsModified)
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
                System.Diagnostics.Process.Start("cmd.exe", "/k " + CurrentFile);
            }
            else
            {
                System.Diagnostics.Process.Start("cmd.exe", "/c " + CurrentFile);
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

        private void MenuNewFileEventMethod(object sender, RoutedEventArgs e)
        {
        }

        private void OnConsoleViewCheckChanged(object sender, RoutedEventArgs e)
        {
            if (ConsoleMenuView.IsChecked)
            {
            }
            else
            {

            }

        }
    }
}
