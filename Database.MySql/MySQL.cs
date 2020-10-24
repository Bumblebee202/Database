using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Database.MySql
{
    public class MySQL : IDatabase, IDisposable
    {
        bool _disposed;
        string _connectionString;
        MySqlConnection _connection;

        public MySQL() => _disposed = false;

        public MySQL(string connectionString)
        {
            _disposed = false;
            _connectionString = connectionString;
        }

        public string DatabaseName => _connection?.Database;

        public string DataSource => _connection?.DataSource;

        public int ConnectionTimeout => _connection != null ? _connection.ConnectionTimeout : -1;

        public string ServerVersion => _connection?.ServerVersion;

        public ConnectionState State => _connection.State;

        public void SetConnectionString(string connectionString)
        {
            Dispose(true);
            _connectionString = connectionString;
        }

        public async Task OpenConnection()
        {
            _connection = new MySqlConnection(_connectionString);
            await _connection.OpenAsync();
        }

        public void CloseConnection() => Dispose(true);

        public ITransaction BeginTransaction() => new MySQLTransaction(_connection);

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

        public IDatabaseCommand Procedure(string name) => new MySQLCommand(name, _connection, CommandType.StoredProcedure);

        public IDatabaseCommand Procedure(string name, ITransaction transaction)
            => new MySQLCommand(name, _connection, CommandType.StoredProcedure, transaction);

        public IDatabaseCommand Query(string text) => new MySQLCommand(text, _connection, CommandType.Text);

        public IDatabaseCommand Query(string text, ITransaction transaction)
            => new MySQLCommand(text, _connection, CommandType.Text, transaction);

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

        ~MySQL() => Dispose(false);
    }
}
