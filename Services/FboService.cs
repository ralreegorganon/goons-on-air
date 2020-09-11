﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    }

    public class FboService : IFboService
    {
        public GlobalCredentials GlobalCredentials { get; set; }

        public async Task DownloadFboMissions(string outputFolder)
        {
            var missions = new List<FboMission>();

            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company) =>
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
                                    QueryNextRequestDelta = $"{Math.Round((q.NextRequest - DateTime.UtcNow).Value.TotalHours)} hours",
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
                                        MissionExpiresDelta = MissionExpiresDelta(m.ExpirationDate),
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
                                        MissionExpiresDelta = MissionExpiresDelta(m.ExpirationDate),
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

            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company) => {
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
                            MissionExpiresDelta = MissionExpiresDelta(m.ExpirationDate),
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
                            MissionExpiresDelta = MissionExpiresDelta(m.ExpirationDate),
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
            var missions = new List<Mission>();

            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company) =>
            {
                var airportsResponse = await client.GetAirportsAsync();
                var airportsLookup = airportsResponse.Body.GetAirportsResult.ToDictionary(k => k.Id, v => v);

                var favoriteResponse = await client.FavoritesGetMissionsAsync(ap, company.Id);
                foreach (var m in favoriteResponse.Body.FavoritesGetMissionsResult)
                {
                    foreach (var cargo in m.Cargos)
                    {
                        missions.Add(new Mission {
                            MissionId = m.Id,
                            MissionMainAirport = airportsLookup[m.MainAirportId].ICAO,
                            MissionMainAirportHeading = m.MainAirportHeading,
                            MissionTotalDistance = m.TotalDistance,
                            MissionExpires = m.ExpirationDate?.ToString("s"),
                            MissionExpiresDelta = MissionExpiresDelta(m.ExpirationDate),
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
                            MissionExpires = m.ExpirationDate?.ToString("s"),
                            MissionExpiresDelta = MissionExpiresDelta(m.ExpirationDate),
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

            var path = Path.Combine(outputFolder, $"FavoriteMissions_{DateTime.Now:yyyy-MM-dd_HH.mm.ss}.xlsx");

            using var package = new ExcelPackage(new FileInfo(path));
            var worksheet = package.Workbook.Worksheets.Add("Favorite Missions");
            worksheet.Cells["A1"].LoadFromCollection(Collection: missions, PrintHeaders: true);
            using (var rng = worksheet.Cells[worksheet.Dimension.Address])
            {
                var table = worksheet.Tables.Add(rng, "FavoriteMissions");
                table.TableStyle = TableStyles.Light1;
            }
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            await package.SaveAsync();
        }

        public async Task RefreshFboQueries()
        {
            await OnAirClient.RunInOnAirScope(GlobalCredentials.AccessParams, async (client, ap, company) => {
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

                        if (currentMissions.Any(x => x.IsFavorited))
                        {
                            continue;
                        }

                        await client.FBOLogisticQueryGenerateMissionsAsync(ap, company.Id, q.Id);
                    }
                }
            });
        }

        private string MissionExpiresDelta(DateTime? expirationDate)
        {
            if (expirationDate == null)
            {
                return "never";
            }

            var delta = (expirationDate - DateTime.UtcNow).Value;

            return delta.ToString(@"dd\:hh\:mm");
        }
    }

    public class Mission
    {
        public string MissionId { get; set; }
        public string MissionMainAirport { get; set; }
        public double? MissionMainAirportHeading { get; set; }
        public double? MissionTotalDistance { get; set; }
        public string MissionExpires { get; set; }
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
