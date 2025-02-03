using IoT.Common;

namespace IoT.Interfaces
{
    public interface ICommandHandler
    {
        public Task<Result<IResponse>> HandleAsync(ICommand command);
    }
}
