using Ceen.Httpd;
using System.Net;

namespace ICLibrary.ICCard
{
    class CeenHttpd
    {
        public static void CeenHttpServer()
        {
            HttpServer.ListenAsync(
                new IPEndPoint(IPAddress.Any, 33448),
                false,
                new ServerConfig()
                .AddRoute("/tryread", new HttpHandlerDelegate(async (ctx) =>
                {
                    ctx.Response.SetNonCacheable();
                    await ctx.Response.WriteAllAsync(Helper.GetResponseText());
                    return true;
                }))
            ).GetAwaiter().GetResult();
        }
    }
}