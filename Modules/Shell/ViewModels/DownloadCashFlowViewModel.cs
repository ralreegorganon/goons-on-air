using System.Threading.Tasks;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;
using GoonsOnAir.Services;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public class DownloadCashFlowViewModel : OutputProcessBaseViewModel
    {
        public DownloadCashFlowViewModel()
        {
            DisplayName = "Download Cash Flow";
        }

        public IFboService FboService { get; set; }

        protected override async Task DoTheWork()
        {
            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Downloading Cash Flow..."
            });

            await FboService.DownloadCashFlow(OutputFolder);

            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Download Cash Flow complete."
            });
        }
    }
}
