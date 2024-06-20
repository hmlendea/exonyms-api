using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ExonymsAPI.Service.Processors
{
    public class NameConstructor : INameConstructor
    {
        Dictionary<string, string> germanMiddleHighTransformations;

        public NameConstructor()
        {
            germanMiddleHighTransformations = new Dictionary<string, string>
            {
                { @"sch", "s" },
                { @"Sch", "S" },
                { @"hl", "hel" },

                { @"([Aa])a", "$1" },
                { @"([Bb])erg", "$1ërc" },
                { @"([Bb])[uü]rg", "$1urc" },
                { @"([Bb])rok", "$1ruch" },
                { @"([Bb])rück", "$1ruk" },
                { @"([Bb])run", "$1raun" },
                { @"([Nn])ord", "$1ort" },
                { @"([Ww])ei", "$1í" },
                { @"\bDo\B", "Thu" },
                { @"\bEns", "Anis" },
                { @"\bG\B", "Ch" },
                { @"a\b", "e" },
                { @"anno", "ano" },
                { @"au\B", "ou" },
                { @"au\b", "öu" },
                { @"ay", "ei" },
                { @"dal", "tal" },
                { @"den", "dun" },
                { @"dm", "dem" },
                { @"drecht", "chtrad" },
                { @"ds", "des" },
                { @"dt", "t" },
                { @"e([dt])", "ei$1" },
                { @"enb", "ernb" },
                { @"erar", "ærer" },
                { @"ere", "eer" },
                { @"err", "ehr" },
                { @"ers", "eres" },
                { @"ert", "irs" },
                { @"feld", "felt" },
                { @"ff", "f" },
                { @"ford", "furt" },
                { @"ge([nr])", "ce$1" },
                { @"ham\b", "heim" },
                { @"ie([bns])", "i$1" },
                { @"iern", "iren" },
                { @"land", "lant" },
                { @"ler\b", "lære" },
                { @"n[a]*s", "nes" },
                { @"or\b", "ar" },
                { @"Öster", "Ost" },
                { @"ouen", "ouwen" },
                { @"reid", "rid" },
                { @"reu", "riu" },
                { @"rik", "rich" },
                { @"row", "rew" },
                { @"run", "raun" },
                { @"sg[aoö]u\b", "sachgöu" },
                { @"ssen", "snede" },
                { @"sten\b", "stein" },
                { @"thal", "tal" },
                { @"Ts", "Cz" },
                { @"tt", "t" },
                { @"uck", "ök" },
                { @"üd", "üed" },
                { @"vad", "wat" },
                { @"ver\b", "vere" },
                { @"wi[c]*k", "weich" },
                { @"wíg\b", "wích" },
                { @"x", "cc" },
                { @"zn", "zen" },
                //{ @"bs", "bichts" },
                //{ @"n[aoö]u\b", "nouwe" },

                { @"([^s])chl", "$1chel" },
                { @"ck", "k" },
                { @"on", "un" },
                { @"ss", "z" },
                { @"ß", "z" },

                { @"lsbërc", "lebërc" },
                { @"([Bb])roun", "$1raun" },
            };
        }

        public string Construct(string baseName, string language)
        {
            string constructedName = baseName;

            IDictionary<string, string> transformations;

            if (language == "gmh")
            {
                transformations = germanMiddleHighTransformations;
            }
            else
            {
                transformations = new Dictionary<string, string>();
            }

            foreach (string pattern in transformations.Keys)
            {
                constructedName = Regex.Replace(constructedName, pattern, transformations[pattern]);
            }

            return constructedName;
        }
    }
}
