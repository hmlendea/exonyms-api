using NuciLog.Core;

namespace ExonymsAPI.Logging
{
    public sealed class MyLogInfoKey : LogInfoKey
    {
        MyLogInfoKey(string name) : base(name) { }

        public static LogInfoKey GeoNamesId => new MyLogInfoKey(nameof(GeoNamesId));

        public static LogInfoKey WikiDataId => new MyLogInfoKey(nameof(WikiDataId));

        public static LogInfoKey DefaultName => new MyLogInfoKey(nameof(DefaultName));

        public static LogInfoKey Count => new MyLogInfoKey(nameof(Count));
    }
}
