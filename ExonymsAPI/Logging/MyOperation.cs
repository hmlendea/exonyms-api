using NuciLog.Core;

namespace ExonymsAPI.Logging
{
    public sealed class MyOperation : Operation
    {
        MyOperation(string name) : base(name) { }

        public static Operation GatherExonyms => new MyOperation(nameof(GatherExonyms));

        public static Operation GatherGeoNamesExonyms => new MyOperation(nameof(GatherGeoNamesExonyms));

        public static Operation GatherWikiDataExonyms => new MyOperation(nameof(GatherWikiDataExonyms));
    }
}
