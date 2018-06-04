using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AuthServer.Provider;
using Common;
using AuthServer.Extension;

namespace AuthServer.Controllers
{
    [Route("api/oauth2/token")]
    public class TokenController : Controller
    {
        private readonly IConfiguration Configuration;

        public TokenController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpPost]
        public JsonResult Create([FromBody]LoginInputModel inputModel)
        {
            var audienceConfig = Configuration.GetSection("Audience");
            var symmetricKeyAsBase64 = audienceConfig["Secret"];
            var expiresTime = ConvertHelper.ConvertToInt(audienceConfig["ExpiresTime"]);
            var iss = audienceConfig["Iss"];
            var aud = audienceConfig["Aud"];

            if (inputModel.username == "admin" && inputModel.password == "12" && inputModel.client_secret == symmetricKeyAsBase64)
            {
                var token = new JwtTokenBuilder()
                                .AddSecurityKey(JwtSecurityKey.Create(inputModel.client_secret))
                                .AddSubject(inputModel.username)
                                .AddIssuer(iss)
                                .AddAudience(aud)
                                .AddClaim("scope", "openid pv btv tk bbt")
                                .AddClaim("NameSpace", inputModel.name_space)
                                .AddExpiry(expiresTime)
                                .Build();

                return this.ToJson(token);
            }
            else
            {
                return this.ToJson(Unauthorized());
            }
        }
    }

    public class LoginInputModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string grant_type { get; set; }
        public string client_secret { get; set; }
        public string name_space { get; set; }
    }
}
