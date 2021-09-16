﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Hackney.Core.Http
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Helper method to ensure that the application's DI container is used to inject 
        /// the required components to use IHttpContextWrapper.
        /// </summary>
        /// <param name="serviceCollection">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddHttpContextWrapper(this IServiceCollection serviceCollection)
        {
            if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.TryAddScoped<IHttpContextWrapper, HttpContextWrapper>();
            return serviceCollection;
        }
    }
}
