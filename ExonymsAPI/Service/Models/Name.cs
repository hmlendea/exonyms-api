using System;
using System.Text.Json.Serialization;

namespace ExonymsAPI.Service.Models
{
    public sealed class Name(string name) : IEquatable<Name>
    {
        private string value;

        public string OriginalValue { get; set; } = name;

        public string Value
        {
            get => string.IsNullOrWhiteSpace(value) ? OriginalValue : value;
            set => this.@value = value;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Comment { get; set; }

        public static bool IsNullOrWhiteSpace(Name name)
        {
            if (name is null)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(name.Value))
            {
                return true;
            }

            return false;
        }

        public bool Equals(Name other)
        {
            if (other is null)
            {
                return false;
            }

            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Name);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
}
