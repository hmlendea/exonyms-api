using System.Text.RegularExpressions;

namespace ExonymsAPI.Service.Normalisers
{
    public class NameNormaliser : INameNormaliser
    {
        public string Normalise(string languageCode, string name)
        {
            string normalisedName = name;

            normalisedName = Regex.Replace(normalisedName, " - .*", string.Empty);
            normalisedName = Regex.Replace(normalisedName, ",.*", string.Empty);
            normalisedName = Regex.Replace(normalisedName, "[…]", string.Empty);
            normalisedName = Regex.Replace(normalisedName, "/.*", string.Empty);
            normalisedName = Regex.Replace(normalisedName, "\\(.*", string.Empty);
            normalisedName = Regex.Replace(normalisedName, "\\s*<alternateName .*$", string.Empty);
            normalisedName = Regex.Replace(normalisedName, "^\"(.*)\"$", "$1");
            normalisedName = Regex.Replace(normalisedName, @"^[^\s]*:", string.Empty);

            normalisedName = RemoveWords(normalisedName);
            normalisedName = RemoveLanguageSpecificWords(languageCode, normalisedName);

            normalisedName = Regex.Replace(normalisedName, @"\s\s*", " ");
            normalisedName = Regex.Replace(normalisedName, @"^[\s\-]*", string.Empty);
            normalisedName = Regex.Replace(normalisedName, @"[\s\-]*$", string.Empty);
            normalisedName = normalisedName.Trim();

            return normalisedName;
        }

        private string RemoveWords(string name)
        {
            string cleanedName = name;

            // Abbey
            cleanedName = Regex.Replace(
                cleanedName,
                @"[AaOo][bp][abd]([aet][z]*[iy][ae]*|[ií][aj]|(c|ts)[t]*[vw][oí])|" +
                @"Benediktinerabtei",
                string.Empty);

            // Agency
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Aa]gen[cț][ijy][a]*",
                string.Empty);

