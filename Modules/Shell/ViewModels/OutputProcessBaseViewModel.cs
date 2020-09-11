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
    public abstract class OutputProcessBaseViewModel : Screen
    {
        public IEventAggregator EventAggregator { get; set; }

        public string OutputFolder { get; set; }

        public bool IsRunning { get; set; }

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
        public bool CanRun => !string.IsNullOrWhiteSpace(OutputFolder) && Directory.Exists(OutputFolder);

        public async Task Run()
        {
            IsRunning = true;

            try
            {
                await DoTheWork();
            }
            catch (Exception e)
            {
                Log.Error(e, "An unhandled exception occurred");
                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Error: {e}"
                });
                throw;
            }
            finally
            {
                IsRunning = false;
            }
        }

        protected abstract Task DoTheWork();
    }
}