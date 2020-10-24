using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Database.MsSQL
{
    public class MsSQLCommand : IDatabaseCommand
    {
        readonly SqlCommand _command;
        ITransaction _transaction;

        public MsSQLCommand(string name, SqlConnection connection, CommandType commandType)
        {
            _command = new SqlCommand(name, connection)
            {
                CommandType = commandType
            };
        }

        public MsSQLCommand(string name, SqlConnection connection, CommandType commandType, ITransaction transaction)
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
                _command.Transaction = _transaction.Transaction as SqlTransaction;
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
            return new MsSQLReader(reader);
        }

        public async Task<object> ExecuteScalar() => await _command.ExecuteScalarAsync();

        object ToDBNull<T>(T value)
        {
            if (value != null)
                return value;
            return DBNull.Value;
        }

        SqlParameter CreateParameter<T>(string name)
        {
            SqlDbType type = typeof(T).Name switch
            {
                "Byte" => SqlDbType.TinyInt,
                "Int16" => SqlDbType.SmallInt,
                "Int32" => SqlDbType.Int,
                "Int64" => SqlDbType.BigInt,
                "Single" => SqlDbType.Real,
                "Double" => SqlDbType.Float,
                "Decimal" => SqlDbType.Decimal,
                "String" => SqlDbType.NVarChar,
                "Boolean" => SqlDbType.Bit,
                "DateTime" => SqlDbType.DateTime,
                _ => throw new Exception("Error type")
            };

            return new SqlParameter
            {
                ParameterName = $"@{name}",
                SqlDbType = type,
            };
        }

        public IDatabaseCommand OutputParameter<T>(string name)
        {
            SqlParameter parameter = CreateParameter<T>(name);
            parameter.Direction = ParameterDirection.Output;

            _command.Parameters.Add(parameter);
            return this;
        }

        public IDatabaseCommand InputParameter<T>(string name, T value)
        {
            SqlParameter parameter = CreateParameter<T>(name);
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = ToDBNull(value);

            _command.Parameters.Add(parameter);
            return this;
        }

        public IDatabaseCommand InputParameter<T>(string name, T value, int size)
        {
            SqlParameter parameter = CreateParameter<T>(name);
            parameter.Direction = ParameterDirection.Input;
            parameter.Value = ToDBNull(value);
            parameter.Size = size;

            _command.Parameters.Add(parameter);
            return this;
        }

        public T Value<T>(string paramName) => (T)_command.Parameters[$"@{paramName}"].Value;
    }
}
