﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using IContainer = Autofac.IContainer;

namespace GoonsOnAir
{
    /// <summary>
    ///     A strongly-typed version of Caliburn.Micro.Bootstrapper that specifies the type of root model to create for the
    ///     application.
    /// </summary>
    public class AutofacBootstrapper : BootstrapperBase
    {
        #region Properties

        protected IContainer Container { get; private set; }

        /// <summary>
        ///     Should the namespace convention be enforced for type registration. The default is true.
        ///     For views, this would require a views namespace to end with Views
        ///     For view-models, this would require a view models namespace to end with ViewModels
        ///     <remarks>Case is important as views would not match.</remarks>
        /// </summary>
        public bool EnforceNamespaceConvention { get; set; }

        /// <summary>
        ///     The base type required for a view model
        /// </summary>
        public Type ViewModelBaseType { get; set; }

        /// <summary>
        ///     Method for creating the window manager
        /// </summary>
        public Func<IWindowManager> CreateWindowManager { get; set; }

        /// <summary>
        ///     Method for creating the event aggregator
        /// </summary>
        public Func<IEventAggregator> CreateEventAggregator { get; set; }

        #endregion

        /// <summary>
        ///     Do not override this method. This is where the IoC container is configured.
        ///     <remarks>
        ///         Will throw <see cref="System.ArgumentNullException" /> is either CreateWindowManager
        ///         or CreateEventAggregator is null.
        ///     </remarks>
        /// </summary>
        protected override void Configure()
        {
            //  allow base classes to change bootstrapper settings
            ConfigureBootstrapper();

            //  validate settings
            if (CreateWindowManager == null)
            {
                throw new ArgumentNullException(nameof(CreateWindowManager));
            }

            if (CreateEventAggregator == null)
            {
                throw new ArgumentNullException(nameof(CreateEventAggregator));
            }

            //  configure container
            var builder = new ContainerBuilder();

            //  register view models
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                //  must be a type with a name that ends with ViewModel
                .Where(type => type.Name.EndsWith("ViewModel"))
                //  must be in a namespace ending with ViewModels
                .Where(type => !EnforceNamespaceConvention || !string.IsNullOrWhiteSpace(type.Namespace) && type.Namespace.EndsWith("ViewModels"))
                //  must implement INotifyPropertyChanged (deriving from PropertyChangedBase will statisfy this)
                .Where(type => type.GetInterface(ViewModelBaseType.Name, false) != null)
                //  registered as self
                .AsSelf()
                //  always create a new one
                .InstancePerDependency()
                .PropertiesAutowired();

            //  register views
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                //  must be a type with a name that ends with View
                .Where(type => type.Name.EndsWith("View"))
                //  must be in a namespace that ends in Views
                .Where(type => !EnforceNamespaceConvention || !string.IsNullOrWhiteSpace(type.Namespace) && type.Namespace.EndsWith("Views"))
                //  registered as self
                .AsSelf()
                //  always create a new one
                .InstancePerDependency()
                .PropertiesAutowired();

            //  register the single window manager for this container
            builder.Register(c => CreateWindowManager())
                .InstancePerLifetimeScope();
            //  register the single event aggregator for this container
            builder.Register(c => CreateEventAggregator())
                .InstancePerLifetimeScope();

            //  allow derived classes to add to the container
            ConfigureContainer(builder);

            Container = builder.Build();
        }

        /// <summary>
        ///     Do not override unless you plan to full replace the logic. This is how the framework
        ///     retrieves services from the Autofac container.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <param name="key">The key to locate.</param>
        /// <returns>The located service.</returns>
        protected override object GetInstance(Type service, string key)
        {
            object instance;
            if (string.IsNullOrWhiteSpace(key))
            {
                if (Container.TryResolve(service, out instance))
                {
                    return instance;
                }
            }
            else
            {
                if (Container.TryResolveNamed(key, service, out instance))
                {
                    return instance;
                }
            }

            throw new Exception($"Could not locate any instances of contract {key ?? service.Name}.");
        }

        /// <summary>
        ///     Do not override unless you plan to full replace the logic. This is how the framework
        ///     retrieves services from the Autofac container.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <returns>The located services.</returns>
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return Container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        /// <summary>
        ///     Do not override unless you plan to full replace the logic. This is how the framework
        ///     retrieves services from the Autofac container.
        /// </summary>
        /// <param name="instance">The instance to perform injection on.</param>
        protected override void BuildUp(object instance)
        {
            Container.InjectProperties(instance);
        }

        /// <summary>
        ///     Override to provide configuration prior to the Autofac configuration. You must call the base version BEFORE any
        ///     other statement or the behaviour is undefined.
        ///     Current Defaults:
        ///     EnforceNamespaceConvention = true
        ///     ViewModelBaseType = <see cref="System.ComponentModel.INotifyPropertyChanged" />
        ///     CreateWindowManager = <see cref="Caliburn.Micro.WindowManager" />
        ///     CreateEventAggregator = <see cref="Caliburn.Micro.EventAggregator" />
        /// </summary>
        protected virtual void ConfigureBootstrapper()
        {
            //  by default, enforce the namespace convention
            EnforceNamespaceConvention = true;
            //  the default view model base type
            ViewModelBaseType = typeof(INotifyPropertyChanged);
            //  default window manager
            CreateWindowManager = () => new WindowManager();
            //  default event aggregator
            CreateEventAggregator = () => new EventAggregator();
        }

        /// <summary>
        ///     Override to include your own Autofac configuration after the framework has finished its configuration, but
        ///     before the container is created.
        /// </summary>
        /// <param name="builder">The Autofac configuration builder.</param>
        protected virtual void ConfigureContainer(ContainerBuilder builder)
        {
        }
    }
}