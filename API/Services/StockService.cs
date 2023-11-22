
using API.Models;

namespace API.Services
{
    public class StockService
    {
        public Response GetPrice(string TKR)
        {
            return new Response { Result = 10.0, Status = Status.Successful };
            throw new NotImplementedException();
        }
    }
}
