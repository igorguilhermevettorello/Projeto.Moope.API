using MediatR;

namespace Projeto.Moope.Core.Commands.Base
{
    /// <summary>
    /// Interface base para Commands que não retornam resultado
    /// </summary>
    public interface ICommand : IRequest
    {
    }

    /// <summary>
    /// Interface base para Commands que retornam resultado
    /// </summary>
    /// <typeparam name="TResponse">Tipo do resultado retornado</typeparam>
    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }
}
