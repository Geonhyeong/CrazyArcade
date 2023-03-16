using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace LoginServer.DB
{
    public class AccountQuery
    {
        public AppDbContext Context { get; }

        public AccountQuery(AppDbContext context)
        {
            Context = context;
        }

        public async Task<AccountDb> FindOneAsync(string accountName)
        {
            using var cmd = Context.Connection.CreateCommand();
            cmd.CommandText = @"SELECT `AccountDbId`, `AccountName`, `Password` FROM `Account` WHERE `AccountName` = @accountname";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@accountname",
                DbType = DbType.String,
                Value = accountName,
            });
            var result = await ReadAllAsync(await cmd.ExecuteReaderAsync());
            return result.Count > 0 ? result[0] : null;
        }

        public async Task DeleteAllAsync()
        {
            using var txn = await Context.Connection.BeginTransactionAsync();
            using var cmd = Context.Connection.CreateCommand();
            cmd.CommandText = @"DELETE FROM `Account`";
            await cmd.ExecuteNonQueryAsync();
            await txn.CommitAsync();
        }

        private async Task<List<AccountDb>> ReadAllAsync(DbDataReader reader)
        {
            var accounts = new List<AccountDb>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var account = new AccountDb(Context)
                    {
                        AccountDbId = reader.GetInt32(0),
                        AccountName = reader.GetString(1),
                        Password = reader.GetString(2),
                    };
                    accounts.Add(account);
                }
            }
            return accounts;
        }
    }
}
