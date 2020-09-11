using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;
using PropertyChanged;
using Serilog;
using Screen = Caliburn.Micro.Screen;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public abstract class OutputProcessBaseViewModel : ProcessBaseViewModel
    {
        public string OutputFolder { get; set; }


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

        [DependsOn( nameof(OutputFolder))]
        public override bool CanRun => !string.IsNullOrWhiteSpace(OutputFolder) && Directory.Exists(OutputFolder);
    }
}