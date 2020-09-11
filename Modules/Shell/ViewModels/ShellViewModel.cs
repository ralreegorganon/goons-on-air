using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public class ShellViewModel : Conductor<Screen>.Collection.OneActive,
        IHandle<StatusMessageEvent>
    {
        public ShellViewModel(IEventAggregator eventAggregator)
        {
            DisplayName = "GOONS On Air";
            eventAggregator.SubscribeOnPublishedThread(this);
        }

        public string StatusText { get; set; }

        public DownloadFboMissionsViewModel DownloadFboMissionsViewModel { get; set; }
        public DownloadFavoritesViewModel DownloadFavoritesViewModel { get; set; }
        public DownloadPendingViewModel DownloadPendingViewModel { get; set; }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            await SwitchToDownloadFboMissions();
        }

        public async Task SwitchToDownloadFboMissions()
        {
            await ActivateItemAsync(DownloadFboMissionsViewModel, CancellationToken.None);
        }

        public async Task SwitchToDownloadFavorites()
        {
            await ActivateItemAsync(DownloadFavoritesViewModel, CancellationToken.None);
        }

        public async Task SwitchToDownloadPending()
        {
            await ActivateItemAsync(DownloadPendingViewModel, CancellationToken.None);
        }

        public Task HandleAsync(StatusMessageEvent message, CancellationToken cancellationToken)
        {
            StatusText = message.Text;

            return Task.CompletedTask;
        }
    }
}
