using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Database.MsSQL
{
    public class MsSQLTransaction : ITransaction
    {
        public MsSQLTransaction(SqlConnection connection) => Start(connection);

        public DbTransaction Transaction { get; private set; }

        async void Start(SqlConnection connection) => Transaction = await connection.BeginTransactionAsync();

        public async Task Commit() => await Transaction.CommitAsync();

        public async Task Rollback() => await Transaction.RollbackAsync();
    }
}
