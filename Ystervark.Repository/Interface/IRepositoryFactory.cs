﻿namespace Ystervark.Repository.Interface
{
    /// <summary>
    /// Defines the interfaces for <see cref="IRepository{TEntity}"/> interfaces.
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// Gets the specified repository for the <typeparamref name="TEntity" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <returns>
        /// An instance of type inherited from <see cref="IRepository{TEntity}" /> interface.
        /// </returns>
        IRepository<TEntity> GetRepository<TEntity>(int? tenantId = null) where TEntity : class;
    }
}