namespace ExonymsAPI.Service.Models
{
    public class Name
    {
        private string value;

        public string OriginalValue { get; set; }

        public string Value
        {
            get
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return OriginalValue;
                }

                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public Name(string name)
        {
            OriginalValue = name;
        }

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
    }
}
