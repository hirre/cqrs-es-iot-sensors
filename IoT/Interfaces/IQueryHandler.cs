using IoT.Common;

namespace IoT.Interfaces
{
    public interface IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery
        where TResponse : class
    {
        public Task<Result<TResponse>> HandleAsync(TQuery query);
    }
}
