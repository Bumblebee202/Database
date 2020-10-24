using Database;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace MsSQL
{
    public class MsSQLReader : IDatabaseReader, IDisposable
    {
        bool _disposed;
        DbDataReader _reader;

        public MsSQLReader(DbDataReader reader)
        {
            _disposed = false;
            _reader = reader;
        }

        public bool HasRows => _reader.HasRows;

        public object this[string name] => _reader[name];

        public bool IsDBNull(string name)
        {
            int ordinal = GetOrdinal(name);
            return _reader.IsDBNull(ordinal);
        }

        public string GetColumnName(int ordinal) => _reader.GetName(ordinal);

        public int GetOrdinal(string columnName) => _reader.GetOrdinal(columnName);

        public T Value<T>(string name) => (T)_reader[name];

        public T Value<T>(int ordinal) => _reader.GetFieldValue<T>(ordinal);

        public Task<bool> Read() => _reader.ReadAsync();

        public void Close() => Dispose(false);

        protected async virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                await _reader.DisposeAsync();
                _reader = null;
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
