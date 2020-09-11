using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliburn.Micro;
using PropertyChanged;
using Screen = Caliburn.Micro.Screen;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public abstract class InputOutputProcessBaseViewModel : ProcessBaseViewModel
    {
        public string InputFolder { get; set; }

        public string OutputFolder { get; set; }

        public void BrowseInput()
        {
            using var dialog = new FolderBrowserDialog {
                RootFolder = Environment.SpecialFolder.Desktop,
                SelectedPath = InputFolder
            };

            if (DialogResult.OK == dialog.ShowDialog())
            {
                InputFolder = dialog.SelectedPath;
            }
        }

        public void BrowseOutput()
        {
            using var dialog = new FolderBrowserDialog {
                RootFolder = Environment.SpecialFolder.Desktop,
                SelectedPath = OutputFolder
            };

            if (DialogResult.OK == dialog.ShowDialog())
            {
                OutputFolder = dialog.SelectedPath;
            }
        }

        [DependsOn(nameof(InputFolder), nameof(OutputFolder))]
        public override bool CanRun => !string.IsNullOrWhiteSpace(OutputFolder) && !string.IsNullOrWhiteSpace(InputFolder) && Directory.Exists(OutputFolder) && Directory.Exists(InputFolder);
    }
}
