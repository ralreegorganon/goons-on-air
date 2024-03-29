﻿using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using OnAir.Client.OnAirServiceReference;

namespace GoonsOnAir
{
    public interface IOnAirClient
    {
        public Task RunInOnAirScope(AccessParams ap, Func<WSOnAirSoapClient, AccessParams, Company, Company, Task> action);

        public string VaName { get; set; }
        public string WorldName { get; set; }
        public string ApiServer { get; set; }
    }

    public class OnAirClient : IOnAirClient
    {
        public async Task RunInOnAirScope(AccessParams ap, Func<WSOnAirSoapClient, AccessParams, Company, Company, Task> action)
        {
            var basicHttpsBinding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
            var remoteAddress = new EndpointAddress($"https://{ApiServer}.onair.company/WS/WSOnAir.asmx");
            basicHttpsBinding.CloseTimeout = TimeSpan.FromSeconds(120.0);
            basicHttpsBinding.OpenTimeout = TimeSpan.FromSeconds(120.0);
            basicHttpsBinding.ReceiveTimeout = TimeSpan.FromSeconds(120.0);
            basicHttpsBinding.SendTimeout = TimeSpan.FromSeconds(120.0);
            basicHttpsBinding.MaxBufferSize = int.MaxValue;
            basicHttpsBinding.MaxReceivedMessageSize = (long)int.MaxValue;
            basicHttpsBinding.ReaderQuotas.MaxDepth = 128;
            basicHttpsBinding.ReaderQuotas.MaxStringContentLength = 8388608;
            basicHttpsBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            basicHttpsBinding.ReaderQuotas.MaxBytesPerRead = 4096;
            basicHttpsBinding.ReaderQuotas.MaxNameTableCharCount = 16384;

            var client = new WSOnAirSoapClient(basicHttpsBinding, remoteAddress);
            var worlds = await client.GetWorldsAsync(ap);
            var currentWorld = worlds.Body.GetWorldsResult.Single(x => x.Name == WorldName);
            var companyResult = await client.GetCompanyByOwnerAsync(ap, null, currentWorld.Id);
            var company = companyResult.Body.GetCompanyByOwnerResult;
            var vaCompanyResult = await client.GetCompanyByCodeAsync(VaName, currentWorld.Id);
            var vaCompany = vaCompanyResult.Body.GetCompanyByCodeResult;

            await action(client, ap, company, vaCompany);
        }

        public string VaName { get; set; } = "GOONS";

        public string WorldName { get; set; } = "Thunder";

        public string ApiServer { get; set; } = "thunder";
    }
}
