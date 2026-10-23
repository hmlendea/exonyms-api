using System.Linq;
using System.Text.RegularExpressions;

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;

using NUnit.Framework;

using ExonymsAPI.Service;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ExonymsAPI.ArchitectureTests
{
    [TestFixture]
    public sealed class DependencyTests
    {
        private static readonly Architecture ProductionArchitecture = new ArchLoader()
            .LoadAssemblies(typeof(ExonymsService).Assembly)
            .Build();

        [TestCase("ExonymsAPI.API", "ExonymsAPI.Service.Gatherers")]
        [TestCase("ExonymsAPI.API", "ExonymsAPI.Service.Processors")]
        [TestCase("ExonymsAPI.API", "ExonymsAPI.Client")]
        [TestCase("ExonymsAPI.Service", "ExonymsAPI.API")]
        [TestCase("ExonymsAPI.Client", "ExonymsAPI.API")]
        [TestCase("ExonymsAPI.Client", "ExonymsAPI.Service")]
        [TestCase("ExonymsAPI.Service.Gatherers", "ExonymsAPI.API")]
        [TestCase("ExonymsAPI.Service.Processors", "ExonymsAPI.API")]
        [TestCase("ExonymsAPI.Service.Processors", "ExonymsAPI.Client")]
        [TestCase("ExonymsAPI.Service.Processors", "ExonymsAPI.Configuration")]
        [TestCase("ExonymsAPI.Service.Processors", "ExonymsAPI.Logging")]
        [TestCase("ExonymsAPI.Service.Processors", "ExonymsAPI.Service.Models")]
        [TestCase("ExonymsAPI.Service.Processors", "ExonymsAPI.Service.Gatherers")]
        [TestCase("ExonymsAPI.Service.Models", "ExonymsAPI.API")]
        [TestCase("ExonymsAPI.Service.Models", "ExonymsAPI.Client")]
        [TestCase("ExonymsAPI.Service.Models", "ExonymsAPI.Configuration")]
        [TestCase("ExonymsAPI.Service.Models", "ExonymsAPI.Logging")]
        [TestCase("ExonymsAPI.Service.Models", "ExonymsAPI.Service.Gatherers")]
        [TestCase("ExonymsAPI.Service.Models", "ExonymsAPI.Service.Processors")]
        public void GivenComponents_WhenCheckingDependencies_ThenBoundariesArePreserved(
            string sourceNamespace,
            string forbiddenNamespace)
            => AssertBoundary(sourceNamespace, forbiddenNamespace, true);

        [TestCase("ExonymsAPI.Service.Gatherers", "ExonymsAPI.Service", false)]
        [TestCase("ExonymsAPI.Service.Processors", "ExonymsAPI.Service", false)]
        [TestCase("ExonymsAPI.Service.Models", "ExonymsAPI.Service", false)]
        public void GivenInnerComponents_WhenCheckingOrchestration_ThenDependenciesPointInward(
            string sourceNamespace,
            string forbiddenNamespace,
            bool includeForbiddenSubnamespaces)
            => AssertBoundary(sourceNamespace, forbiddenNamespace, includeForbiddenSubnamespaces);

        [TestCase("ExonymsAPI.Service", "System.Net.Http")]
        [TestCase("ExonymsAPI.Service", "Microsoft.AspNetCore")]
        [TestCase("ExonymsAPI.Service.Processors", "System.Net")]
        [TestCase("ExonymsAPI.Service.Processors", "System.IO")]
        [TestCase("ExonymsAPI.Service.Models", "Microsoft.AspNetCore")]
        [TestCase("ExonymsAPI.Service.Models", "Microsoft.EntityFrameworkCore")]
        [TestCase("ExonymsAPI.Service.Models", "Amazon.DynamoDBv2")]
        [TestCase("ExonymsAPI.Service.Models", "Dapper")]
        [TestCase("ExonymsAPI.Service.Models", "MediatR")]
        [TestCase("ExonymsAPI.Service", "Microsoft.EntityFrameworkCore")]
        [TestCase("ExonymsAPI.Service", "Amazon.DynamoDBv2")]
        [TestCase("ExonymsAPI.API", "Microsoft.EntityFrameworkCore")]
        [TestCase("ExonymsAPI.API", "Amazon.DynamoDBv2")]
        public void GivenComponents_WhenCheckingExternalDependencies_ThenResponsibilitiesArePreserved(
            string sourceNamespace,
            string forbiddenNamespace)
        {
            IObjectProvider<IType> sourceTypes = SelectNamespace(sourceNamespace, true);

            if (string.Equals(sourceNamespace, "ExonymsAPI.Service"))
            {
                sourceTypes = SelectNamespace(sourceNamespace, false);
            }

            Assert.That(
                sourceTypes.GetObjects(ProductionArchitecture),
                Is.Not.Empty,
                $"The {sourceNamespace} boundary must select production types.");

            Types().That().Are(sourceTypes).Should().NotDependOnAny(
                SelectNamespace(forbiddenNamespace, true))
                .Because($"{sourceNamespace} must not use {forbiddenNamespace} directly")
                .Check(ProductionArchitecture);
        }

        [TestCase("ExonymsAPI.Service.ExonymsService")]
        [TestCase("ExonymsAPI.Service.Gatherers.GeoNamesGatherer")]
        [TestCase("ExonymsAPI.Service.Gatherers.WikiDataGatherer")]
        [TestCase("ExonymsAPI.Client.TransliterationAPI.TransliterationApiClient")]
        public void GivenImplementations_WhenCheckingPresentation_ThenOnlyServiceContractsAreUsed(
            string implementationName)
        {
            IObjectProvider<IType> implementations = Types().That().HaveFullName(implementationName);
            Assert.That(implementations.GetObjects(ProductionArchitecture), Is.Not.Empty);

            Types().That().Are(SelectNamespace("ExonymsAPI.API", true))
                .Should().NotDependOnAny(implementations)
                .Because("presentation delegates to injected service interfaces, not implementations")
                .Check(ProductionArchitecture);
        }

        [Test]
        public void GivenProductionAssembly_WhenLoadingArchitecture_ThenTestAssembliesAreExcluded()
            => Assert.That(
                ProductionArchitecture.Assemblies.Select(assembly => assembly.Name),
                Is.EquivalentTo(new[] { typeof(ExonymsService).Assembly.GetName().Name }));

        [TestCase("ExonymsAPI.Service.Gatherers.GeoNamesGatherer")]
        [TestCase("ExonymsAPI.Service.Gatherers.WikiDataGatherer")]
        [TestCase("ExonymsAPI.Service.Processors.NameConstructor")]
        [TestCase("ExonymsAPI.Service.Processors.NameNormaliser")]
        [TestCase("ExonymsAPI.Client.TransliterationAPI.TransliterationApiClient")]
        public void GivenOrchestration_WhenCheckingDependencies_ThenInjectedInterfacesAreUsed(
            string implementationName)
        {
            IObjectProvider<IType> implementations = Types().That().HaveFullName(implementationName);
            IObjectProvider<IType> orchestrationTypes = SelectNamespace("ExonymsAPI.Service", false);
            Assert.That(implementations.GetObjects(ProductionArchitecture), Is.Not.Empty, implementationName);
            Assert.That(orchestrationTypes.GetObjects(ProductionArchitecture), Is.Not.Empty);

            Types().That().Are(orchestrationTypes).Should().NotDependOnAny(implementations)
                .Because("orchestration depends on injected contracts, not infrastructure implementations")
                .Check(ProductionArchitecture);
        }

        private static void AssertBoundary(
            string sourceNamespace,
            string forbiddenNamespace,
            bool includeForbiddenSubnamespaces)
        {
            IObjectProvider<IType> sourceTypes = SelectNamespace(sourceNamespace, true);
            IObjectProvider<IType> forbiddenTypes =
                SelectNamespace(forbiddenNamespace, includeForbiddenSubnamespaces);

            Assert.That(sourceTypes.GetObjects(ProductionArchitecture), Is.Not.Empty, sourceNamespace);
            Assert.That(forbiddenTypes.GetObjects(ProductionArchitecture), Is.Not.Empty, forbiddenNamespace);

            Types().That().Are(sourceTypes).Should().NotDependOnAny(forbiddenTypes)
                .Because($"{sourceNamespace} must not depend on {forbiddenNamespace}; dependencies point inward")
                .Check(ProductionArchitecture);
        }

        private static IObjectProvider<IType> SelectNamespace(string namespaceName, bool includeSubnamespaces)
        {
            string namespacePattern = $"^{Regex.Escape(namespaceName)}$";

            if (includeSubnamespaces)
            {
                namespacePattern = $"^{Regex.Escape(namespaceName)}(\\..*)?$";
            }

            return Types().That().ResideInNamespaceMatching(namespacePattern);
        }
    }
}