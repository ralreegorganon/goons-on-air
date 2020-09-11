using System.Threading.Tasks;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;
using GoonsOnAir.Services;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public class DownloadFboMissionsViewModel : OutputProcessBaseViewModel
    {
        public DownloadFboMissionsViewModel()
        {
            DisplayName = "Download FBO Missions";
        }

        public IFboService FboService { get; set; }

        protected override async Task DoTheWork()
        {
            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Downloading FBO missions..."
            });

            await FboService.DownloadFboMissions(OutputFolder);

            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Download FBO missions complete."
            });
        }
    }
}
