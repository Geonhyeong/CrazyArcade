using MySql.Data.MySqlClient;
using System.Data;
using System.Threading.Tasks;

namespace LoginServer.DB
{
    public class AccountDb
    {
        public int AccountDbId { get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }

        internal AppDbContext Context { get; set; }

        public AccountDb() { }
        
        internal AccountDb(AppDbContext context)
        {
            Context = context;
        }

        public async Task InsertAsync()
        {
            using var cmd = Context.Connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO `Account` (`AccountName`, `Password`, `Nickname`) VALUES (@accountname, @password, @nickname);";
            BindParams(cmd);
            await cmd.ExecuteNonQueryAsync();
            AccountDbId = (int)cmd.LastInsertedId;
        }

        public async Task UpdateAsync()
        {
            using var cmd = Context.Connection.CreateCommand();
            cmd.CommandText = @"UPDATE `Account` SET `AccountName` = @accountname, `Password` = @password, `Nickname` = @nickname WHERE `AccountDbId` = @accountdbid;";
            BindParams(cmd);
            BindId(cmd);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync()
        {
            using var cmd = Context.Connection.CreateCommand();
            cmd.CommandText = @"DELETE FROM `Account` WHERE `AccountDbId` = @accountdbid;";
            BindId(cmd);
            await cmd.ExecuteNonQueryAsync();
        }

        private void BindId(MySqlCommand cmd)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@accountdbid",
                DbType = DbType.Int32,
                Value = AccountDbId,
            });
        }

        private void BindParams(MySqlCommand cmd)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@accountname",
                DbType = DbType.String,
                Value = AccountName,
            });
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@password",
                DbType = DbType.String,
                Value = Password,
            });
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@nickname",
                DbType = DbType.String,
                Value = Nickname,
            });
        }

    }
}
