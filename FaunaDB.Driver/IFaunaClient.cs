using System.Threading.Tasks;

namespace FaunaDB.Driver
{
    public interface IFaunaClient
    {
        Task<RequestResult> Query(Expr query);
    }
}