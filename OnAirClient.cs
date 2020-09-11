using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using ServiceReference1;
using ServiceReference2;
using AccessParams = ServiceReference2.AccessParams;

namespace GoonsOnAir
{
    public static class OnAirClient
    {
        public static async Task RunInOnAirScope(AccessParams ap, Func<WSOnAirSoapClient, AccessParams, Company, Task> action)
        {
            var basicHttpsBinding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
            var remoteAddress = new EndpointAddress("https://server.onair.company/WS/WSOnAir.asmx");
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
            var currentWorld = worlds.Body.GetWorldsResult.Single(x => x.Name == "Thunder");
            var companyResult = await client.GetCompanyByOwnerAsync(ap, null, currentWorld.Id);
            var company = companyResult.Body.GetCompanyByOwnerResult;

            await action(client, ap, company);
        }

        public static async Task RunInOnAirScope(string email, string password, Func<WSOnAirSoapClient, AccessParams, Company, Task> action)
        {
            var authClient = new OnAirAuthenticationWSClient(
                new BasicHttpsBinding(BasicHttpsSecurityMode.Transport),
                new EndpointAddress("https://authentication.onair.company/WS/OnAirAuthenticationWS.svc"));

            var loginResult = await authClient.LoginAsync(new LoginParams {
                AccessParams = new ServiceReference1.AccessParams {
                    OAuth2Service = ServiceReference1.OAuth2Service.OnAir,
                    OnAirParams = new ServiceReference1.OnAirParams {
                        Email = email,
                        Password = password
                    }
                },
                DirectoryId = new Guid("61a74a0a-3152-4675-be1a-3a8bd3a874d1")
            });

            var token = loginResult.AccessParams.OnAirParams.AccessToken;

            var accessParams = new ServiceReference2.AccessParams {
                OnAirParams = new ServiceReference2.OnAirParams { AccessToken = token }
            };

            await RunInOnAirScope(accessParams, action);
        }
    }
}
