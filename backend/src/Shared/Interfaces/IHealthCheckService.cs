using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reembolso.Shared.DTOs;

namespace Reembolso.Shared.Interfaces
{
    public interface IHealthCheckService
    {
        /// <summary>
        /// Executa verificação completa de saúde do serviço
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resultado da verificação de saúde</returns>
        Task<HealthCheckResultDto> CheckHealthAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica a conectividade com o banco de dados
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resultado da verificação do banco de dados</returns>
        Task<IndividualHealthCheckDto> CheckDatabaseAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica dependências externas (outros microserviços, Redis, etc.)
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de verificações de dependências externas</returns>
        Task<List<IndividualHealthCheckDto>> CheckExternalDependenciesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica recursos do sistema (memória, CPU, disco)
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de verificações de recursos do sistema</returns>
        Task<List<IndividualHealthCheckDto>> CheckSystemResourcesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica serviços internos específicos do microserviço
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de verificações de serviços internos</returns>
        Task<List<IndividualHealthCheckDto>> CheckInternalServicesAsync(CancellationToken cancellationToken = default);
    }
}