            // Ancient
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Aa]ncient|" +
                @"Antiikin [Aa]nti[i]*[ck](a|in)*|" +
                @"Ar[c]*ha[ií][ac]",
                string.Empty);


            // Area
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Aa]r[dei]a[l]*",
                string.Empty);

            // Autonomous Government
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Aa][uv]tonom(e|\noye|ous) ([Gg]overnment|[Pp]ravitel’stvo|[Rr]egering)|" +
                @"[Gg]obierno [Aa]ut[oó]nomo|" +
                @"[Öö]zerk [Hh]ükümeti",
                string.Empty);

            // Canton
            cleanedName = Regex.Replace(
                cleanedName,
                @"[CcKk][’]*[hy]*[aāe][i]*[nṇ][tṭ][’]*[aoóuū]n(a|i|o|s|u[l]*)*",
                string.Empty);

            // Castle
            cleanedName = Regex.Replace(
                cleanedName,
                @"[CcGgKk]a[i]*[sz][lt][ei]*[aál][il]*[eoulmn]*[a]*|" +
                @"[Cc]h[aâ]teau|" +
                @"Dvorac|" +
                @"[KkQq]al[ae]s[iı]|" +
                @"Z[aá]m[aeo][gk][y]*",
                string.Empty);

            // Cathedral
            cleanedName = Regex.Replace(
                cleanedName,
                @"[CcKk]at[h]*[eé]dr[ai][kl][aeoó]*[s]*",
                string.Empty);

            // Church
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Bb]iserica|" +
                @"[Cc]hiesa|" +
                @"[Cc]hurch|" +
                @"[Éé]glise|" +
                @"[Ii]greja|" +
                @"[Kk]yōkai",
                string.Empty);

            // City
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Cc]iud[aá][dt]*|" +
                @"[Cc]ivitas|" +
                @"[CcSs](ee|i)[tṭ]\+[aàeiy]|" +
                @"Nagara|" +
                @"Oraș(ul)*|" +
                @"Śahara|" +
                @"Sich’i|" +
                @"[Ss]tadt",
                string.Empty);

            // Commune
            cleanedName = Regex.Replace(
                cleanedName,
                @"[CcKk]om[m]*un[ae]*|" +
                @"[Kk]özség",
                string.Empty);

            // Council
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Cc]o[u]*n[cs][ei]l[l]*(iul)|" +
                @"[Cc]omhairle",
                string.Empty);

            // Country
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Cc]ontea|" +
                @"[Jj]ude[tț]ul|" +
                @"[Nn]egeri|" +
                @"[Xx]i[aà]n",
                string.Empty);

            // County
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Cc]o[u]*[mn]t(a(do|t)|y)|" +
                @"Landgra[a]*fs(cha(ft|p)|tvo)",
                string.Empty);

            // Department
            cleanedName = Regex.Replace(
                cleanedName,
                @"[DdḌḍ][eéi]p[’]*[aā][i]*r[tṭ][’]*[aei]*m[aeēi][e]*[nṇ]*[gtṭ]*[’]*(as|i|o|u(l|va)*)*|" +
                @"Ilākhe|" +
                @"Penbiran|" +
                @"Tuṟai|" +
                @"Vibhaaga|" +
                @"Zhang Wàt",
                string.Empty);

            // Desert
            cleanedName = Regex.Replace(
                cleanedName,
                @"Anapat|" +
                @"[Aa]nialwch|" +
                @"Çölü|" +
                @"[Dd][i]*[eè]*[sșz][iy]*er[tz](h|o|ul)*|" +
                @"Eḍāri|" +
                @"Gaineamhh|" +
                @"Gurun|" +
                @"Hoang|" +
                @"Maru[bs]h(tal|ūmi)|" +
                @"[Mm]ortua|" +
                @"Pālaivaṉam|" +
                @"Pustynia|" +
                @"Raṇa|" +
                @"Sa[bm]ak[u]*|" +
                @"Se wedhi|" +
                @"shāmò|" +
                @"Tá Laēy Saāi|" +
                @"Vaalvnt",
                string.Empty);

            // Diocese
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Dd]io[eít]*[cks][eēi][sz][eēi]*[s]*",
                string.Empty);

            // District
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Aa]maṇḍalam|" +
                @"[Aa]pygarda|" +
                @"[Bb]arrutia|" +
                @"[Bb]ucağı|" +
                @"Ḍāḥīẗ|" +
                @"[Dd][h]*[iy]str[eiy][ckt]*[akt][eouy]*[als]*|" +
                @"[Iiİi̇]l[cç]esi|" +
                @"járás|" +
                @"[Jj]ēlā|" +
                @"Jil[lh]*[aāeo][a]*|" +
                @"Koān|" +
                @"Māvaṭṭam|" +
                @"Mnṭqẗ|" +
                @"[Pp]asuni|" +
                @"[Pp]iirikunta|" +
                @"[Pp]irrâdâh|" +
                @"Qu(ận)*|" +
                @"[Rr]a[iy]on[iu]|" +
                @"sum",
                string.Empty);

            // Duchy
            cleanedName = Regex.Replace(
                cleanedName,
                @"bǎijué|" +
                @"[Dd][uü][ck]([aá][dt]*[otu][l]*|h[éy]|lüğü)|" +
                @"Hertogdom|" +
                @"Kadipaten",
                string.Empty);

            // Emirate
            cleanedName = Regex.Replace(
                cleanedName,
                @"Aēy Mí Raēy Dtà|" +
                @"[ĀāEeÉéƏəIiYy]m[aāi]r[l]*[aàāẗhi][dğty]*([aeiou][l]*)*|" +
                @"qiúcháng|" +
                @"Saamiro|" +
                @"Tiểu vương quốc|" +
                @"T[’']ohuguk",
                string.Empty);

            // Fort
            cleanedName = Regex.Replace(
                cleanedName,
                @"Benteng|" +
                @"([CcKk][aá]str[aou][lm]*|" +
                @"[Cc]héngbǎo|" +
                @"Chillā|" +
                @"[Ff][aäe]st(ni|u)ng(en)*|" +
                @"[Ff][oū]rt([aă][lr]e[a]*[szț]a|[e]*[r]*e[t]*s[s]*[y]*[ae]*|ez[z]*a|e|ikaĵo|ul)*|" +
                @"Kōṭṭai|" +
                @"[Kk]repost|" +
                @"[Tt]rd[i]*n(jav)*a|" +
                @"[Yy]ōsai|" +
                @"[Zz]amogy)( (roman|royale))*",
                string.Empty);

            // Hundred
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Hh][äe]r[r]*[ae]d|" +
                @"[Hh]undred|" +
                @"[Kk]ihlakunta",
                string.Empty);

            // Island
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Aa]raly|" +
                @"Đảo|" +
                @"[Ǧǧ]zīrẗ|" +
                @"[Ii]l[hl]a|" +
                @"[Ii]nsula|" +
                @"[Ii]sl[ae]|" +
                @"[Ii]sland|" +
                @"[Îî]le|" +
                @"[Nn][eḗ]sos|" +
                @"Ostr[io]v|" +
                @"Sŏm",
                string.Empty);

            // Kingdom
            cleanedName = Regex.Replace(
                cleanedName,
                @"guó|" +
                @"Irācciyam|" +
                @"[Kk][eoö]ni[n]*[gk]r[e]*[iy][cej]*[hk]|" +
                @"K[io]ng[e]*d[oø]m(met)*|" +
                @"[Kk]irályság|" +
                @"[Kk][o]*r[oaá]l[oe]*[v]*stv[ío]|" +
                @"Ōkoku|" +
                @"Rājy[a]*|" +
                @"[Rr]egatul|" +
                @"[Rr][eo][giy][an][eolu][m]*[e]*|" +
                @"[Rr]īce|" +
                @"[Tt]eyrnas",
                string.Empty);

            // Lake
            cleanedName = Regex.Replace(
                cleanedName,
                @"Gölü|" +
                @"[Ll]a(c|cul|go|ke)|" +
                @"[Nn][uú][u]*r|" +
                @"[Oo]zero",
                string.Empty);

            // Language
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Bb][h]*[aā][a]*[sṣ][h]*[aā][a]*|" +
                @"[Ll][l]*[aeií][mn][g]*[buv]*[ao](ge)*",
                string.Empty);

            // Mountain
            cleanedName = Regex.Replace(
                cleanedName,
                @"([Gg]e)*[Bb]i[e]*rge[r]*|" +
                @"[Dd]ağları|" +
                @"[GgHh][ao]ra\b|" +
                @"Ǧibāl|" +
                @"[Mm][ouū][u]*n[tț][aei]*([gi]*[ln][es]|ii|s)*|" +
                @"[Pp]arvata[ṁ]*|" +
                @"[Ss]hānmài",
                string.Empty);

            // Monastery
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Bb]iara|" +
                @"[Kk]l[aáo][o]*[sš][z]*t[eo]r(is)*|" +
                @"((R[eo][y]*al|[BV]asilikó) )*[Mm][aăo][i]*[n]*[aăei]*(ĥ|st)[eèḗiíy]*[r]*(e[a]*|[iı]|[ij]o[a]*|o|y)*|" +
                @"[Ss]amostan|" +
                @"[Ss]hu[u]*dōin",
                string.Empty);

            // Municipium
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Bb]elediyesi|" +
                @"Chibang Chach’ije|" +
                @"Chū-tī|" +
                @"Đô thị tự trị|" +
                @"[Kk]ong-[Ss]iā|" +
                @"[Kk]otamadya|" +
                @"[Mm]eūang|" +
                @"[Mm][y]*un[i]*[t]*[cs]ip[’]*([aā]*l[i]*[dtṭ][’]*(a[ds]|é|et’i|[iī]|y)|i[ou][lm]*)|" +
                @"[Mm]unicipi|" +
                @"[Nn]agara [Ss]abhāva|" +
                @"[Nn]a[gk][a]*r[aā](pālika|ṭci)|" +
                @"[Pp]ašvaldība|" +
                @"[Pp][a]*urasabh[āe]|" +
                @"[Ss]avivaldybė",
                string.Empty);

            // Municipality
            cleanedName = Regex.Replace(
                cleanedName,
                @"Bwrdeistref|" +
                @"Concello|" +
                @"D[ḗií]mos|" +
                @"[Gg][e]*m[e]*[ij]*n[dt]*[ae]|" +
                @"gielda|" +
                @"O[bp]([cćčš]|s[hj])[t]*ina|" +
                @"udalerria",
                string.Empty);

            // National Park
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Nn]ational [Pp]ark|" +
                @"Par[cq]u[el] Na[ctț]ional|" +
                @"[Vv]ườn [Qq]uốc",
                string.Empty);

            // Oasis
            cleanedName = Regex.Replace(
                cleanedName,
                @"[aā]l-[Ww]āḥāt|" +
                @"[OoÓóŌō][syẏ]*[aáāeē][sz][h]*[aiīeėē][ans]*[uŭ]*|" +
                @"Oūh Aēy Sít",
                string.Empty);

            // Peninsula
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Bb][aá]n[ ]*[dđ][aả]o|" +
                @"[Dd]uoninsulo|" +
                @"[Hh]antō|" +
                @"[Ll]edenez|" +
                @"[Nn]iemimaa|" +
                @"[Pp][ao][luŭ][ouv]ostr[ao][uŭv]|" +
                @"[Pp][eé]n[iíì][n]*[t]*[csz][ou][lł][aāe]|" +
                @"[Pp]enrhyn|" +
                @"Poàn-tó|" +
                @"[Ss]emenanjung|" +
                @"Tīpakaṟpam|" +
                @"[Yy]arim [Oo]roli|" +
                @"[Yy]arımadası|" +
                @"[Žž]arym [Aa]raly",
                string.Empty);

            // Plateau
            cleanedName = Regex.Replace(
                cleanedName,
                @"Alt[io]p[il]*[aà](no)*|" +
                @"Àrd-thìr|" +
                @"Daichi|" +
                @"gāoyuán|" +
                @"Hḍbẗ|" +
                @"ordokia|" +
                @"[Pp][’]*lat[’]*[e]*([aå][nu](et)*|o(s[iu])*)|" +
                @"[Pp]lošina|" +
                @"[Pp]lynaukštė",
                string.Empty);

            // Prefecture
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Pp]r[aäeé][e]*fe[ckt]t[uúū]r[ae]*",
                string.Empty);

            // Province
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Cc]hangwat|" +
                @"eanangoddi|" +
                @"[Ee]par[ck]hía|" +
                @"[Ll]alawigan|" +
                @"[Mm]ākāṇam|" +
                @"Mḥāfẓẗ|" +
                @"Mkoa|" +
                @"Mqāṭʿẗ|" +
                @"[Pp][’]*r[aāou][bpvw][ëií][nñ]*[t]*[csśz]*[eėiíjoy]*[aeėnsz]*|" +
                @"Pradēśa|" +
                @"Pr[aā][a]*nt[y]*[a]*|" +
                @"Rát|" +
                @"[Ss][h]*[éě]ng|" +
                @"Shuu|" +
                @"suyu|" +
                @"[Tt]alaith|" +
                @"[VvWw]il[ao][jy][ae][ht][i]*",
                string.Empty);

            // Region
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Aa]ñcala|" +
                @"[Bb]ölgesi|" +
                @"[Ee]skualdea|" +
                @"[Ff]aritanin[']*i|" +
                @"Gobolka|" +
                @"[Kk]alāpaya|" +
                @"Khu vực|" +
                @"[Kk]shetr|" +
                @"Kwáāen|" +
                @"[Pp]akuti|" +
                @"[Pp]aḷāta|" +
                @"[Pp]eri(f|ph)[eéē]r[e]*i[j]*a|" +
                @"[Pp]iirkond|" +
                @"[Pp]r[a]*desh[a]*|" +
                @"[Pp]rāntaṁ|" +
                @"[Rr][eé][gģhx][ij]*([ãoóu][ou]*n*[ei]*[as]*|st[aā]n)|" +
                @"[Rr]ijn",
                string.Empty);

            // Republic
            cleanedName = Regex.Replace(
                cleanedName,
                @"Cộng hòa|" +
                @"[DdTt][aáä][aä]*[ʹ]*s[s]*[ei]*v[aäá][ʹ]*ld[di]|" +
                @"[Dd][eēi]mokr[h]*atía|" +
                @"gōnghé|" +
                @"[Gg]weriniaeth|" +
                @"[Jj]anarajaya|" +
                @"Khiung-fò-koet|" +
                @"Kongwaguk|" +
                @"Köztársaság|" +
                @"Kyōwa( Koku)*|" +
                @"Olómìnira|" +
                @"Praj[aā][a]*[s]*t[t]*a[a]*(k|ntra)|" +
                @"[Rr][eéi][s]*[ ]*p[’]*[aāuüùúy][ā’]*b[ba]*l[eií][’]*[cgkq][ck]*[’]*([ai]|as[ıy]|en|[hḥ]y|i|ue)*|" +
                @"[Ss]ăā-taā-rá-ná-rát|" +
                @"[Tt]a[sz][ao]val[dt](a|kund)",
                string.Empty);

            // River
            cleanedName = Regex.Replace(
                cleanedName,
                @"Abhainn|" +
                @"Afon|" +
                @"[Ff][il]u(me|viul)|" +
                @"Gawa|" +
                @"Nadī|" +
                @"Nhr|" +
                @"[Rr]âu[l]*|" +
                @"[Rr]iver|" +
                @"Sungai",
                string.Empty);

            // Ruin
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Rr]uin[ae]*",
                string.Empty);

            // State
            cleanedName = Regex.Replace(
                cleanedName,
                @"Bang|" +
                @"[EeÉéIi]*[SsŜŝŜŝŠšŞş][h]*[tṭ][’]*[aeē][dtṭ]([aeiıos]|ul)|" +
                @"[Oo]sariik|" +
                @"[Oo]st[’]*an[ıi]|" +
                @"Ūlāīẗ|" +
                @"[Uu]stoni|" +
                @"valstija*",
                string.Empty);

            // Temple
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Dd]ēvālaya(mu)*|" +
                @"[Kk]ōvil|" +
                @"[Mm][a]*ndir[a]*|" +
                @"Ná Tiān|" +
                @"[Pp]agoda|" +
                @"[Tt]emp[e]*l[eou]*[l]*",
                string.Empty);

            // Territory
            cleanedName = Regex.Replace(
                cleanedName,
                @"Chunju|" +
                @"Iqlīm|" +
                @"Lãnh|" +
                @"Léng-thó͘|" +
                @"lǐngde|" +
                @"Lurraldea|" +
                @"[Tt]er[r]*[iy]t[oó]r[iy][iy]*[r]*[aeou]*[mt]*",
                string.Empty);

            // Township
            cleanedName = Regex.Replace(
                cleanedName,
                @"[CcKk]anton[ae]*(mendua)*|" +
                @"[Tt]ownship",
                string.Empty);

            // University
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Dd]aigaku|" +
                @"(Lā )*[BbVv]i[sś][h]*[vw]\+(a[bv])*idyāla[yẏ][a]*[ṁ]*|" +
                @"[Oo]llscoil|" +
                @"[Uu]niversit(ate[a]a*|y)|" +
                @"[Vv]idyaapith",
                string.Empty);

            // Voivodeship
            cleanedName = Regex.Replace(
                cleanedName,
                @"V[éo][i]*[e]*vod[ae]*(s(hip|tv[ií])|t(e|ul))",
                string.Empty);

            // of
            cleanedName = Regex.Replace(
                cleanedName,
                @"\b(" +
                @"[AaĀā]p[h]*[a]*|" +
                @"[Dd][aeio]*[ls]*|" +
                @"gia|" +
                @"ja|" +
                @"[Oo]f|" +
                @"[Mm]ạc|" +
                @"ng|" +
                @"[Tt]a|" +
                @"[Tt]hổ|" +
                @"t[ēi]s|" +
                @"[Tt]o[uy]|" +
                @"van|" +
                @"w|" +
                @"[Yy]r" +
                @")" +
                @"[ ""\'’']",
                string.Empty);

            return cleanedName.Trim();
        }

        private string RemoveLanguageSpecificWords(string languageCode, string name)
        {
            string cleanedName = name;

            if (languageCode.Equals("ang"))
            {
                cleanedName = Regex.Replace(cleanedName, @"enrice\b", "e");
            }

            if (languageCode.Equals("fi"))
            {
                cleanedName = Regex.Replace(cleanedName, @"in luostari", string.Empty);
                cleanedName = Regex.Replace(cleanedName, @"un kunta", "u");
            }

            if (languageCode.Equals("ja"))
            {
                cleanedName = Regex.Replace(cleanedName, @"Ken\b", string.Empty);
            }

            if (languageCode.Equals("kaa"))
            {
                cleanedName = Regex.Replace(cleanedName, @"U'", "Ú");
            }

            if (languageCode.Equals("ko"))
            {
                cleanedName = Regex.Replace(cleanedName, @"[gj]u\b", string.Empty);
            }

            if (languageCode.Equals("lt"))
            {
                cleanedName = Regex.Replace(cleanedName, @"\bŠv", "Šventasis");
            }

            if (languageCode.StartsWith("zh") ||
                languageCode.Equals("cdo") ||
                languageCode.Equals("nan"))
            {
                cleanedName = Regex.Replace(cleanedName, @"(fǔ|[Hh][úū]|shìzhēn)\b", string.Empty);
                cleanedName = Regex.Replace(cleanedName, @"-", string.Empty);

                if (languageCode.Equals("nan"))
                {
                    cleanedName = Regex.Replace(cleanedName, @"(chhī|khu)\b", string.Empty);
                }

                if (languageCode.StartsWith("zh"))
                {
                    cleanedName = Regex.Replace(cleanedName, @"ōu\b", string.Empty);
                }
            }

            return cleanedName;
        }
    }
}
