using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using NUnit.Framework;

using ExonymsAPI.Service;

namespace ExonymsAPI.ArchitectureTests
{
    [TestFixture]
    public sealed class ProjectReferenceTests
    {
        private static readonly Assembly ProductionAssembly = typeof(ExonymsService).Assembly;

        private static readonly XDocument ProductionProject = XDocument.Load(
            Path.Combine(TestContext.CurrentContext.TestDirectory, "ProductionProjects", "ExonymsAPI.csproj"));

        [Test]
        public void GivenTheMonolithicHost_WhenCheckingProjectReferences_ThenNoOtherLayersAreReferenced()
            => Assert.That(
                ProductionProject.Descendants("ProjectReference"),
                Is.Empty,
                "The documented architecture has one production project; new projects require explicit boundaries.");

        [TestCase("PackageReference")]
        [TestCase("Reference")]
        public void GivenProductionReferences_WhenCheckingDeclarations_ThenTestDependenciesAreExcluded(
            string referenceKind)
            => Assert.That(
                ProductionProject.Descendants(referenceKind)
                    .Select(reference => (string?)reference.Attribute("Include"))
                    .Where(reference => reference is not null && IsTestDependency(reference)),
                Is.Empty,
                "Production project declarations must not reference test projects or test frameworks.");

        [Test]
        public void GivenProductionAssembly_WhenCheckingReferences_ThenTestDependenciesAreExcluded()
            => Assert.That(
                ProductionAssembly.GetReferencedAssemblies()
                    .Select(reference => reference.Name)
                    .Where(reference => reference is not null && IsTestDependency(reference)),
                Is.Empty,
                "The compiled production assembly must not depend on tests or test frameworks.");

        [Test]
        public void GivenProductionAssembly_WhenCheckingVisibility_ThenInternalsAreNotExposed()
            => Assert.That(
                ProductionAssembly.GetCustomAttributes<InternalsVisibleToAttribute>(),
                Is.Empty,
                "Production internals must not be exposed to other assemblies for testing.");

        [Test]
        public void GivenProductionProject_WhenCheckingVisibility_ThenInternalsAreNotExposed()
            => Assert.That(
                ProductionProject.Descendants().Where(element =>
                    string.Equals(element.Name.LocalName, "InternalsVisibleTo") ||
                    string.Equals(element.Name.LocalName, "AssemblyAttribute") &&
                    ((string?)element.Attribute("Include"))?.Contains(
                        nameof(InternalsVisibleToAttribute), StringComparison.Ordinal) is true),
                Is.Empty,
                "Production project declarations must not grant access to internals.");

        private static bool IsTestDependency(string referenceName) =>
            referenceName.Contains("Tests", StringComparison.OrdinalIgnoreCase) ||
            referenceName.Contains("TestPlatform", StringComparison.OrdinalIgnoreCase) ||
            referenceName.Contains("Test.Sdk", StringComparison.OrdinalIgnoreCase) ||
            referenceName.Contains("NUnit", StringComparison.OrdinalIgnoreCase) ||
            referenceName.Contains("xunit", StringComparison.OrdinalIgnoreCase) ||
            referenceName.Contains("Moq", StringComparison.OrdinalIgnoreCase) ||
            referenceName.Contains("ArchUnit", StringComparison.OrdinalIgnoreCase);
    }
}