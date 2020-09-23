using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using ServiceReference2;

namespace GoonsOnAir.Services
{
    public interface IFboService
    {
        public Task DownloadFboMissions(string outputFolder);
        public Task DownloadPendingMissions(string outputFolder);
        public Task DownloadFavoriteMissions(string outputFolder);
        public Task RefreshFboQueries();
        public Task AcceptMyFavorites();
        public Task AcceptVaFavorites();
        public Task FavoriteMissionForMyCompany(string missionId);
        public Task FavoriteMissionForVa(string missionId);
        public Task UpgradeFbos(bool shouldIncreaseJetFuelCapacity, int? jetFuelCapacity, bool shouldStartSellingJetFuel, decimal? jetFuelSalePrice, bool shouldStopSellingJetFuel, bool shouldPurchaseJetFuel, bool shouldLimitFbos, List<string> icaos);
        public Task DownloadCashFlow(string outputFolder);
    }

    public class FboService : IFboService
    {
        public GlobalCredentials GlobalCredentials { get; set; }

        public async Task DownloadFboMissions(string outputFolder)
        {
            var missions = new List<FboMission>();

            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company, va) =>
                {
                    var airportsResponse = await client.GetAirportsAsync();
                    var airportsLookup = airportsResponse.Body.GetAirportsResult.ToDictionary(k => k.Id, v => v);

                    var companyFbOsResponse = await client.GetCompanyFBOsAsync(company.Id);
                    foreach (var fbo in companyFbOsResponse.Body.GetCompanyFBOsResult)
                    {
                        var logisticQueriesResponse = await client.GetFBOLogisticQueriesAsync(ap, company.Id, fbo.Id);
                        foreach (var q in logisticQueriesResponse.Body.GetFBOLogisticQueriesResult.OrderBy(x => x.Order))
                        {
                            var missionsResponse = await client.FBOLogisticQueryGetMissionsAsync(ap, company.Id, q.Id);
                            var currentMissions = missionsResponse.Body.FBOLogisticQueryGetMissionsResult;

                            if (currentMissions.Count == 0)
                            {
                                missions.Add(new FboMission {
                                    FboId = fbo.Id,
                                    FboName = fbo.Name,
                                    FboIcao = airportsLookup[fbo.AirportId].ICAO,
                                    QueryId = q.Id,
                                    QueryOrder = q.Order,
                                    QueryDirection = q.Heading,
                                    QueryMinRange = q.MinRange,
                                    QueryMaxRange = q.MaxRange,
                                    QueryMinCargo = q.MinCargoWeight,
                                    QueryMaxCargo = q.MaxCargoWeight,
                                    QueryMinPAX = q.MinPAX,
                                    QueryMaxPAX = q.MaxPAX,
                                    QueryMinAirportSize = q.MinAirportSize,
                                    QueryCanGenerateMissions = q.CanGenerateMissions,
                                    QueryNextRequest = q.NextRequest?.ToString("s"),
                                    QueryNextRequestDelta = q.NextRequest == null ? null : $"{Math.Round((q.NextRequest - DateTime.UtcNow).Value.TotalHours)} hours",
                                });
                                continue;
                            }

                            foreach (var m in currentMissions)
                            {
                                foreach (var cargo in m.Cargos)
                                {
                                    missions.Add(new FboMission {
                                        FboId = fbo.Id,
                                        FboName = fbo.Name,
                                        FboIcao = airportsLookup[fbo.AirportId].ICAO,
                                        QueryId = q.Id,
                                        QueryOrder = q.Order,
                                        QueryDirection = q.Heading,
                                        QueryMinRange = q.MinRange,
                                        QueryMaxRange = q.MaxRange,
                                        QueryMinCargo = q.MinCargoWeight,
                                        QueryMaxCargo = q.MaxCargoWeight,
                                        QueryMinPAX = q.MinPAX,
                                        QueryMaxPAX = q.MaxPAX,
                                        QueryMinAirportSize = q.MinAirportSize,
                                        QueryCanGenerateMissions = q.CanGenerateMissions,
                                        QueryNextRequest = q.NextRequest?.ToString("s"),
                                        QueryNextRequestDelta = $"{Math.Round((q.NextRequest - DateTime.UtcNow).Value.TotalHours)} hours",
                                        MissionId = m.Id,
                                        MissionMainAirport = airportsLookup[m.MainAirportId].ICAO,
                                        MissionMainAirportHeading = m.MainAirportHeading,
                                        MissionTotalDistance = m.TotalDistance,
                                        MissionExpires = m.ExpirationDate?.ToString("s"),
                                        MissionExpiresDelta = MissionExpiresDeltaFormatted(m.ExpirationDate),
                                        MissionExpiresDeltaMinutes = MissionExpiresDeltaMinutes(m.ExpirationDate),
                                        MissionPenalty = m.Penality,
                                        MissionPay = m.Pay,
                                        MissionFavorited = m.IsFavorited,
                                        LegFrom = cargo.DepartureAirport.ICAO,
                                        LegTo = cargo.DestinationAirport.ICAO,
                                        LegCurrent = cargo.CurrentAirport?.ICAO,
                                        LegAircraft = cargo.CurrentAircraft?.Identifier,
                                        LegCargo = cargo.Weight,
                                        LegHeading = cargo.Heading,
                                        LegDistance = cargo.Distance
                                    });
                                }
                                foreach (var charter in m.Charters)
                                {
                                    missions.Add(new FboMission {
                                        FboId = fbo.Id,
                                        FboName = fbo.Name,
                                        FboIcao = airportsLookup[fbo.AirportId].ICAO,
                                        QueryId = q.Id,
                                        QueryOrder = q.Order,
                                        QueryDirection = q.Heading,
                                        QueryMinRange = q.MinRange,
                                        QueryMaxRange = q.MaxRange,
                                        QueryMinCargo = q.MinCargoWeight,
                                        QueryMaxCargo = q.MaxCargoWeight,
                                        QueryMinPAX = q.MinPAX,
                                        QueryMaxPAX = q.MaxPAX,
                                        QueryMinAirportSize = q.MinAirportSize,
                                        QueryCanGenerateMissions = q.CanGenerateMissions,
                                        QueryNextRequest = q.NextRequest?.ToString("s"),
                                        QueryNextRequestDelta = $"{Math.Round((q.NextRequest - DateTime.UtcNow).Value.TotalHours)} hours",
                                        MissionId = m.Id,
                                        MissionMainAirport = airportsLookup[m.MainAirportId].ICAO,
                                        MissionMainAirportHeading = m.MainAirportHeading,
                                        MissionTotalDistance = m.TotalDistance,
                                        MissionExpires = m.ExpirationDate?.ToString("s"),
                                        MissionExpiresDelta = MissionExpiresDeltaFormatted(m.ExpirationDate),
                                        MissionExpiresDeltaMinutes = MissionExpiresDeltaMinutes(m.ExpirationDate),
                                        MissionPenalty = m.Penality,
                                        MissionPay = m.Pay,
                                        MissionFavorited = m.IsFavorited,
                                        LegFrom = charter.DepartureAirport.ICAO,
                                        LegTo = charter.DestinationAirport.ICAO,
                                        LegCurrent = charter.CurrentAirport?.ICAO,
                                        LegAircraft = charter.CurrentAircraft?.Identifier,
                                        LegPax = charter.PassengersNumber,
                                        LegHeading = charter.Heading,
                                        LegDistance = charter.Distance
                                    });
                                }
                            }
                        }
                    }
                });

