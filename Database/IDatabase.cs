using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Database
{
    public interface IDatabase
    {
        void SetConnectionString(string connectionString);
        Task OpenConnection();
        void CloseConnection();
        ITransaction BeginTransaction();
        Task EndTransaction(ITransaction transaction, bool rollback);
        IDatabaseCommand Procedure(string name);
        IDatabaseCommand Procedure(string name, ITransaction transaction);
        IDatabaseCommand Query(string text);
        IDatabaseCommand Query(string text, ITransaction transaction);
        Task Execute(IDatabaseCommand command);
        Task<T> Execute<T>(IDatabaseCommand command, Func<IDatabaseCommand, T> creator);
        Task<IEnumerable<T>> Fill<T>(IDatabaseCommand command, Func<IDatabaseReader, T> selector);

        string DatabaseName { get; }
        string DataSource { get; }
        string ServerVersion { get; }
        int ConnectionTimeout { get; }
        ConnectionState State { get; }
    }
}
