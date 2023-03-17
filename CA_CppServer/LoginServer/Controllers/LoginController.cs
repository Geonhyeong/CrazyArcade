using LoginServer.DB;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LoginServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        public AppDbContext Context { get; }

        public LoginController(AppDbContext context)
        {
            Context = context;
        }

        [HttpPost]
        [Route("register")]
        public async Task<RegisterPacketRes> Register([FromBody] RegisterPacketReq req)
        {
            RegisterPacketRes res = new RegisterPacketRes();

            await Context.Connection.OpenAsync();
            var query = new AccountQuery(Context);
            var result = await query.FindOneAsyncByNickname(req.Nickname);
            if (result is null)
            {
                AccountDb account = new AccountDb(Context);
                account.AccountName = req.AccountName;
                account.Password = req.Password;
                account.Nickname = req.Nickname;
                await account.InsertAsync();

                res.RegisterOk = true;
            }
            else
            {
                res.RegisterOk = false;
            }

            return res;
        }

        [HttpPost]
        [Route("login")]
        public async Task<LoginPacketRes> Login([FromBody] LoginPacketReq req)
        {
            LoginPacketRes res = new LoginPacketRes();

            await Context.Connection.OpenAsync();
            var query = new AccountQuery(Context);
            var result = await query.FindOneAsyncByAccountName(req.AccountName);
            if (result is null)
            {
                res.LoginOk = false;
            }
            else
            {
                if (result.Password != req.Password)
                {
                    res.LoginOk = false;
                }
                else
                {
                    res.LoginOk = true;

                    // TODO : 토큰 발급
                }
            }
            
            return res;
        }
    }

}
