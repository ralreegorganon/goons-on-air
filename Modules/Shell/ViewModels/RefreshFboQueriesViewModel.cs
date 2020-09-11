using System.Threading.Tasks;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;
using GoonsOnAir.Services;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public class RefreshFboQueriesViewModel : ProcessBaseViewModel
    {
        public RefreshFboQueriesViewModel()
        {
            DisplayName = "Refresh FBO Queries";
        }

        public IFboService FboService { get; set; }

        protected override async Task DoTheWork()
        {
            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Refreshing FBO Queries..."
            });

            await FboService.RefreshFboQueries();

            await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                Text = $"Refreshing FBO Queries complete."
            });
        }
    }
}
