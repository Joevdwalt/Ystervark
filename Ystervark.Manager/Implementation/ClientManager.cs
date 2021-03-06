using System.Collections.Generic;
using System.Threading.Tasks;
using Ystervark.Database.Models;
using Ystervark.Manager.Base;
using Ystervark.Manager.Interface;
using Ystervark.Models.DTO;
using Ystervark.Repository.Extensions;
using Ystervark.Repository.Interface;

namespace Ystervark.Manager.Implementation
{
    /// <summary>
    /// Client Manager Class
    /// </summary>
    /// <seealso cref="Ystervark.Manager.Base.BaseManager" />
    /// <seealso cref="Ystervark.Manager.Interface.IClientManager" />
    public class ClientManager : BaseManager, IClientManager
    {
        #region ClientManager - Repository Properties

        /// <summary>
        /// Gets the client repository.
        /// </summary>
        /// <value>
        /// The client repository.
        /// </value>
        public IRepository<Client> ClientRepository => base.UnitOfWork.GetRepository<Client>();

        #endregion

        #region Implementation of IClientManager

        /// <summary>
        /// Gets the client data.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ClientModel>> GetClientData()
        {
            var dbResponse = await this.ClientRepository.GetPagedListAsync(null, null, null, 1, 25);
            return base.Mapper.Map<IEnumerable<ClientModel>>(dbResponse.Items);
        }

        /// <summary>
        /// Gets the client data in a summarized collection.
        /// </summary>
        /// <param name="archived">if set to <c>true</c> archived clients will be included; otherwise <c>false</c>.</param>
        /// <returns></returns>
        public async Task<IEnumerable<ClientSummaryModel>> GetAsSummary(bool? archived = null) => await this.ClientRepository
            .LoadStoredProcedure("dbo.GetClientsAsSummary")
            .WithSqlParam("TenantId", base.TenantId)
            .WithSqlParam("Archived", archived)
            .ExecuteStoredProcAsync<ClientSummaryModel>();

        #endregion
    }
}