            var path = Path.Combine(outputFolder, $"FboMissions_{DateTime.Now:yyyy-MM-dd_HH.mm.ss}.xlsx");

            using var package = new ExcelPackage(new FileInfo(path));
            var worksheet = package.Workbook.Worksheets.Add("FBO Missions");
            worksheet.Cells["A1"].LoadFromCollection(Collection: missions, PrintHeaders: true);
            using (var rng = worksheet.Cells[worksheet.Dimension.Address])
            {
                var table = worksheet.Tables.Add(rng, "FBOMissions");
                table.TableStyle = TableStyles.Light1;
            }
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var column = "E1:E" + worksheet.Dimension.End.Row;
            var range = worksheet.Cells[column];

            var condition1 = worksheet.ConditionalFormatting.AddExpression(range);
            condition1.Formula = new ExcelFormulaAddress(range.Address) + "= 0";
            condition1.Style.Fill.BackgroundColor.Color = System.Drawing.Color.LightSkyBlue;

            var condition2 = worksheet.ConditionalFormatting.AddExpression(range);
            condition2.Formula = new ExcelFormulaAddress(range.Address) + "= 1";
            condition2.Style.Fill.BackgroundColor.Color = System.Drawing.Color.MediumSeaGreen;

            var condition3 = worksheet.ConditionalFormatting.AddExpression(range);
            condition3.Formula = new ExcelFormulaAddress(range.Address) + "= 2";
            condition3.Style.Fill.BackgroundColor.Color = System.Drawing.Color.Salmon;


