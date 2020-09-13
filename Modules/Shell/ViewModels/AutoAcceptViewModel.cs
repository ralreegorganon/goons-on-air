using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;
using GoonsOnAir.Services;
using Serilog;
using Timer = System.Timers.Timer;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public class AutoAcceptViewModel : Screen
    {
        public AutoAcceptViewModel()
        {
            DisplayName = "Automatically Accept Favorite Missions";
        }

        public IEventAggregator EventAggregator { get; set; }

        public IFboService FboService { get; set; }

        private Timer AcceptFavoritesTimer { get; set; }

        private BackgroundWorker BackgroundWorker { get; } = new BackgroundWorker();

        public bool AutoAcceptMine { get; set; }

        public bool AutoAcceptVa { get; set; }

        public string LastRanAt { get; set; }

        public void OnAutoAcceptMineChanged()
        {
            Accept(null, null);
        }

        public void OnAutoAcceptVaChanged()
        {
            Accept(null, null);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var interval = 15;

            BackgroundWorker.DoWork += BackgroundWorkerOnDoWork;
            BackgroundWorker.RunWorkerCompleted += BackgroundWorkerOnRunWorkerCompleted;
            BackgroundWorker.WorkerSupportsCancellation = true;

            AcceptFavoritesTimer = new Timer(TimeSpan.FromMinutes(interval)
                .TotalMilliseconds);
            AcceptFavoritesTimer.Elapsed += Accept;
            AcceptFavoritesTimer.Start();

            return base.OnActivateAsync(cancellationToken);
        }

        private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            AcceptFavoritesTimer.Stop();

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new System.Action(() => {
                IsRunning = true;
            }));

            try
            {
                if (AutoAcceptMine)
                {
                    FboService.AcceptMyFavorites().Wait();
                }
                if (AutoAcceptVa)
                {
                    FboService.AcceptVaFavorites().Wait();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unhandled exception occurred");
                EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Error: {ex}"
                }).Wait();
                throw;
            }

            if (BackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void BackgroundWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new System.Action(() => {
                IsRunning = false;
            }));

            LastRanAt = $"Last ran at (local time): {DateTime.Now:g}" ;

            AcceptFavoritesTimer.Start();
        }

        private void Accept(object sender, ElapsedEventArgs e)
        {
            if (!BackgroundWorker.IsBusy)
            {
                BackgroundWorker.RunWorkerAsync();
            }
        }

        public bool IsRunning { get; set; }
    }
}
