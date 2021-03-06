﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Ystervark.Manager.Base;
using Ystervark.Models.DTO;
using Ystervark.Models.Enums;
using Ystervark.Models.Helper;
using Ystervark.Models.Interface;

namespace Ystervark.API.Controllers.Base
{
    /// <summary>
    /// Base Controller Class
    /// </summary>
    /// <typeparam name="TManager">The type of the manager.</typeparam>
    /// <seealso cref="Controller" />
    public abstract class BaseController<TManager> : Controller
        where TManager : class, IBaseManager
    {
        #region BaseController - Properties

        /// <summary>
        /// Gets the manager.
        /// </summary>
        /// <value>
        /// The manager.
        /// </value>
        protected TManager Manager { get; }

        /// <summary>
        /// Gets or sets the mapper.
        /// </summary>
        /// <value>
        /// The mapper.
        /// </value>
        public IMapper Mapper { get; set; }

        #endregion

        #region BaseController - CTOR

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController{TManager}" /> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        protected BaseController(TManager manager)
        {
            this.Manager = manager;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController{TManager}"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="mapper">The mapper.</param>
        protected BaseController(TManager manager, IMapper mapper)
        {
            this.Manager = manager;
            this.Mapper = mapper;
        }

        #endregion

        #region BaseController - Private Worker Methods

        /// <summary>
        /// Sets the active user.
        /// </summary>
        private void SetActiveUser()
        {
            this.Manager.ActiveResource = new ResourceModel
            {
                ResourceId = Convert.ToInt32(base.User.Claims.First(f => f.Type == YstervarkClaimNames.ResourceId).Value),
                ResourceName = base.User.Claims.First(f => f.Type == YstervarkClaimNames.ResourceName).Value,
                TenantId = Convert.ToInt32(base.User.Claims.First(f => f.Type == YstervarkClaimNames.TenantId).Value)
            };

            SetManagerSession(this.Manager.GetType(), this.Manager, this.Manager.ActiveResource);
        }

        /// <summary>
        /// Sets the manager session.
        /// </summary>
        /// <param name="managerType">Type of the class.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="resource">The resource.</param>
        private static void SetManagerSession(Type managerType, object instance, IResource resource)
        {
            foreach (var property in managerType.GetProperties())
            {
                object value;

                if (typeof(IActiveResource).IsAssignableFrom(property.PropertyType))
                {
                    // found a property that is an IActiveResource
                    value = property.GetValue(instance);
                    if (value is IActiveResource activeResource)
                    {
                        activeResource.ActiveResource = resource;
                        SetManagerSession(value.GetType(), value, resource);
                    }
                }
                if (!typeof(ITenant).IsAssignableFrom(property.PropertyType)) continue;

                // found a property that is an ITenant
                value = property.GetValue(instance);
                var tenant = value as ITenant;
                if (tenant == null) continue;
                tenant.TenantId = resource.TenantId;
                SetManagerSession(value.GetType(), value, resource);
            }
        }

        #endregion

        #region BaseController - Protected Methods

        /// <summary>
        /// Constructs the response asynchronous.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        protected async Task<IActionResult> ConstructResponseAsync(Func<Task> action)
        {
            this.SetActiveUser();
            await action();
            return Ok();
        }

        /// <summary>
        /// Constructs the response asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        protected async Task<IActionResult> ConstructResponseAsync<TResult>(Func<Task<TResult>> action)
        {
            this.SetActiveUser();
            var result = await action();
            var response = new Response<TResult>
            {
                IsSuccess = true,
                Message = string.Empty,
                Data = result
            };

            return Ok(response);
        }

        /// <summary>
        /// Constructs the response asynchronous.
        /// </summary>
        /// <typeparam name="TMap">The type to use when running <see cref="AutoMapper"/>.</typeparam>
        /// <typeparam name="TOutput">The type of the output returned by the delegate function.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        protected async Task<IActionResult> ConstructResponseAsync<TMap, TOutput>(Func<Task<TOutput>> action)
            where TMap : class where TOutput : class
        {
            this.SetActiveUser();
            var result = await action();
            var response = new Response<TMap>
            {
                IsSuccess = true,
                Message = string.Empty,
                Data = this.Mapper.Map<TMap>(result)
            };

            return Ok(response);
        }

        #endregion
    }
}