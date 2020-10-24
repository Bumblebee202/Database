using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Database.MySql
{
    public class MySQLCommand : IDatabaseCommand
    {
        readonly MySqlCommand _command;
        ITransaction _transaction;

        public MySQLCommand(string name, MySqlConnection connection, CommandType commandType)
        {
            _command = new MySqlCommand(name, connection)
            {
                CommandType = commandType
            };
        }

        public MySQLCommand(string name, MySqlConnection connection, CommandType commandType, ITransaction transaction)
            : this(name, connection, commandType) => Transaction = transaction;

        public DbCommand Command
        {
            get => _command;
        }

        public ITransaction Transaction
        {
            get => _transaction;
            set
            {
                _transaction = value;
                _command.Transaction = _transaction.Transaction as MySqlTransaction;
            }
        }

        public int CommandTimeout
        {
            get => _command.CommandTimeout;
            set => _command.CommandTimeout = value;
        }

        public string Text
        {
            get => _command.CommandText;
        }

        public async Task ExecuteNonQuery() => await _command.ExecuteNonQueryAsync();

        public async Task<IDatabaseReader> ExecuteReader()
        {
            DbDataReader reader = await _command.ExecuteReaderAsync();
            return new MySQLReader(reader);
        }

        public async Task<object> ExecuteScalar() => await _command.ExecuteScalarAsync();

        object ToDBNull<T>(T value)
        {
            if (value != null)
                return value;
            return DBNull.Value;
        }

        MySqlParameter CreateParameter<T>(string name)
        {
            DbType type = typeof(T).Name switch
            {
                "Byte" => DbType.Byte,
                "Int16" => DbType.Int16,
                "Int32" => DbType.Int32,
                "Int64" => DbType.Int64,
                "Single" => DbType.Single,
                "Double" => DbType.Double,
                "Decimal" => DbType.Decimal,
                "String" => DbType.String,
                "Boolean" => DbType.Boolean,
                "DateTime" => DbType.DateTime,
                _ => throw new Exception("Error type")
            };

            return new MySqlParameter
            {
                ParameterName = name,
                DbType = type
            };
        }

        public IDatabaseCommand InputParameter<T>(string name, T value)
        {
            MySqlParameter parameter = CreateParameter<T>(name);
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = ToDBNull(value);

            _command.Parameters.Add(parameter);
            return this;
        }

        public IDatabaseCommand InputParameter<T>(string name, T value, int size)
        {
            MySqlParameter parameter = CreateParameter<T>(name);
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = ToDBNull(value);
            parameter.Size = size;

            _command.Parameters.Add(parameter);
            return this;
        }

        public IDatabaseCommand OutputParameter<T>(string name)
        {
            MySqlParameter parameter = CreateParameter<T>(name);
            parameter.Direction = ParameterDirection.Output;

            _command.Parameters.Add(parameter);
            return this;
        }

        public T Value<T>(string paramName) => (T)_command.Parameters[paramName].Value;
    }
}
