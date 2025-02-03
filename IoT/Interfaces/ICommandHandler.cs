using IoT.Common;

namespace IoT.Interfaces
{
    public interface ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand
        where TResponse : class
    {
        public Task<Result<TResponse>> HandleAsync(TCommand command);
    }
}