            await package.SaveAsync();
        }

        public async Task DownloadPendingMissions(string outputFolder)
        {
            var missions = new List<Mission>();

            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company, va) => {
                var airportsResponse = await client.GetAirportsAsync();
                var airportsLookup = airportsResponse.Body.GetAirportsResult.ToDictionary(k => k.Id, v => v);

                var missionsResponse = await client.GetMissionsByCompanyIDAsync(ap, company.Id);

                foreach (var m in missionsResponse.Body.GetMissionsByCompanyIDResult)
                {
                    foreach (var cargo in m.Cargos)
                    {
                        missions.Add(new Mission {
                            MissionId = m.Id,
                            MissionMainAirport = airportsLookup[m.MainAirportId].ICAO,
                            MissionMainAirportHeading = m.MainAirportHeading,
                            MissionTotalDistance = m.TotalDistance,
                            MissionExpires = m.ExpirationDate == null ? "Never" : m.ExpirationDate?.ToString("s"),
                            MissionExpiresDelta = MissionExpiresDeltaFormatted(m.ExpirationDate),
                            MissionExpiresDeltaMinutes = MissionExpiresDeltaMinutes(m.ExpirationDate),
                            MissionPenalty = m.Penality,
                            MissionPay = m.Pay,
                            MissionFavorited = m.IsFavorited,
                            LegFrom = cargo.DepartureAirport.ICAO,
                            LegTo = cargo.DestinationAirport.ICAO,
                            LegCurrent = cargo.CurrentAirport?.ICAO,
                            LegAircraft = cargo.CurrentAircraft?.Identifier,
                            LegCargo = cargo.Weight,
                            LegHeading = cargo.Heading,
                            LegDistance = cargo.Distance
                        });
                    }
                    foreach (var charter in m.Charters)
                    {
                        missions.Add(new Mission {
                            MissionId = m.Id,
                            MissionMainAirport = airportsLookup[m.MainAirportId].ICAO,
                            MissionMainAirportHeading = m.MainAirportHeading,
                            MissionTotalDistance = m.TotalDistance,
                            MissionExpires = m.ExpirationDate == null ? "Never" : m.ExpirationDate?.ToString("s"),
                            MissionExpiresDelta = MissionExpiresDeltaFormatted(m.ExpirationDate),
                            MissionExpiresDeltaMinutes = MissionExpiresDeltaMinutes(m.ExpirationDate),
                            MissionPenalty = m.Penality,
                            MissionPay = m.Pay,
                            MissionFavorited = m.IsFavorited,
                            LegFrom = charter.DepartureAirport.ICAO,
                            LegTo = charter.DestinationAirport.ICAO,
                            LegCurrent = charter.CurrentAirport?.ICAO,
                            LegAircraft = charter.CurrentAircraft?.Identifier,
                            LegPax = charter.PassengersNumber,
                            LegHeading = charter.Heading,
                            LegDistance = charter.Distance
                        });
                    }
                }
            });

            var path = Path.Combine(outputFolder, $"PendingMissions_{DateTime.Now:yyyy-MM-dd_HH.mm.ss}.xlsx");

            using var package = new ExcelPackage(new FileInfo(path));
            var worksheet = package.Workbook.Worksheets.Add("Pending Missions");
            worksheet.Cells["A1"].LoadFromCollection(Collection: missions, PrintHeaders: true);
            using (var rng = worksheet.Cells[worksheet.Dimension.Address])
            {
                var table = worksheet.Tables.Add(rng, "PendingMissions");
                table.TableStyle = TableStyles.Light1;
            }
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            await package.SaveAsync();
        }

        public async Task DownloadFavoriteMissions(string outputFolder)
        {
            var missionsMine = new List<Mission>();
            var missionsVa = new List<Mission>();

            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company, va) =>
            {
                var airportsResponse = await client.GetAirportsAsync();
                var airportsLookup = airportsResponse.Body.GetAirportsResult.ToDictionary(k => k.Id, v => v);

                async Task GetFavorites(Company c, List<Mission> missions)
                {
                    var favoriteResponse = await client.FavoritesGetMissionsAsync(ap, c.Id);
                    foreach (var m in favoriteResponse.Body.FavoritesGetMissionsResult)
                    {
                        foreach (var cargo in m.Cargos)
                        {
                            missions.Add(new Mission {
                                MissionId = m.Id,
                                MissionMainAirport = airportsLookup[m.MainAirportId]
                                    .ICAO,
                                MissionMainAirportHeading = m.MainAirportHeading,
                                MissionTotalDistance = m.TotalDistance,
                                MissionExpires = m.ExpirationDate?.ToString("s"),
                                MissionExpiresDelta = MissionExpiresDeltaFormatted(m.ExpirationDate),
                                MissionExpiresDeltaMinutes = MissionExpiresDeltaMinutes(m.ExpirationDate),
                                MissionPenalty = m.Penality,
                                MissionPay = m.Pay,
                                MissionFavorited = m.IsFavorited,
                                LegFrom = cargo.DepartureAirport.ICAO,
                                LegTo = cargo.DestinationAirport.ICAO,
                                LegCurrent = cargo.CurrentAirport?.ICAO,
                                LegAircraft = cargo.CurrentAircraft?.Identifier,
                                LegCargo = cargo.Weight,
                                LegHeading = cargo.Heading,
                                LegDistance = cargo.Distance
                            });
                        }

                        foreach (var charter in m.Charters)
                        {
                            missions.Add(new Mission {
                                MissionId = m.Id,
                                MissionMainAirport = airportsLookup[m.MainAirportId]
                                    .ICAO,
                                MissionMainAirportHeading = m.MainAirportHeading,
                                MissionTotalDistance = m.TotalDistance,
                                MissionExpires = m.ExpirationDate?.ToString("s"),
                                MissionExpiresDelta = MissionExpiresDeltaFormatted(m.ExpirationDate),
                                MissionExpiresDeltaMinutes = MissionExpiresDeltaMinutes(m.ExpirationDate),
                                MissionPenalty = m.Penality,
                                MissionPay = m.Pay,
                                MissionFavorited = m.IsFavorited,
                                LegFrom = charter.DepartureAirport.ICAO,
                                LegTo = charter.DestinationAirport.ICAO,
                                LegCurrent = charter.CurrentAirport?.ICAO,
                                LegAircraft = charter.CurrentAircraft?.Identifier,
                                LegPax = charter.PassengersNumber,
                                LegHeading = charter.Heading,
                                LegDistance = charter.Distance
                            });
                        }
                    }
                }

                await GetFavorites(company, missionsMine);
                await GetFavorites(va, missionsVa);
            });

            var path = Path.Combine(outputFolder, $"FavoriteMissions_{DateTime.Now:yyyy-MM-dd_HH.mm.ss}.xlsx");

            using var package = new ExcelPackage(new FileInfo(path));
            var worksheet = package.Workbook.Worksheets.Add("Favorite Missions (Mine)");
            worksheet.Cells["A1"].LoadFromCollection(Collection: missionsMine, PrintHeaders: true);
            using (var rng = worksheet.Cells[worksheet.Dimension.Address])
            {
                var table = worksheet.Tables.Add(rng, "FavoriteMissionsMine");
                table.TableStyle = TableStyles.Light1;
            }
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            worksheet = package.Workbook.Worksheets.Add("Favorite Missions (VA)");
            worksheet.Cells["A1"].LoadFromCollection(Collection: missionsVa, PrintHeaders: true);
            using (var rng = worksheet.Cells[worksheet.Dimension.Address])
            {
                var table = worksheet.Tables.Add(rng, "FavoriteMissionsVA");
                table.TableStyle = TableStyles.Light1;
            }
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            await package.SaveAsync();
        }

        public async Task RefreshFboQueries()
        {
            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company, va) => {
                var favoriteResponse = await client.FavoritesGetMissionsAsync(ap, va.Id);
                var companyFavorites = favoriteResponse.Body.FavoritesGetMissionsResult.Select(x => x.Id);

                var companyFbOsResponse = await client.GetCompanyFBOsAsync(company.Id);
                foreach (var fbo in companyFbOsResponse.Body.GetCompanyFBOsResult)
                {
                    var logisticQueriesResponse = await client.GetFBOLogisticQueriesAsync(ap, company.Id, fbo.Id);
                    foreach (var q in logisticQueriesResponse.Body.GetFBOLogisticQueriesResult.OrderBy(x => x.Order))
                    {
                        if (!q.CanGenerateMissions)
                        {
                            continue;
                        }

                        var missionsResponse = await client.FBOLogisticQueryGetMissionsAsync(ap, company.Id, q.Id);
                        var currentMissions = missionsResponse.Body.FBOLogisticQueryGetMissionsResult;

                        if (currentMissions.Any(x => x.IsFavorited || companyFavorites.Contains(x.Id)))
                        {
                            continue;
                        }

                        await client.FBOLogisticQueryGenerateMissionsAsync(ap, company.Id, q.Id);
                    }
                }
            });
        }

        public async Task AcceptMyFavorites()
        {
            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company, va) => {
                var peopleResponse = await client.GetUserPeopleByCompanyIDAsync(ap, company.Id);
                var favoriteResponse = await client.FavoritesGetMissionsAsync(ap, company.Id);
                foreach (var m in favoriteResponse.Body.FavoritesGetMissionsResult)
                {
                    if ((m.ExpirationDate - DateTime.UtcNow).Value.TotalMinutes < 22 && m.ExpirationDate > DateTime.UtcNow)
                    {
                        await client.AcceptMissionAsync(ap, peopleResponse.Body.GetUserPeopleByCompanyIDResult.Id, company.Id, m.Id);
                    }
                }
            });
        }

        public async Task AcceptVaFavorites()
        {
            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company, va) => {
                var peopleResponse = await client.GetUserPeopleByCompanyIDAsync(ap, va.Id);
                var favoriteResponse = await client.FavoritesGetMissionsAsync(ap, va.Id);
                foreach (var m in favoriteResponse.Body.FavoritesGetMissionsResult)
                {
                    if ((m.ExpirationDate - DateTime.UtcNow).Value.TotalMinutes < 22 && m.ExpirationDate > DateTime.UtcNow)
                    {
                        await client.AcceptMissionAsync(ap, peopleResponse.Body.GetUserPeopleByCompanyIDResult.Id, va.Id, m.Id);
                    }
                }
            });
        }

        public async Task FavoriteMissionForMyCompany(string missionId)
        {
            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company, va) => {
                await client.FavoritesAddAsync(ap, company.Id, missionId);
            });
        }

        public async Task FavoriteMissionForVa(string missionId)
        {
            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company, va) => {
                await client.FavoritesAddAsync(ap,  va.Id, missionId);
            });
        }

        public async Task UpgradeFbos(bool shouldIncreaseJetFuelCapacity, int? jetFuelCapacity, bool shouldStartSellingJetFuel, decimal? jetFuelSalePrice, bool shouldStopSellingJetFuel, bool shouldPurchaseJetFuel, bool shouldLimitFbos, List<string> icaos)
        {
            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company, va) => {
                var airportsResponse = await client.GetAirportsAsync();
                var airportsLookup = airportsResponse.Body.GetAirportsResult.ToDictionary(k => k.Id, v => v);
                var peopleResponse = await client.GetUserPeopleByCompanyIDAsync(ap, company.Id);

                var companyFbOsResponse = await client.GetCompanyFBOsAsync(company.Id);
                foreach (var fbo in companyFbOsResponse.Body.GetCompanyFBOsResult)
                {
                    if (shouldLimitFbos && !icaos.Contains(airportsLookup[fbo.AirportId]
                        .ICAO))
                    {
                        continue;
                    }

                    if (shouldStopSellingJetFuel)
                    {
                        fbo.AllowFuelJetSelling = false;
                    }

                    if (shouldStartSellingJetFuel)
                    {
                        fbo.AllowFuelJetSelling = true;
                        fbo.FuelJetSellPrice = jetFuelSalePrice.Value;
                    }

                    if (shouldStopSellingJetFuel || shouldStartSellingJetFuel)
                    {
                        await client.UpdateFBOSellParamsAsync(ap, company.Id, fbo);
                    }

                    if (shouldPurchaseJetFuel && (fbo.FuelJetCapacity - fbo.FuelJetQuantity) > 0)
                    {
                        await client.BuyFuelFromLocalDealerAsync(ap, peopleResponse.Body.GetUserPeopleByCompanyIDResult.Id, fbo.Id, 0, fbo.FuelJetCapacity - fbo.FuelJetQuantity);
                    }

                    if (shouldIncreaseJetFuelCapacity && fbo.FuelJetOrderedCapacity == 0)
                    {
                        fbo.FuelJetCapacity += jetFuelCapacity.Value;
                        await client.UpdateFBOAsync(ap, fbo);
                    }
                }
            });
        }

        public async Task DownloadCashFlow(string outputFolder)
        {
            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company, va) => {
                var cashFlowResponse = await client.AccountGetCompanyCashFlowAsync(ap, company.Id);
                var flow = cashFlowResponse.Body.AccountGetCompanyCashFlowResult.Entries.Select(x => new {
                        Timestamp = x.CreationDate.ToString("s"),
                        x.Description,
                        x.Amount
                    })
                    .ToList();

                var cashFlowResponseVa = await client.AccountGetCompanyCashFlowAsync(ap, va.Id);
                var flowVa = cashFlowResponse.Body.AccountGetCompanyCashFlowResult.Entries.Select(x => new {
                        Timestamp = x.CreationDate.ToString("s"),
                        x.Description,
                        x.Amount
                    })
                    .ToList();

                var path = Path.Combine(outputFolder, $"CashFlow_{DateTime.Now:yyyy-MM-dd_HH.mm.ss}.xlsx");

                using var package = new ExcelPackage(new FileInfo(path));
                var worksheet = package.Workbook.Worksheets.Add("Cash Flow (Mine)");
                worksheet.Cells["A1"].LoadFromCollection(Collection: flow, PrintHeaders: true);
                using (var rng = worksheet.Cells[worksheet.Dimension.Address])
                {
                    var table = worksheet.Tables.Add(rng, "CashFlowMine");
                    table.TableStyle = TableStyles.Light1;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                worksheet = package.Workbook.Worksheets.Add("Cash Flow (VA)");
                worksheet.Cells["A1"].LoadFromCollection(Collection: flowVa, PrintHeaders: true);
                using (var rng = worksheet.Cells[worksheet.Dimension.Address])
                {
                    var table = worksheet.Tables.Add(rng, "CashFlowVA");
                    table.TableStyle = TableStyles.Light1;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                await package.SaveAsync();
            });
        }

        private string MissionExpiresDeltaFormatted(DateTime? expirationDate)
        {
            if (expirationDate == null)
            {
                return "never";
            }

            var delta = (expirationDate - DateTime.UtcNow).Value;

            return delta.Ticks < 0 ? "expired" : $"{delta:dd} days, {delta:hh} hours, {delta:mm} minutes";
        }

        private int? MissionExpiresDeltaMinutes(DateTime? expirationDate)
        {
            if (expirationDate == null)
            {
                return null;
            }

            return (int?) (expirationDate - DateTime.UtcNow).Value.TotalMinutes;
        }
    }

    public class Mission
    {
        public string MissionId { get; set; }
        public string MissionMainAirport { get; set; }
        public double? MissionMainAirportHeading { get; set; }
        public double? MissionTotalDistance { get; set; }
        public string MissionExpires { get; set; }
        public int? MissionExpiresDeltaMinutes { get; set; }
        public string MissionExpiresDelta { get; set; }
        public decimal? MissionPenalty { get; set; }
        public decimal? MissionPay { get; set; }
        public bool? MissionFavorited { get; set; }
        public string LegFrom { get; set; }
        public string LegTo { get; set; }
        public string LegCurrent { get; set; }
        public double? LegCargo { get; set; }
        public int? LegPax { get; set; }
        public double? LegHeading { get; set; }
        public double? LegDistance { get; set; }
        public string LegAircraft { get; set; }
    }

    public class FboMission
    {
        public string FboId { get; set; }
        public string FboName { get; set; }
        public string FboIcao { get; set; }
        public string QueryId { get; set; }
        public int QueryOrder { get; set; }
        public double QueryDirection { get; set; }
        public double QueryMinRange { get; set; }
        public double QueryMaxRange { get; set; }
        public double QueryMinCargo { get; set; }
        public double QueryMaxCargo { get; set; }
        public double QueryMinPAX { get; set; }
        public double QueryMaxPAX { get; set; }
        public int QueryMinAirportSize { get; set; }
        public bool QueryCanGenerateMissions { get; set; }
        public string QueryNextRequest { get; set; }
        public string QueryNextRequestDelta { get; set; }
        public string MissionId { get; set; }
        public string MissionMainAirport { get; set; }
        public double? MissionMainAirportHeading { get; set; }
        public double? MissionTotalDistance { get; set; }
        public string MissionExpires { get; set; }
        public int? MissionExpiresDeltaMinutes { get; set; }
        public string MissionExpiresDelta { get; set; }
        public decimal? MissionPenalty { get; set; }
        public decimal? MissionPay { get; set; }
        public bool? MissionFavorited { get; set; }
        public string LegFrom { get; set; }
        public string LegTo { get; set; }
        public string LegCurrent { get; set; }
        public double? LegCargo { get; set; }
        public int? LegPax { get; set; }
        public double? LegHeading { get; set; }
        public double? LegDistance { get; set; }
        public string LegAircraft { get; set; }
    }
}
