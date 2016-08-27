﻿using System;
using System.ComponentModel.Composition.Hosting;
using AppsTracker.Data.Service;

namespace AppsTracker.ServiceLocation
{
    public sealed class ServiceLocator : IServiceResolver
    {
        private static Lazy<ServiceLocator> instance = new Lazy<ServiceLocator>(() => new ServiceLocator());
        public static ServiceLocator Instance
        {
            get { return instance.Value; }
        }

        private ExportProvider container;

        private ServiceLocator()
        {
        }

        public void Initialize(ExportProvider container)
        {
            this.container = container;
        }

        public T Resolve<T>() where T : IBaseService
        {
            if (container == null)
                throw new InvalidOperationException("Container not initialized");

            return container.GetExportedValue<T>();
        }

        public T Resolve<T>(string contract) where T : IBaseService
        {
            if (container == null)
                throw new InvalidOperationException("Container not initialized");

            return container.GetExportedValue<T>(contract);
        }
    }
}
