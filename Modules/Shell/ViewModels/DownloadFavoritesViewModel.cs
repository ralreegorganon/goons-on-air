using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;
using GoonsOnAir.Services;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public class DownloadFavoritesViewModel : OutputProcessBaseViewModel
    {
        public DownloadFavoritesViewModel()
        {
            DisplayName = "Download Favorite Missions";
        }

        public IFboService FboService { get; set; }

        protected override async Task DoTheWork()
        {
            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Downloading Favorite missions..."
            });

            await FboService.DownloadFavoriteMissions(OutputFolder);

            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Download Favorite missions complete."
            });
        }
    }
}
