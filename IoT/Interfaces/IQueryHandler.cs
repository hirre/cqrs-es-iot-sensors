using IoT.Common;

namespace IoT.Interfaces
{
    public interface IQueryHandler<TQuery> where TQuery : IQuery
    {
        public Task<Result<IResponse>> HandleAsync(TQuery query);
    }
}
