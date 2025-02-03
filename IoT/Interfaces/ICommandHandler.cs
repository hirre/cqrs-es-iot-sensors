using IoT.Common;

namespace IoT.Interfaces
{
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        public Task<Result<IResponse>> HandleAsync(TCommand command);
    }
}
