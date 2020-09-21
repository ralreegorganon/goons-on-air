using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GoonsOnAir.Modules.Shell.Events;
using GoonsOnAir.Services;
using PropertyChanged;
using Serilog;

namespace GoonsOnAir.Modules.Shell.ViewModels
{
    public class FboUpgradeViewModel : Screen
    {
        public FboUpgradeViewModel()
        {
            DisplayName = "Upgrade FBOs";
        }

        public IEventAggregator EventAggregator { get; set; }

        public IFboService FboService { get; set; }

        public bool ShouldIncreaseJetFuelCapacity { get; set; }
        public bool ShouldPurchaseJetFuel { get; set; }
        public bool ShouldStartSellingJetFuel { get; set; }
        public bool ShouldStopSellingJetFuel { get; set; }
        public bool ShouldLimitFbos { get; set; }
        public int? JetFuelCapacity { get; set; }
        public decimal? JetFuelSalePrice { get; set; }
        public string FboIcaos { get; set; }

        public bool IsRunning { get; set; }

        [DependsOn(nameof(ShouldIncreaseJetFuelCapacity), nameof(ShouldPurchaseJetFuel), nameof(ShouldStartSellingJetFuel), nameof(ShouldStopSellingJetFuel), nameof(ShouldLimitFbos), nameof(JetFuelCapacity), nameof(JetFuelSalePrice), nameof(FboIcaos))]
        public bool CanRun
        {
            get
            {
                if (!ShouldIncreaseJetFuelCapacity && !ShouldPurchaseJetFuel && !ShouldStartSellingJetFuel && !ShouldStopSellingJetFuel)
                {
                    return false;
                }

                if (ShouldIncreaseJetFuelCapacity && JetFuelCapacity == null)
                {
                    return false;
                }

                if (ShouldStartSellingJetFuel && JetFuelSalePrice == null)
                {
                    return false;
                }

                if (ShouldLimitFbos && string.IsNullOrWhiteSpace(FboIcaos))
                {
                    return false;
                }
                
                return true;
            }
        }

        public async Task Run()
        {
            IsRunning = true;

            try
            {
                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Upgrading FBOs..."
                });


                var icaos = FboIcaos?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .ToList() ?? new List<string>();

                await FboService.UpgradeFbos(ShouldIncreaseJetFuelCapacity, JetFuelCapacity, ShouldStartSellingJetFuel, JetFuelSalePrice, ShouldStopSellingJetFuel, ShouldPurchaseJetFuel, ShouldLimitFbos, icaos);

                await EventAggregator.PublishOnUIThreadAsync(new StatusMessageEvent {
                    Text = $"Upgrading FBOs complete."
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
