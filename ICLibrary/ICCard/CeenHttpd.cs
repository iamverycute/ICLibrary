using Ceen.Httpd;
using System.Net;
using Ceen.Httpd.Handler;

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
                    ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
                    await ctx.Response.WriteAllAsync(Helper.GetResponseText());
                    return true;
                }))
                .AddRoute("/beep", new HttpHandlerDelegate(async (ctx) =>
                {
                    ctx.Response.SetNonCacheable();
                    ctx.Response.StatusCode = Ceen.HttpStatusCode.NoContent;
                    ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
                    await ctx.Response.WriteAllAsync(Helper.iBeep());
                    return true;
                }))
                .AddRoute(new FileHandler(".\\www"))
            ).GetAwaiter().GetResult();
        }
    }
}