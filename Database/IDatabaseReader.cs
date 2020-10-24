using System.Threading.Tasks;

namespace Database
{
    public interface IDatabaseReader
    {
        bool HasRows { get; }
        bool IsDBNull(string name);
        string GetColumnName(int ordinal);
        int GetOrdinal(string columnName);
        T Value<T>(string name);
        T Value<T>(int ordinal);
        object this[string name] { get; }
        Task<bool> Read();
        void Close();
    }
}
