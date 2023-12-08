using System.Text.RegularExpressions;

namespace ExonymsAPI.Service.Processors
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

            // of
            string of_pattern =
                @"\b(" +
                @"[AaĀā]p[h]*[a]*|" +
                @"[Dd][aeio]*[ls]*|" +
                @"gia|" +
                @"\bhan\b|" +
                @"ja|" +
                @"[Oo]f|" +
                @"[Mm]ạc|" +
                @"ng|" +
                @"[Tt]a|" +
                @"[Tt]hổ|" +
                @"[Tt][ēiī]s|" +
                @"[Tt]o[uy]|" +
                @"van|" +
                @"w|" +
                @"[Yy]r" +
                @")" +
                @"[ ""\'’']";

            // Peninsula (must be before Island)
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
                @"Tübegi|" +
                @"[Yy]arim [Oo]roli|" +
                @"[Yy]arımadası|" +
                @"[Žž]arym [Aa]raly",
                string.Empty);

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

            // Airport
            cleanedName = Regex.Replace(
                cleanedName,
                @"\b[Aa][ei]r[o]*port\b",
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
                @"\b[Aa]r[dei]a[l]*\b",
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
                @"\b[CcGgKk]a[i]*[sz][lt][ei]*[aál][il]*[eoulmn]*[a]*\b|" +
                @"\b[Cc]h[aâ]teau|" +
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
                @"\b[Cc]hurch|" +
                @"[Éé]glise|" +
                @"[Ii]greja|" +
                @"[Kk]yōkai",
                string.Empty);

            // City
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Cc]iud[aá][dt]*|" +
                @"[Cc]ivitas|" +
                @"\b[CcSs](ee|i)[tṭ][tṭ]*[aàeiy]\b|" +
                @"\b[Kk]hot\b|" +
                @"Na[gk]ara[mṁ]*|" +
                @"Oraș(ul)*|" +
                @"a Śahar|" +
                @"Śahara|" +
                @"\bShi\b|" +
                @"\Bsh[iì]\b|" +
                @"Sich’i|" +
                @"[Ss]tadt",
                string.Empty);

            // Cliff
            cleanedName = Regex.Replace(
                cleanedName,
                @"\b[Cc]liff[s]*\b",
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
                @"\bXian\b",
                string.Empty);

            // County
            cleanedName = Regex.Replace(
                cleanedName,
                @"\b[CKck]o[ou]*[mn][eēiī]*[dt](a(do|t)|eía|eth|é|io|y)\b|" +
                @"\b[Cc]omitatu[ls]\b|" +
                @"\b[Dd]aerah\b|" +
                @"eko konderria\b|" +
                @"ga leatna\b|" +
                @"\bili\b|" +
                @"i[n]* lään[i]*\b|" +
                @"\b[Kk][oō][aā]n\b|" +
                @"\bLandgra[a]*fs(cha(ft|p)|tvo)\b|" +
                @"\bLehn\b|" +
                @"megye\b|" +
                @"s [Ll]än\b|" +
                @"s lēne\b|" +
                @"Sir\b",
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
                @"[Mm]a[nṇ][dḍ]ala[m]*\b|" +
                @"Māvaṭṭam|" +
                @"Mnṭqẗ|" +
                @"n piiri|" +
                @"[Pp]asuni|" +
                @"[Pp]iirikunta|" +
                @"[Pp]irrâdâh|" +
                @"Qu(ận)*|" +
                @"[Rr]a[iy]on[iu]|" +
                @"sum",
                string.Empty);

            // Division
            cleanedName = Regex.Replace(
                cleanedName,
                @"\b[Bb][ai][b]*h[aā]g[i]*a[n]*\b|" +
                @"\b[Dd]ivisi[oó][n]*[e]*\b|" +
                @"\b[K]ōṭṭam\b|" +
                @"\bMandal\b|" +
                @"vibhāgaḥ\b",
                string.Empty);

            // Duchy
            cleanedName = Regex.Replace(
                cleanedName,
                @"bǎijué|" +
                @"\bCông quốc\b|" +
                @"\b[Dd][o]*[uüū][cgkq][iy]*[aá]*([dt]*[otu][l]*|eth|h[éy]*|l[ıü]ğ[ıü])\b|" +
                @"\bH[i]*er[t]*[sz]*[iou][o]*(ch|g)[s]*[dt][oöøuv][o]*[m]*(et)*\b|" +
                @"\bKadipaten|" +
                @"\bkunigaikštystė\b",
                string.Empty);

            // Emirate
            cleanedName = Regex.Replace(
                cleanedName,
                @"Aēy Mí Raēy Dtà|" +
                @"\b[ĀāEeÉéƏəIiYy]m[aāi]r[l]*[aàāẗhi][n]*[dğty]*([aeiou][l]*)*|" +
                @"qiúcháng|" +
                @"Saamiro|" +
                @"Tiểu vương quốc|" +
                @"T[’']ohuguk",
                string.Empty);

            // Fort
            cleanedName = Regex.Replace(
                cleanedName,
                @"Benteng|" +
                @"(" +
                @"\b[CcKk][aá]str[aou][lm]*\b|" +
                @"[Cc]héngbǎo|" +
                @"Chillā|" +
                @"[Ff][aäe]st(ni|u)ng(en)*|" +
                @"[Ff][oū]rt([aă][lr]e[a]*[szț]a|[e]*[r]*e[t]*s[s]*[y]*[ae]*|ez[z]*a|e|ikaĵo|ul)*|" +
                @"Kōṭṭai|" +
                @"[Kk]repost|" +
                @"[Tt]rd[i]*n(jav)*a|" +
                @"[Yy]ōsai|" +
                @"[Zz]amogy" +
                @")( (roman|royale))*",
                string.Empty);

            // Hundred
            cleanedName = Regex.Replace(
                cleanedName,
                @"\b[Hh]erred\b|" +
                @"\b[Hh]undred\b|" +
                @"\b[Kk]ihlakunta\b",
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
                @"\Bguó\b|" +
                @"\bIrācciyam\b|" +
                @"\b[Kk][eoö]ni[n]*[gk]r[e]*[iy][cej]*[hk]\b|" +
                @"\bK[iou]ng[ae]*d[oöø]m([m]*et)*[s]*\b|" +
                @"\b[Kk]irályság\b|" +
                @"\b[Kk][o]*r[oaá]l[oe]*[v]*stv[ío]\b|" +
                @"\bŌkoku\b|" +
                @"\bRājy[a]*\b|" +
                @"\b[Rr]egatul\b|" +
                @"\b[Rr][eo][giy][an][eolu][m]*[e]*\b|" +
                @"\b[Rr]īce\b|" +
                @"\b[Tt]eyrnas\b",
                string.Empty);

            // Lake
            cleanedName = Regex.Replace(
                cleanedName,
                @"\b([Jj]e|[Oo])zero\b|" +
                @"\b[Ll][l]*[ay](c|cul|go|ke|n)\b|" +
                @"\b[Nn][uú][u]*r\b|" +
                @"\bGölü\b|" +
                @"\bHu\b",
                string.Empty);

            // Language
            cleanedName = Regex.Replace(
                cleanedName,
                @"\b[Bb][h]*[aā][a]*[sṣ][h]*[aā][a]*\b|" +
                @"\bL[l]*[aeií][mn][g]*[buv]*[ao](ge)*\b",
                string.Empty);

            // Marquisate
            cleanedName = Regex.Replace(
                cleanedName,
                @"Markgr[ae][fv]s((k|ch)a[fp][e]*t|tvo)|" +
                @"Mar[ckq][hu]*[ei][sz][aáà][dt]*[eo]*|" +
                @"hóu\b",
                string.Empty);

            // Mountain
            cleanedName = Regex.Replace(
                cleanedName,
                @"([Gg]e)*[Bb]i[e]*rge[r]*|" +
                @"[Dd]ağları|" +
                @"\b[GgHh][ao]ra\b|" +
                @"Ǧibāl|" +
                @"[Mm][ouū][u]*n[tț][aei]*([gi]*[ln][es]|ii|s)*|" +
                @"[Pp]arvata[ṁ]*|" +
                @"[Ss]hānmài",
                string.Empty);

            // Monastery
            cleanedName = Regex.Replace(
                cleanedName,
                @"[Bb]iara|" +
                @"[Kk]l[aáo][o]*[sš][z]*t[eo]r(is)*\b|" +
                @"\b((R[eo][y]*al|[BV]asilikó) )*[Mm][aăo][i]*[n]*[aăei]*(ĥ|st)[eèḗiíy]*[r]*(e[a]*|[iı]|[ij]o[a]*|o|y)*\b|" +
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
                @"\b[Nn]agara [Ss]abhāva|" +
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
                @"\b[OoÓóŌō][syẏ]*[aáāeē][sz][h]*[aiīeėē][ans]*[uŭ]*\b|" +
                @"Oūh Aēy Sít",
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
                @"[Pp]r[aäeé][e]*fe[ckt]t[uúū]r[ae]*|" +
                @"[Tt]od[oō]fuken\b",
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
                @"\bM([ai]n[tṭţ])*[a]*q[aā][tṭ]*(ʿẗ|h)*\b|" +
                @"[Pp][’]*r[aāou][bpvw][ëií][nñ]*[t]*[csśz]*[eėiíjoy]*[aeėnsz]*|" +
                @"\bPradēśa\b|" +
                @"\bPr[aā][a]*nt[y]*[a]*[ṁ]*\b|" +
                @"\bRát\b|" +
                @"[Ss][h]*[éě]ng|" +
                @"\bShuu\b|" +
                @"\bsuyu\b|" +
                @"\b[Tt]alaith\b|" +
                @"\b[VvWw]il[ao][jy][ae][ht][i]*\b",
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
                @"Afon\b|" +
                @"Darjoi\b|" +
                @"[Ff][il]u(me|viul)|" +
                @"\B[Gg]ang\b|" +
                @"\B[GKgk]awa\b|" +
                @"\Bhé\b|" +
                @"jõgi\b|" +
                @"\b[Kk]ogin\b|" +
                @"\b[Mm]to\b|" +
                @"Nadī\b|" +
                @"N[e]*h[a]*r[i]*\b|" +
                @"Raka\b|" +
                @"\b[Rr][âií][ou][l]*\b|" +
                @"\b[Rr]iver[o]*\b|" +
                @"\b[Ss]het\'|" +
                @"\bSungai\b|" +
                @"\bWenz\b",
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

            // Town
            cleanedName = Regex.Replace(
                cleanedName,
                @"\bBy\b|" +
                @"\bKasabası\b|" +
                @"\bKota\b|" +
                @"Paṭṭaṇaṁ|" +
                @"\b[Ss]tad|" +
                @"\b[TṬtṭ][aāo][v]*[uūw][mn][a]*\b|" +
                @"\Bxiāng\b",
                string.Empty);

            // Township
            cleanedName = Regex.Replace(
                cleanedName,
                @"[CcKk]anton[ae]*(mendua)*|" +
                @"[Tt]ownship|" +
                @"\Bxiāng\b",
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

            // Valley
            cleanedName = Regex.Replace(
                cleanedName,
                @"\bPaḷḷattākku\b|" +
                @"\bValley\b|" +
                @"\bW[aā]d[iī]*\b",
                string.Empty);

            // Voivodeship
            cleanedName = Regex.Replace(
                cleanedName,
                @"V[éo][ij]*[e]*vod[ae]*(s(hip|tv[iío])|t(e|ul))",
                string.Empty);

            cleanedName = Regex.Replace(cleanedName, "^\\s*" + of_pattern, string.Empty);
            cleanedName = Regex.Replace(cleanedName, of_pattern + "\\s*$", string.Empty);

            return cleanedName.Trim();
        }

        private string RemoveLanguageSpecificWords(string languageCode, string name)
        {
            string cleanedName = name;

            if (languageCode.Equals("ang")) // Old English
            {
                cleanedName = Regex.Replace(cleanedName, @"\Benrice\b", "e");
            }

            if (languageCode.Equals("be")) // Belarussian
            {
                cleanedName = Regex.Replace(cleanedName, @"\Bski raion\b", string.Empty); // District
            }

            if (languageCode.Equals("bg")) // Bulgarian
            {
                cleanedName = Regex.Replace(cleanedName, @"\Bski rayon\b", string.Empty); // District
            }

            if (languageCode.Equals("cu")) // Church Slavonic
            {
                cleanedName = Regex.Replace(cleanedName, @"\Bski rajon\b", string.Empty); // District
            }

            if (languageCode.Equals("cv")) // Chuvash
            {
                cleanedName = Regex.Replace(cleanedName, @"\Bskij rajon\b", string.Empty); // District
            }

            if (languageCode.Equals("cz")) // Czech
            {
                cleanedName = Regex.Replace(cleanedName, @"\Bské [Vv]évodství\b", "sko");
            }

            if (languageCode.Equals("en")) // English
            {
                cleanedName = Regex.Replace(cleanedName, @"National Natural Reserve", string.Empty);
                cleanedName = Regex.Replace(cleanedName, @"Tower\b", string.Empty);
            }

            if (languageCode.Equals("hi")) // Hindi
            {
                cleanedName = Regex.Replace(cleanedName, @"\b[Nn]adi\b", string.Empty);
            }

            if (languageCode.Equals("hu")) // Hungarian
            {
                cleanedName = Regex.Replace(cleanedName, @"\b[Tt]artomány\b", string.Empty); // Province. Or Emirate?
            }

            if (languageCode.Equals("it")) // Italian
            {
                cleanedName = Regex.Replace(cleanedName, @"\b[Tt]orre\b", string.Empty); // Tower
            }

            if (languageCode.Equals("fi")) // Finnish
            {
                cleanedName = Regex.Replace(cleanedName, @"\Bin luostari\b", string.Empty);
                cleanedName = Regex.Replace(cleanedName, @"\Bikunta\b", string.Empty);
                cleanedName = Regex.Replace(cleanedName, @"\Bun kunta\b", "u");

                cleanedName = Regex.Replace(cleanedName, @"kunta\b", string.Empty);
            }

            if (languageCode.Equals("ja")) // Japanese
            {
                cleanedName = Regex.Replace(cleanedName, @"\Bbaraki\b", string.Empty);
                cleanedName = Regex.Replace(cleanedName, @"\Bgun\b", string.Empty); // District
                cleanedName = Regex.Replace(cleanedName, @"\Bshū\b", string.Empty); // Province. Or Emirate?
                cleanedName = Regex.Replace(cleanedName, @"Ken\b", string.Empty);
            }

            if (languageCode.Equals("kaa"))
            {
                cleanedName = Regex.Replace(cleanedName, @"U'", "Ú");
            }

            if (languageCode.Equals("kk")) // Kazakh
            {
                cleanedName = Regex.Replace(cleanedName, @"\bawdanı\b", string.Empty); // District
            }

            if (languageCode.Equals("ko")) // Korean
            {
                cleanedName = Regex.Replace(cleanedName, @" Gun\b", string.Empty);
                cleanedName = Regex.Replace(cleanedName, @" Ju\b", string.Empty); // Province. Or Emirate?
                cleanedName = Regex.Replace(cleanedName, @"\B[gj]u\b", string.Empty);
                cleanedName = Regex.Replace(cleanedName, @"\Bhyeon\b", string.Empty); // County
            }

            if (languageCode.Equals("lt")) // Lithuanian
            {
                cleanedName = Regex.Replace(cleanedName, @"\bŠv", "Šventasis");
            }


            if (languageCode.Equals("lv")) // Latvian
            {
                cleanedName = Regex.Replace(cleanedName, @"\Bas mintaka", "a"); // Province. Or Emirate?
            }

            if (languageCode.Equals("ml"))
            {
                cleanedName = Regex.Replace(cleanedName, @"\bBandar\b", string.Empty); // Town
            }

            if (languageCode.Equals("mr"))
            {
                cleanedName = Regex.Replace(cleanedName, @"\bPardeś\b", string.Empty); // Province
            }

            if (languageCode.Equals("ms"))
            {
                cleanedName = Regex.Replace(cleanedName, @"\b[Ll]apangan [Tt]erbang\b", string.Empty); // Airport
            }

            if (languageCode.Equals("pl"))
            {
                cleanedName = Regex.Replace(cleanedName, @"\b[Pp]ort [Ll]otniczy\b", string.Empty); // Airport
                cleanedName = Regex.Replace(cleanedName, @"\b[Mm]yit\b", string.Empty); // River
            }

            if (languageCode.Equals("ru"))
            {
                cleanedName = Regex.Replace(cleanedName, @"\Bskiy rayon\b", string.Empty); // District
            }

            if (languageCode.Equals("sv")) // Swedish
            {
                cleanedName = Regex.Replace(cleanedName, @"s [Hh]ärad\b", string.Empty); // Hundred
            }

            if (languageCode.Equals("uk")) // Ukrainian
            {
                cleanedName = Regex.Replace(cleanedName, @"\Bskyi raion\b", string.Empty); // District
            }

            if (languageCode.Equals("vi")) // Vietnamese
            {
                cleanedName = Regex.Replace(cleanedName, @"\bSông\b", string.Empty); // River
                cleanedName = Regex.Replace(cleanedName, @"\bHuyện", string.Empty);
            }

            // Chinese
            if (languageCode.StartsWith("zh") ||
                languageCode.Equals("cdo") ||
                languageCode.Equals("nan"))
            {
                cleanedName = Regex.Replace(cleanedName, @"-", string.Empty);

                if (languageCode.Equals("nan"))
                {
                    cleanedName = Regex.Replace(cleanedName, @"(chhī|khu)\b", string.Empty);
                }

                if (languageCode.StartsWith("zh"))
                {
                    cleanedName = Regex.Replace(cleanedName, @"\Bcūn\b", string.Empty);
                    cleanedName = Regex.Replace(cleanedName, @"\Bfǔ\b", string.Empty);
                    cleanedName = Regex.Replace(cleanedName, @"\Bgōng\b", string.Empty);
                    cleanedName = Regex.Replace(cleanedName, @"\Bgūjízìránbǎohù\b", string.Empty); // National Natural Reserve
                    cleanedName = Regex.Replace(cleanedName, @"\Bhé\b", string.Empty); // River
                    cleanedName = Regex.Replace(cleanedName, @"\Bhú\b", string.Empty);
                    cleanedName = Regex.Replace(cleanedName, @"\Bhúxiàn\b", string.Empty); // Lake
                    cleanedName = Regex.Replace(cleanedName, @"\Bōu\b", string.Empty);
                    cleanedName = Regex.Replace(cleanedName, @"\Bshì\b", string.Empty);
                    cleanedName = Regex.Replace(cleanedName, @"\Bshìzhēn\b", string.Empty);
                    cleanedName = Regex.Replace(cleanedName, @"\Bsì\b", string.Empty); // Monastery
                    cleanedName = Regex.Replace(cleanedName, @"\Bxiàn\b", string.Empty); // Country
                    cleanedName = Regex.Replace(cleanedName, @"\Bxiāng\b", string.Empty); // Township
                    cleanedName = Regex.Replace(cleanedName, @"\Bzhēn\b", string.Empty);
                    cleanedName = Regex.Replace(cleanedName, @"\Bzhuān\b", string.Empty);
                }
            }

            return cleanedName;
        }
    }
}
