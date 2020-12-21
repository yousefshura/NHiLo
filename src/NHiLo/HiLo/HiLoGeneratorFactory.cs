﻿using Microsoft.Extensions.Configuration;
using NHiLo.Common;
using NHiLo.HiLo;
using NHiLo.HiLo.Config;
using NHiLo.HiLo.Repository;
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace NHiLo // this should be available at the root namespace
{
    /// <summary>
    /// Factory that creates <see cref="IKeyGeneratorFactory"/> for client usage.
    /// </summary>
    public class HiLoGeneratorFactory : IKeyGeneratorFactory<long>
    {
        // When instantiated, key generators are stored in a static field. That's how NHilo keeps the id generation globally per AppDomain.
        private readonly static ConcurrentDictionary<string, IKeyGenerator<long>> _keyGenerators = new ConcurrentDictionary<string, IKeyGenerator<long>>();
        private readonly IHiLoRepositoryFactory _repositoryFactory;
        private readonly IHiLoConfiguration _config;
        private static readonly Regex _entityNameValidator = new Regex(@"^[a-zA-Z]+[a-zA-Z0-9]*$");

        public HiLoGeneratorFactory(IConfiguration configuration) :
            this(null, configuration)
        {

        }

        private HiLoGeneratorFactory(IHiLoRepositoryFactory repositoryFactory, IConfiguration configuration)
        {
            if (configuration == null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables();
                configuration = builder.Build();
            }
            _config = new HiLoConfigurationBuilder(new ConfigurationManagerWrapper(configuration)).Build();
            _repositoryFactory = repositoryFactory ?? new HiLoRepositoryFactory();
        }

        /// <summary>
        /// Gets the object which generates new unique keys for a giben entity name.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public IKeyGenerator<long> GetKeyGenerator(string entityName)
        {
            EnsureCorrectEntityName(entityName);
            lock (_keyGenerators)
            {
                if (!_keyGenerators.ContainsKey(entityName))
                    _keyGenerators.TryAdd(entityName, CreateKeyGenerator(entityName));
                return _keyGenerators[entityName];
            }
        }

        private void EnsureCorrectEntityName(string entityName)
        {
            if (!_entityNameValidator.IsMatch(entityName) || entityName.Length > Constants.MAX_LENGTH_ENTITY_NAME)
            {
                throw new NHiloException(ErrorCodes.InvalidEntityName);
            }
        }

        private IKeyGenerator<long> CreateKeyGenerator(string entityName)
        {
            var entityConfig = _config.GetEntityConfig(entityName);
            var repository = _repositoryFactory.GetRepository(entityName, _config);
            return new HiLoGenerator(repository, entityConfig.MaxLo);
        }
    }
}
