using System.Data.Common;
using System.Threading.Tasks;

namespace Database
{
    public interface IDatabaseCommand
    {
        DbCommand Command { get; }
        ITransaction Transaction { get; set; }
        int CommandTimeout { get; set; }
        string Text { get; }

        Task ExecuteNonQuery();
        Task<IDatabaseReader> ExecuteReader();
        Task<object> ExecuteScalar();
        IDatabaseCommand OutputParameter<T>(string name);
        IDatabaseCommand InputParameter<T>(string name, T value);
        IDatabaseCommand InputParameter<T>(string name, T value, int size);
        T Value<T>(string paramName);
    }
}
