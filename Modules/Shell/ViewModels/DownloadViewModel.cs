using System.Threading.Tasks;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;
using GoonsOnAir.Services;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public class DownloadViewModel : OutputProcessBaseViewModel
    {
        public DownloadViewModel()
        {
            DisplayName = "Download";
        }

        public IFboService FboService { get; set; }

        public bool DownloadCashFlow { get; set; }
        public bool DownloadPendingMissions { get; set; }
        public bool DownloadFavoriteMissions { get; set; }
        public bool DownloadFboMissions { get; set; }
        public bool DownloadFboSummary { get; set; }


        protected override async Task DoTheWork()
        {
            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Downloading..."
            });

            if (DownloadCashFlow)
            {
                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Downloading cash flow..."
                });

                await FboService.DownloadCashFlow(OutputFolder);
            }

            if (DownloadPendingMissions)
            {
                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Downloading pending missions..."
                });

                await FboService.DownloadPendingMissions(OutputFolder);
            }

            if (DownloadFavoriteMissions)
            {
                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Downloading favorite missions..."
                });

                await FboService.DownloadFavoriteMissions(OutputFolder);
            }

            if (DownloadFboMissions)
            {
                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Downloading FBO missions..."
                });

                await FboService.DownloadFboMissions(OutputFolder);
            }

            if (DownloadFboSummary)
            {
                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Downloading FBO summary..."
                });

                await FboService.DownloadFboSummary(OutputFolder);
            }

            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Download complete."
            });
        }
    }
}
