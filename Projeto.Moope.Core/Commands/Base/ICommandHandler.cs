using MediatR;

namespace Projeto.Moope.Core.Commands.Base
{
    /// <summary>
    /// Interface base para Handlers de Commands que não retornam resultado
    /// </summary>
    /// <typeparam name="TCommand">Tipo do Command</typeparam>
    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
        where TCommand : ICommand
    {
    }

    /// <summary>
    /// Interface base para Handlers de Commands que retornam resultado
    /// </summary>
    /// <typeparam name="TCommand">Tipo do Command</typeparam>
    /// <typeparam name="TResponse">Tipo do resultado retornado</typeparam>
    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
    }
}
