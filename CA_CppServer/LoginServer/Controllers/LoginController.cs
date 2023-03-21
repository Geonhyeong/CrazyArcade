using LoginServer.DB;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace LoginServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly string connectionString = "localhost:6379";

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
            var result = await query.FindOneAsyncByAccountName(req.AccountName);
            if (result is null)
            {
                AccountDb account = new AccountDb(Context);
                account.AccountName = req.AccountName;
                account.Password = req.Password;
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

                    // 토큰 생성 및 RedisDB에 저장
                    DateTime expired = DateTime.UtcNow;
                    expired.AddSeconds(600);

                    RedisManager redisManager = new RedisManager(connectionString);
                    IDatabase db = redisManager.GetDatabase();

                    string json = await db.StringGetAsync(result.AccountDbId.ToString());
                    RedisToken redisToken;
                    if (json != null)
                    {
                        redisToken = JsonConvert.DeserializeObject<RedisToken>(json);
                        redisToken.Token = new Random().Next(Int32.MinValue, Int32.MaxValue);
                        redisToken.Expired = expired.Ticks;
                        await db.StringSetAsync(result.AccountDbId.ToString(), JsonConvert.SerializeObject(redisToken));
                    }
                    else
                    {
                        redisToken = new RedisToken()
                        {
                            AccountDbId = result.AccountDbId,
                            Token = new Random().Next(Int32.MinValue, Int32.MaxValue),
                            Expired = expired.Ticks
                        };
                        await db.StringSetAsync(result.AccountDbId.ToString(), JsonConvert.SerializeObject(redisToken));
                    }

                    res.AccountId = result.AccountDbId;
                    res.Token = redisToken.Token;
                }
            }
            
            return res;
        }
    }

}
