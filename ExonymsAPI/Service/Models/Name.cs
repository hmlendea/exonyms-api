namespace ExonymsAPI.Service.Models
{
    public class Name
    {
        private string normalisedName;

        public string OriginalName { get; set; }

        public string NormalisedName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(normalisedName))
                {
                    return OriginalName;
                }

                return normalisedName;
            }
            set
            {
                normalisedName = value;
            }
        }

        public Name(string name)
        {
            OriginalName = name;
        }

        public static bool IsNullOrWhiteSpace(Name name)
        {
            if (name is null)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(name.OriginalName))
            {
                return true;
            }

            return false;
        }
    }
}
