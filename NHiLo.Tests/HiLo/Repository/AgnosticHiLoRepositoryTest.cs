﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHiLo.HiLo.Repository;
using NHiLo.HiLo.Config;
using System.Data.Common;
using Moq;
using System.Data;
using NHiLo.Tests.TestDoubles;

namespace NHiLo.Tests.HiLo.Repository
{
    public class AgnosticHiLoRepositoryTest
    {
        private class TestableHiLoRepository : AgnosticHiLoRepository
        {
            public TestableHiLoRepository(string entityName, IHiLoConfiguration config)
                : base(entityName, config)
            {
            }

            internal int CallsToGetNextHiFromDatabase { get; private set; }
            internal int CallsToCreateRepositoryStructure { get; private set; }
            internal int CallsToInitializeRepositoryForEntity { get; private set; }

            protected override long GetNextHiFromDatabase(IDbCommand cmd)
            {
                CallsToGetNextHiFromDatabase++;
                return 0;
            }

            protected override void CreateRepositoryStructure(IDbCommand cmd)
            {
                CallsToCreateRepositoryStructure++;
            }

            protected override void InitializeRepositoryForEntity(IDbCommand cmd)
            {
                CallsToInitializeRepositoryForEntity++;
            }
        }

        [TestClass]
        public class PrepareRepository
        {
            private Mock<IHiLoConfiguration> SetupConfiguration(Mock<IHiLoConfiguration> mock, bool createHiLoStructureIfNotExists)
            {
                mock.SetupGet(p => p.CreateHiLoStructureIfNotExists).Returns(createHiLoStructureIfNotExists);
                mock.SetupGet(p => p.ConnectionString).Returns("Valid Config");
                return mock;
            }

            private Mock<DbProviderFactory> SetupDbProvider(Mock<DbProviderFactory> mock)
            {
                var mockConnection = new DbConnectionMock();
                mock.Setup(m => m.CreateConnection()).Returns(mockConnection);
                return mock;
            }

            private TestableHiLoRepository CreateSystemUnderTest(Mock<IHiLoConfiguration> mockConfig, Mock<DbProviderFactory> mockDbProviderFactory)
            {
                var target = new TestableHiLoRepository("", mockConfig.Object) { DbFactoryCreator = (s) => mockDbProviderFactory.Object };
                return target;
            }

            [TestMethod]
            public void ShouldInvokeTheInitializeRepositoryMethodWithoutCreatingRepositoryStructure()
            {
                // Arrange
                var mockConfig = new Mock<IHiLoConfiguration>();
                mockConfig = SetupConfiguration(mockConfig, false);
                var mockDbProviderFactory = new Mock<DbProviderFactory>();
                mockDbProviderFactory = SetupDbProvider(mockDbProviderFactory);
                var target = CreateSystemUnderTest(mockConfig, mockDbProviderFactory);
                // Act
                target.PrepareRepository();
                // Assert
                Assert.AreEqual(1, target.CallsToInitializeRepositoryForEntity);
                Assert.AreEqual(0, target.CallsToCreateRepositoryStructure);
            }

            [TestMethod]
            public void ShouldInvokeTheInitializeRepositoryMethodCreatingRepositoryStructure()
            {
                // Arrange
                var mockConfig = new Mock<IHiLoConfiguration>();
                mockConfig = SetupConfiguration(mockConfig, true);
                var mockDbProviderFactory = new Mock<DbProviderFactory>();
                mockDbProviderFactory = SetupDbProvider(mockDbProviderFactory);
                var target = CreateSystemUnderTest(mockConfig, mockDbProviderFactory);
                // Act
                target.PrepareRepository();
                // Assert
                Assert.AreEqual(1, target.CallsToInitializeRepositoryForEntity);
                Assert.AreEqual(1, target.CallsToCreateRepositoryStructure);
            }

        }
    }
}
