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
                { @"([Hh])aus", "$1us" },

                { @"[AÄÅ]n", "En" },
                { @"\bAard", "Erd" },
                { @"\bAbi", "Abba" },
                { @"\bArc", "Irc" },
                { @"\bAyle", "Ægele" },
                { @"\bChe", "Ke" },
                { @"\bCl", "Kl" },
                { @"\bD[ei]", "Þi" },
                { @"\bDo", "Þu" },
                { @"\bEi", "E" },
                { @"\bEns", "Ans" },
                { @"\bHa", "A" },
                { @"\bInn", "En" },
                { @"\bIp", "Gip" },
                { @"\bTe", "Di" },
                { @"Ä", "A" },

                { @"auf", "op" },
                { @"unca", "uneca" },

                { @"\B([Hh])agen\b", "$1oaven" },
                { @"\Bcaster\b", "ceastre" },
                { @"\Bchenland\b", "cland" },
                { @"\Bdale\b", "dal" },
                { @"\Bde\b", "þa" },
                { @"\Bdon\b", "dune" },
                { @"\Bdorf\b", "þörp" },
                { @"\Bega[u]*\b", "eaga" },
                { @"\Bfast\b", "fasten" },
                { @"\Bfurt\b", "ford" },
                { @"\Bgau\b", "ga" },
                { @"\Bingen\b", "ungi" },
                { @"\Bkirchen\b", "kirikan" },
                { @"\Bköping\b", "kaufing" },
                { @"\Bster\b", "stre" },
                { @"\Bmark\b", "marka" },
                { @"\Bmarschen\b", "maresca" },
                { @"\Bmham\b", "mhem" },
                { @"\Bmold\b", "malli" },
                { @"\Bruhe\b", "rowa" },
                { @"\Bmund\b", "muþ" },
                { @"\Bonn\b", "unno" },
                { @"\Bnau\b", "nawi" },
                { @"\Brecht\b", "rid" },
                { @"\Bsche(de|tha|þa)\b", "sceiþa" },
                { @"\Bst[äa][dt]t\b", "stedi" },
                { @"\Buren\b", "urian" },
                { @"\Bver\b", "vir" },
                { @"\Bwall\b", "wag" },
                { @"\Bweich\b", "wik" },
                { @"\Bwell\b", "wella" },

                { @"lägen", "legin" },

                { @"([^l])den", "$1þan" },
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
                { @"([Hh])eim", "$1em" },
                { @"([Hh])m", "$1em" },
                { @"([Hh])ofen", "$1ofum" },
                { @"([Kk])[oö]nig", "$1uning" },
                { @"([Kk])arl", "$1eril" },
                { @"([r])chen", "$1cinga" },
                { @"([Rr])eich", "$1iki" },
                { @"([Ss])tätten", "$1tedios" },
                { @"([ßt])en\B", "$1un" },
                { @"([Ww])erpen", "$1ervan" },
                { @"[ä]n", "en" },
                { @"aff", "epp" },
                { @"aun", "una" },
                { @"cw", "caw" },
                { @"edm", "eþam" },
                { @"eil", "el" },
                { @"fsb", "fb" },
                { @"gen", "gon" },
                { @"ger", "gir" },
                { @"gham", "gaham" },
                { @"gs", "ges" },
                { @"hr", "har" },
                { @"ieben", "ivun" },
                { @"iep", "ip" },
                { @"irk", "irik" },
                { @"ithe", "iet" },
                { @"itm", "iodm" },
                { @"ken", "kon" },
                { @"kers", "kas" },
                { @"kw", "kaw" },
                { @"lber", "lfar" },
                { @"lig", "lag" },
                { @"lls", "lla" },
                { @"lsr", "lesr" },
                { @"ms", "mes" },
                { @"n[gs]([bd])", "n$1" },
                { @"nem", "nim" },
                { @"ngo", "ngwa" },
                { @"nnb", "nesb" },
                { @"nne", "na" },
                { @"ns([ct])", "nes$1" },
                { @"nt", "ndh" },
                { @"Öster", "Ost" },
                { @"ött", "ut" },
                { @"pen", "pan" },
                { @"pps", "bbians" },
                { @"raue", "rua" },
                { @"rdr", "rodr" },
                { @"red", "riþ" },
                { @"rma", "rima" },
                { @"rs", "rns" },
                { @"rt", "rut" },
                { @"rw", "rew" },
                { @"s([fw])", "so$1" },
                { @"ß", "t" },
                { @"tsch", "disk" },
                { @"tte", "di" },
                { @"uck", "uk" },
                { @"üt", "uþ" },
                { @"wals", "walds" },
                { @"weig", "wiek" },
                { @"wick", "wik" },
                //{ @"bers", "burs" },

                { @"([Ss])tein", "$1ten" },
                { @"ern", "ri" },
                { @"ker", "kar" },
                { @"schl", "sl" },
                { @"wei", "hwi" },
                { @"Wei", "Hwi" },

                { @"ch", "k" },
                { @"ck", "kk" },
                { @"skw", "sw" },

                { @"auk", "ok" },

                { @"aa", "a" },
                { @"ie", "ē" },
                { @"th", "þ" },
                { @"ü", "u" },
                { @"y", "i" },
                //{ @"ö", "o" },
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
