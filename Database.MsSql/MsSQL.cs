using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Database.MsSQL
{
    class MsSQL : IDatabase, IDisposable
    {
        bool _disposed;
        string _connectionString;
        SqlConnection _connection;

        private MsSQL() => _disposed = false;

        public string DatabaseName => _connection?.Database;

        public string DataSource => _connection?.DataSource;

        public int ConnectionTimeout => _connection != null ? _connection.ConnectionTimeout : -1;

        public string ServerVersion => _connection?.ServerVersion;

        public ConnectionState State => _connection.State;

        public void SetConnectionString(string connectionString)
        {
            Dispose(false);
            _connectionString = connectionString;
        }

        public async Task OpenConnection()
        {
            _connection = new SqlConnection(_connectionString);
            await _connection.OpenAsync();
        }

        public void CloseConnection() => Dispose(true);

        public ITransaction BeginTransaction() => new MsSQLTransaction(_connection);

        public Task EndTransaction(ITransaction transaction, bool rollback)
        {
            return Task.Run(() =>
            {
                if (!rollback)
                    transaction.Commit();
                else
                    transaction.Rollback();
            });
        }

        public IDatabaseCommand Procedure(string name) => new MsSQLCommand(name, _connection, CommandType.StoredProcedure);

        public IDatabaseCommand Procedure(string name, ITransaction transaction)
            => new MsSQLCommand(name, _connection, CommandType.StoredProcedure, transaction);

        public IDatabaseCommand Query(string text) => new MsSQLCommand(text, _connection, CommandType.Text);

        public IDatabaseCommand Query(string text, ITransaction transaction)
            => new MsSQLCommand(text, _connection, CommandType.Text, transaction);

        public async Task Execute(IDatabaseCommand command) => await command.ExecuteNonQuery();

        public async Task<T> Execute<T>(IDatabaseCommand command, Func<IDatabaseCommand, T> creator)
        {
            await command.ExecuteNonQuery();

            return creator(command);
        }

        public async Task<IEnumerable<T>> Fill<T>(IDatabaseCommand command, Func<IDatabaseReader, T> selector)
        {
            await command.ExecuteNonQuery();

            List<T> ts = new List<T>();

            IDatabaseReader reader = await command.ExecuteReader();

            while (await reader.Read())
            {
                T t = selector(reader);
                ts.Add(t);
            }

            reader.Close();


            return ts;
        }

        protected async virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MsSQL() => Dispose(false);
    }
}
