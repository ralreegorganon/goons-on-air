using System.Threading.Tasks;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;
using GoonsOnAir.Services;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public class DownloadPendingViewModel : OutputProcessBaseViewModel
    {
        public DownloadPendingViewModel()
        {
            DisplayName = "Download Pending Missions";
        }

        public IFboService FboService { get; set; }

        protected override async Task DoTheWork()
        {
            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Downloading Pending missions..."
            });

            await FboService.DownloadPendingMissions(OutputFolder);

            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Download Pending missions complete."
            });
        }
    }
}
