using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Threading.Tasks;

namespace Database.MySql
{
    public class MySQLTransaction : ITransaction
    {
        public MySQLTransaction(MySqlConnection connection) => Start(connection);

        public DbTransaction Transaction { get; private set; }

        async void Start(MySqlConnection connection) => Transaction = await connection.BeginTransactionAsync();

        public async Task Commit() => await Transaction.CommitAsync();

        public async Task Rollback() => await Transaction.RollbackAsync();
    }
}
