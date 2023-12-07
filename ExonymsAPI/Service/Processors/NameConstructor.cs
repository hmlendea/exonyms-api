using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ExonymsAPI.Service.Processors
{
    public class NameConstructor : INameConstructor
    {
        Dictionary<string, string> germanMiddleHighTransformations;
        Dictionary<string, string> germanOldLowTransformations;

        public NameConstructor()
        {
            germanMiddleHighTransformations = new Dictionary<string, string>
            {
                { @"sch", "s" },
                { @"Sch", "S" },
                { @"hl", "hel" },

                { @"([Aa])a", "$1" },
                { @"([Bb])[eouü]rg", "$1ërc" },
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

            germanOldLowTransformations = new Dictionary<string, string>
            {
                { @"[AÄÅ]n", "En" },
                { @"\bAard", "Erd" },
                { @"\bAbi", "Abba" },
                { @"\bArc", "Irc" },
                { @"\bAyle", "Ægele" },
                { @"\bCl", "Kl" },
                { @"Ä", "A" },

                { @"([^l])den", "$1than" },
                { @"([Aa])a", "$1" },
                { @"([Aa])lt", "$1ld" },
                { @"([Aa])lv", "$1lf" },
                { @"([Bb])[ouü]rgen", "$1urgium" },
                { @"([Bb])ake", "$1adec" },
                { @"([Bb])ir", "$1eor" },
                { @"([Bb])org", "$1urg" },
                { @"([Bb])raun", "$1ron" },
                { @"([Bb])ury", "$1urh" },
                { @"([dr])en", "$1an" },
                { @"([Ff])ield", "$1eld" },
                { @"([Hh])agen\b", "$1oaven" },
                { @"([Hh])aus", "$1us" },
                { @"([Hh])eim", "$1em" },
                { @"([Hh])m", "$1em" },
                { @"([Hh])ofen", "$1ofum" },
                { @"([r])chen", "$1cinga" },
                { @"([Rr])eich", "$1iki" },
                { @"([Ss])tätten", "$1tedios" },
                { @"([ßt])en", "$1un" },
                { @"([Ww])erpen", "$1ervan" },
                { @"\Bgau\b", "ga" },
                { @"\Bwell\b", "wella" },
                { @"aff", "epp" },
                { @"aun", "una" },
                { @"Che", "Ke" },
                { @"cw", "caw" },
                { @"dale\b", "dal" },
                { @"don\b", "dune" },
                { @"fast\b", "fasten" },
                { @"fsb", "fb" },
                { @"ger", "gir" },
                { @"gham", "gaham" },
                { @"hr", "har" },
                { @"ieben", "ivun" },
                { @"kers", "kas" },
                { @"lägen", "legin" },
                { @"ngd", "nd" },
                { @"nsb", "nb" },
                { @"nst", "nest" },
                { @"nt", "ndh" },
                { @"Öster", "Ost" },
                { @"pen", "pan" },
                { @"rs", "rns" },
                { @"rw", "rew" },
                { @"ß", "t" },
                { @"stadt\b", "stedi" },
                { @"tte", "di" },
                { @"uck", "uk" },
                { @"wall\b", "weal" },
                { @"weig", "wiek" },
                { @"wick", "wik" },

                { @"([Ss])tein", "$1ten" },
                { @"ker", "kar" },
                { @"schl", "sl" },
                { @"wei", "hwi" },
                { @"Wei", "Hwi" },

                { @"ch", "k" },
                { @"ck", "kk" },
                { @"skw", "sw" },

                { @"ö", "o" },
                { @"ü", "u" },
                { @"y", "i" },
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
            else if (language == "osx")
            {
                transformations = germanOldLowTransformations;
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
