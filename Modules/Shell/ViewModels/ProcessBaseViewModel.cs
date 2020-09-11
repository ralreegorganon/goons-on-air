using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;
using Serilog;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public abstract class ProcessBaseViewModel : Screen
    {
        public IEventAggregator EventAggregator { get; set; }


        public bool IsRunning { get; set; }

        public virtual bool CanRun => true;

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
