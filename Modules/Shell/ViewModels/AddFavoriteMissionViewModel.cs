using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;
using GoonsOnAir.Services;
using PropertyChanged;
using Serilog;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public class AddFavoriteMissionViewModel : Screen
    {
        public AddFavoriteMissionViewModel()
        {
            DisplayName = "Add Mission To Favorites";
        }

        public IEventAggregator EventAggregator { get; set; }

        public IFboService FboService { get; set; }

        public string MissionId { get; set; }

        public bool IsRunning { get; set; }

        [DependsOn(nameof(MissionId))]
        public virtual bool CanFavoriteForMyCompany => !string.IsNullOrEmpty(MissionId);

        [DependsOn(nameof(MissionId))]
        public virtual bool CanFavoriteForVa => !string.IsNullOrEmpty(MissionId);

        public async Task FavoriteForMyCompany()
        {
            IsRunning = true;

            try
            {
                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Adding Mission to My Favorites..."
                });

                await FboService.FavoriteMissionForMyCompany(MissionId);

                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Adding Mission to My Favorites complete."
                });
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

        public async Task FavoriteForVa()
        {
            IsRunning = true;

            try
            {
                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Adding Mission to VA Favorites..."
                });

                await FboService.FavoriteMissionForVa(MissionId);

                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Adding Mission to VA Favorites complete."
                });
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
    }
}
