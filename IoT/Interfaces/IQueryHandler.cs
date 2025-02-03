using IoT.Common;

namespace IoT.Interfaces
{
    public interface IQueryHandler
    {
        public Task<Result<IResponse>> HandleAsync(IQuery request);
    }
}
