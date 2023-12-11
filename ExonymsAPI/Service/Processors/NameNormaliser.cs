using System.Text.RegularExpressions;

namespace ExonymsAPI.Service.Processors
{
    public class NameNormaliser : INameNormaliser
    {
        public string Normalise(string languageCode, string name)
        {
            string normalisedName = name;

            normalisedName = RemoveTextPattern(normalisedName, " - .*");
            normalisedName = RemoveTextPattern(normalisedName, ",.*");
            normalisedName = RemoveTextPattern(normalisedName, "[…]");
            normalisedName = RemoveTextPattern(normalisedName, "/.*");
            normalisedName = RemoveTextPattern(normalisedName, "\\(.*");
            normalisedName = RemoveTextPattern(normalisedName, "\\s*<alternateName .*$");
            normalisedName = RemoveTextPattern(normalisedName, "^\"(.*)\"$", "$1");
            normalisedName = RemoveTextPattern(normalisedName, @"^[^\s]*:");

            normalisedName = RemoveWords(normalisedName);
            normalisedName = RemoveLanguageSpecificWords(languageCode, normalisedName);

            normalisedName = RemoveTextPattern(normalisedName, @"\s\s*", " ");
            normalisedName = RemoveTextPattern(normalisedName, @"^[\s\-]*");
            normalisedName = RemoveTextPattern(normalisedName, @"[\s\-]*$");
            normalisedName = normalisedName.Trim();

            return normalisedName;
        }

        private string RemoveWords(string name)
        {
            string cleanName = name;

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
            cleanName = RemoveTextPattern(cleanName,
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
                @"[Žž]arym [Aa]raly");

            // Abbey
            cleanName = RemoveTextPattern(cleanName,
                @"[AaOo][bp][abd]([aet][z]*[iy][ae]*|[ií][aj]|(c|ts)[t]*[vw][oí])|" +
                @"Benediktinerabtei");

            // Agency
            cleanName = RemoveTextPattern(cleanName,
                @"[Aa]gen[cț][ijy][a]*");

            // Airport
            cleanName = RemoveTextPattern(cleanName,
                @"\b[Aa][ei]r[o]*port\b");

            // Ancient
            cleanName = RemoveTextPattern(cleanName,
                @"[Aa]ncient|" +
                @"Antiikin [Aa]nti[i]*[ck](a|in)*|" +
                @"Ar[c]*ha[ií][ac]");

            // Area
            cleanName = RemoveTextPattern(cleanName,
                @"\b[Aa]r[dei]a[l]*\b");

            // Autonomous Government
            cleanName = RemoveTextPattern(cleanName,
                @"[Aa][uv]tonom(e|\noye|ous) ([Gg]overnment|[Pp]ravitel’stvo|[Rr]egering)|" +
                @"[Gg]obierno [Aa]ut[oó]nomo|" +
                @"[Öö]zerk [Hh]ükümeti");

            // Canton
            cleanName = RemoveTextPattern(cleanName,
                @"[CcKk][’]*[hy]*[aāe][i]*[nṇ][tṭ][’]*[aoóuū]n(a|i|o|s|u[l]*)*");

            // Castle
            cleanName = RemoveTextPattern(cleanName,
                @"\b[CcGgKk]a[i]*[sz][lt][ei]*[aál][il]*[eoulmn]*[a]*\b|" +
                @"\b[Cc]h[aâ]teau|" +
                @"Dvorac|" +
                @"[KkQq]al[ae]s[iı]|" +
                @"Z[aá]m[aeo][gk][y]*");

            // Cathedral
            cleanName = RemoveTextPattern(cleanName,
                @"[CcKk]at[h]*[eé]dr[ai][kl][aeoó]*[s]*");

            // Church
            cleanName = RemoveTextPattern(cleanName,
                @"[Bb]iserica|" +
                @"[Cc]hiesa|" +
                @"\b[Cc]hurch|" +
                @"[Éé]glise|" +
                @"[Ii]greja|" +
                @"[Kk]yōkai");

            // City
            cleanName = RemoveTextPattern(cleanName,
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
                @"[Ss]tadt");

            // Cliff
            cleanName = RemoveTextPattern(cleanName,
                @"\b[Cc]liff[s]*\b");

            // Commune
            cleanName = RemoveTextPattern(cleanName,
                @"[CcKk]om[m]*un[ae]*|" +
                @"[Kk]özség");

            // Council
            cleanName = RemoveTextPattern(cleanName,
                @"[Cc]o[u]*n[cs][ei]l[l]*(iul)|" +
                @"[Cc]omhairle");

            // Country
            cleanName = RemoveTextPattern(cleanName,
                @"[Cc]ontea|" +
                @"[Jj]ude[tț]ul|" +
                @"[Nn]egeri|" +
                @"\bXian\b");

            // County
            cleanName = RemoveTextPattern(cleanName,
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
                @"Sir\b");

            // Department
            cleanName = RemoveTextPattern(cleanName,
                @"[DdḌḍ][eéi]p[’]*[aā][i]*r[tṭ][’]*[aei]*m[aeēi][e]*[nṇ]*[gtṭ]*[’]*(as|i|o|u(l|va)*)*|" +
                @"Ilākhe|" +
                @"Penbiran|" +
                @"Tuṟai|" +
                @"Vibhaaga|" +
                @"Zhang Wàt");

            // Desert
            cleanName = RemoveTextPattern(cleanName,
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
                @"Vaalvnt");

            // Diocese
            cleanName = RemoveTextPattern(cleanName,
                @"[Dd]io[eít]*[cks][eēi][sz][eēi]*[s]*");

            // District
            cleanName = RemoveTextPattern(cleanName,
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
                @"sum");

            // Division
            cleanName = RemoveTextPattern(cleanName,
                @"\b[Bb][ai][b]*h[aā]g[i]*a[n]*\b|" +
                @"\b[Dd]ivisi[oó][n]*[e]*\b|" +
                @"\b[K]ōṭṭam\b|" +
                @"\bMandal\b|" +
                @"vibhāgaḥ\b");

            // Duchy
            cleanName = RemoveTextPattern(cleanName,
                @"bǎijué|" +
                @"\bCông quốc\b|" +
                @"\b[Dd][o]*[uüū][cgkq][iy]*[aá]*([dt]*[otu][l]*|eth|h[éy]*|l[ıü]ğ[ıü])\b|" +
                @"\bH[i]*er[t]*[sz]*[iou][o]*(ch|g)[s]*[dt][oöøuv][o]*[m]*(et)*\b|" +
                @"\bKadipaten|" +
                @"\bkunigaikštystė\b");

            // Emirate
            cleanName = RemoveTextPattern(cleanName,
                @"Aēy Mí Raēy Dtà|" +
                @"\b[ĀāEeÉéƏəIiYy]m[aāi]r[l]*[aàāẗhi][n]*[dğty]*([aeiou][l]*)*|" +
                @"qiúcháng|" +
                @"Saamiro|" +
                @"Tiểu vương quốc|" +
                @"T[’']ohuguk");

            // Fort
            cleanName = RemoveTextPattern(cleanName,
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
                @")( (roman|royale))*");

            // Hundred
            cleanName = RemoveTextPattern(cleanName,
                @"\b[Hh]erred\b|" +
                @"\b[Hh]undred\b|" +
                @"\b[Kk]ihlakunta\b");

            // Island
            cleanName = RemoveTextPattern(cleanName,
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
                @"Sŏm");

            // Kingdom
            cleanName = RemoveTextPattern(cleanName,
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
                @"\b[Tt]eyrnas\b");

            // Lake
            cleanName = RemoveTextPattern(cleanName,
                @"\b([Jj]e|[Oo])zero\b|" +
                @"\b[Ll][l]*[ay](c|cul|go|ke|n)\b|" +
                @"\b[Nn][uú][u]*r\b|" +
                @"\bGölü\b|" +
                @"\bHu\b");

            // Language
            cleanName = RemoveTextPattern(cleanName,
                @"\b[Bb][h]*[aā][a]*[sṣ][h]*[aā][a]*\b|" +
                @"\bL[l]*[aeií][mn][g]*[buv]*[ao](ge)*\b");

            // Marquisate
            cleanName = RemoveTextPattern(cleanName,
                @"Markgr[ae][fv]s((k|ch)a[fp][e]*t|tvo)|" +
                @"Mar[ckq][hu]*[ei][sz][aáà][dt]*[eo]*|" +
                @"hóu\b");

            // Mountain
            cleanName = RemoveTextPattern(cleanName,
                @"([Gg]e)*[Bb]i[e]*rge[r]*|" +
                @"[Dd]ağları|" +
                @"\b[GgHh][ao]ra\b|" +
                @"Ǧibāl|" +
                @"[Mm][ouū][u]*n[tț][aei]*([gi]*[ln][es]|ii|s)*|" +
                @"[Pp]arvata[ṁ]*|" +
                @"[Ss]hānmài");

            // Monastery
            cleanName = RemoveTextPattern(cleanName,
                @"[Bb]iara|" +
                @"[Kk]l[aáo][o]*[sš][z]*t[eo]r(is)*\b|" +
                @"\b((R[eo][y]*al|[BV]asilikó) )*[Mm][aăo][i]*[n]*[aăei]*(ĥ|st)[eèḗiíy]*[r]*(e[a]*|[iı]|[ij]o[a]*|o|y)*\b|" +
                @"[Ss]amostan|" +
                @"[Ss]hu[u]*dōin");

            // Municipium
            cleanName = RemoveTextPattern(cleanName,
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
                @"[Ss]avivaldybė");

            // Municipality
            cleanName = RemoveTextPattern(cleanName,
                @"Bwrdeistref|" +
                @"Concello|" +
                @"D[ḗií]mos|" +
                @"[Gg][e]*m[e]*[ij]*n[dt]*[ae]|" +
                @"gielda|" +
                @"O[bp]([cćčš]|s[hj])[t]*ina|" +
                @"udalerria");

            // National Park
            cleanName = RemoveTextPattern(cleanName,
                @"[Nn]ational [Pp]ark|" +
                @"Par[cq]u[el] Na[ctț]ional|" +
                @"[Vv]ườn [Qq]uốc");

            // Oasis
            cleanName = RemoveTextPattern(cleanName,
                @"[aā]l-[Ww]āḥāt|" +
                @"\b[OoÓóŌō][syẏ]*[aáāeē][sz][h]*[aiīeėē][ans]*[uŭ]*\b|" +
                @"Oūh Aēy Sít");

            // Plateau
            cleanName = RemoveTextPattern(cleanName,
                @"Alt[io]p[il]*[aà](no)*|" +
                @"Àrd-thìr|" +
                @"Daichi|" +
                @"gāoyuán|" +
                @"Hḍbẗ|" +
                @"ordokia|" +
                @"[Pp][’]*lat[’]*[e]*([aå][nu](et)*|o(s[iu])*)|" +
                @"[Pp]lošina|" +
                @"[Pp]lynaukštė");

            // Prefecture
            cleanName = RemoveTextPattern(cleanName,
                @"[Pp]r[aäeé][e]*fe[ckt]t[uúū]r[ae]*|" +
                @"[Tt]od[oō]fuken\b");

            // Province
            cleanName = RemoveTextPattern(cleanName,
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
                @"\b[VvWw]il[ao][jy][ae][ht][i]*\b");

            // Region
            cleanName = RemoveTextPattern(cleanName,
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
                @"[Rr]ijn");

            // Republic
            cleanName = RemoveTextPattern(cleanName,
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
                @"[Tt]a[sz][ao]val[dt](a|kund)");

            // River
            cleanName = RemoveTextPattern(cleanName,
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
                @"\bWenz\b");

            // Ruin
            cleanName = RemoveTextPattern(cleanName,
                @"[Rr]uin[ae]*");

            // State
            cleanName = RemoveTextPattern(cleanName,
                @"Bang|" +
                @"[EeÉéIi]*[SsŜŝŜŝŠšŞş][h]*[tṭ][’]*[aeē][dtṭ]([aeiıos]|ul)|" +
                @"[Oo]sariik|" +
                @"[Oo]st[’]*an[ıi]|" +
                @"Ūlāīẗ|" +
                @"[Uu]stoni|" +
                @"valstija*");

            // Temple
            cleanName = RemoveTextPattern(cleanName,
                @"[Dd]ēvālaya(mu)*|" +
                @"[Kk]ōvil|" +
                @"[Mm][a]*ndir[a]*|" +
                @"Ná Tiān|" +
                @"[Pp]agoda|" +
                @"[Tt]emp[e]*l[eou]*[l]*");

            // Territory
            cleanName = RemoveTextPattern(cleanName,
                @"Chunju|" +
                @"Iqlīm|" +
                @"Lãnh|" +
                @"Léng-thó͘|" +
                @"lǐngde|" +
                @"Lurraldea|" +
                @"[Tt]er[r]*[iy]t[oó]r[iy][iy]*[r]*[aeou]*[mt]*");

            // Town
            cleanName = RemoveTextPattern(cleanName,
                @"\bBy\b|" +
                @"\bKasabası\b|" +
                @"\bKota\b|" +
                @"Paṭṭaṇaṁ|" +
                @"\b[Ss]tad|" +
                @"\b[TṬtṭ][aāo][v]*[uūw][mn][a]*\b|" +
                @"\Bxiāng\b");

            // Township
            cleanName = RemoveTextPattern(cleanName,
                @"[CcKk]anton[ae]*(mendua)*|" +
                @"[Tt]ownship|" +
                @"\Bxiāng\b");

            // University
            cleanName = RemoveTextPattern(cleanName,
                @"[Dd]aigaku|" +
                @"(Lā )*[BbVv]i[sś][h]*[vw]\+(a[bv])*idyāla[yẏ][a]*[ṁ]*|" +
                @"[Oo]llscoil|" +
                @"[Uu]niversit(ate[a]a*|y)|" +
                @"[Vv]idyaapith");

            // Valley
            cleanName = RemoveTextPattern(cleanName,
                @"\bPaḷḷattākku\b|" +
                @"\bValley\b|" +
                @"\bW[aā]d[iī]*\b");

            // Voivodeship
            cleanName = RemoveTextPattern(cleanName,
                @"V[éo][ij]*[e]*vod[ae]*(s(hip|tv[iío])|t(e|ul))");

            cleanName = RemoveTextPattern(cleanName, "^\\s*" + of_pattern);
            cleanName = RemoveTextPattern(cleanName, of_pattern + "\\s*$");

            return cleanName.Trim();
        }

        private string RemoveLanguageSpecificWords(string languageCode, string name)
        {
            string cleanName = name;

            if (languageCode.Equals("ang")) // Old English
            {
                cleanName = RemoveTextPattern(cleanName, @"\Benrice\b", "e");
            }

            if (languageCode.Equals("be")) // Belarussian
            {
                cleanName = RemoveTextPattern(cleanName, @"\Bski raion\b"); // District
            }

            if (languageCode.Equals("bg")) // Bulgarian
            {
                cleanName = RemoveTextPattern(cleanName, @"\Bski rayon\b"); // District
            }

            if (languageCode.Equals("cu")) // Church Slavonic
            {
                cleanName = RemoveTextPattern(cleanName, @"\Bski rajon\b"); // District
            }

            if (languageCode.Equals("cv")) // Chuvash
            {
                cleanName = RemoveTextPattern(cleanName, @"\Bskij rajon\b"); // District
            }

            if (languageCode.Equals("cz")) // Czech
            {
                cleanName = RemoveTextPattern(cleanName, @"\Bské [Vv]évodství\b", "sko");
            }

            if (languageCode.Equals("en")) // English
            {
                cleanName = RemoveTextPattern(cleanName, @"National Natural Reserve");
                cleanName = RemoveTextPattern(cleanName, @"Tower\b");
            }

            if (languageCode.Equals("hi")) // Hindi
            {
                cleanName = RemoveTextPattern(cleanName, @"\b[Nn]adi\b");
            }

            if (languageCode.Equals("hu")) // Hungarian
            {
                cleanName = RemoveTextPattern(cleanName, @"\b[Tt]artomány\b"); // Province. Or Emirate?
            }

            if (languageCode.Equals("id")) // Indonesian
            {
                cleanName = RemoveTextPattern(cleanName, @"\b[Kk]abupaten\b"); // County
            }

            if (languageCode.Equals("it")) // Italian
            {
                cleanName = RemoveTextPattern(cleanName, @"\b[Tt]orre\b"); // Tower
            }

            if (languageCode.Equals("fi")) // Finnish
            {
                cleanName = RemoveTextPattern(cleanName, @"\Bin luostari\b");
                cleanName = RemoveTextPattern(cleanName, @"\Bikunta\b");
                cleanName = RemoveTextPattern(cleanName, @"\Bun kunta\b", "u");

                cleanName = RemoveTextPattern(cleanName, @"kunta\b");
            }

            if (languageCode.Equals("ja")) // Japanese
            {
                cleanName = RemoveTextPattern(cleanName, @"\Bbaraki\b");
                cleanName = RemoveTextPattern(cleanName, @"\Bgun\b"); // District
                cleanName = RemoveTextPattern(cleanName, @"\Bshū\b"); // Province. Or Emirate?
                cleanName = RemoveTextPattern(cleanName, @"Ken\b");
            }

            if (languageCode.Equals("kaa"))
            {
                cleanName = RemoveTextPattern(cleanName, @"U'", "Ú");
            }

            if (languageCode.Equals("kk")) // Kazakh
            {
                cleanName = RemoveTextPattern(cleanName, @"\bawdanı\b"); // District
            }

            if (languageCode.Equals("ko")) // Korean
            {
                cleanName = RemoveTextPattern(cleanName, @" Gun\b");
                cleanName = RemoveTextPattern(cleanName, @" Ju\b"); // Province. Or Emirate?
                cleanName = RemoveTextPattern(cleanName, @"\B[gj]u\b");
                cleanName = RemoveTextPattern(cleanName, @"\Bhyeon\b"); // County
            }

            if (languageCode.Equals("lt")) // Lithuanian
            {
                cleanName = RemoveTextPattern(cleanName, @"\bŠv", "Šventasis");
            }


            if (languageCode.Equals("lv")) // Latvian
            {
                cleanName = RemoveTextPattern(cleanName, @"\Bas mintaka", "a"); // Province. Or Emirate?
            }

            if (languageCode.Equals("ml"))
            {
                cleanName = RemoveTextPattern(cleanName, @"\bBandar\b"); // Town
            }

            if (languageCode.Equals("mr")) // Marathi
            {
                cleanName = RemoveTextPattern(cleanName, @"\bṄgar\b"); // Town
                cleanName = RemoveTextPattern(cleanName, @"\bPardeś\b"); // Province
            }

            if (languageCode.Equals("ms")) // Malay
            {
                cleanName = RemoveTextPattern(cleanName, @"\b[Bb]andar\b"); // Town
                cleanName = RemoveTextPattern(cleanName, @"\b[Ll]apangan [Tt]erbang\b"); // Airport
            }

            if (languageCode.Equals("pl"))
            {
                cleanName = RemoveTextPattern(cleanName, @"\b[Pp]ort [Ll]otniczy\b"); // Airport
                cleanName = RemoveTextPattern(cleanName, @"\b[Mm]yit\b"); // River
            }

            if (languageCode.Equals("ru"))
            {
                cleanName = RemoveTextPattern(cleanName, @"\Bskiy rayon\b"); // District
            }

            if (languageCode.Equals("sv")) // Swedish
            {
                cleanName = RemoveTextPattern(cleanName, @"s [Hh]ärad\b"); // Hundred
            }

            if (languageCode.Equals("uk")) // Ukrainian
            {
                cleanName = RemoveTextPattern(cleanName, @"\Bskyi raion\b"); // District
            }

            if (languageCode.Equals("vi")) // Vietnamese
            {
                cleanName = RemoveTextPattern(cleanName, @"\bSông\b"); // River
                cleanName = RemoveTextPattern(cleanName, @"\bHuyện\b");
                cleanName = RemoveTextPattern(cleanName, @"\bTrấn\b"); // Town
            }

            // Chinese
            if (languageCode.StartsWith("zh") ||
                languageCode.Equals("cdo") ||
                languageCode.Equals("nan"))
            {
                cleanName = RemoveTextPattern(cleanName, @"-");

                if (languageCode.Equals("nan"))
                {
                    cleanName = RemoveTextPattern(cleanName, @"(chhī|khu)\b");
                }

                if (languageCode.StartsWith("zh"))
                {
                    cleanName = RemoveTextPattern(cleanName, @"\Bcūn\b");
                    cleanName = RemoveTextPattern(cleanName, @"\Bfǔ\b");
                    cleanName = RemoveTextPattern(cleanName, @"\Bgōng\b");
                    cleanName = RemoveTextPattern(cleanName, @"\Bgūjízìránbǎohù\b"); // National Natural Reserve
                    cleanName = RemoveTextPattern(cleanName, @"\Bhé\b"); // River
                    cleanName = RemoveTextPattern(cleanName, @"\Bhú\b");
                    cleanName = RemoveTextPattern(cleanName, @"\Bhúxiàn\b"); // Lake
                    cleanName = RemoveTextPattern(cleanName, @"\Bōu\b");
                    cleanName = RemoveTextPattern(cleanName, @"\Bshì\b");
                    cleanName = RemoveTextPattern(cleanName, @"\Bshìzhēn\b");
                    cleanName = RemoveTextPattern(cleanName, @"\Bsì\b"); // Monastery
                    cleanName = RemoveTextPattern(cleanName, @"\Bxiàn\b"); // Country
                    cleanName = RemoveTextPattern(cleanName, @"\Bxiāng\b"); // Township
                    cleanName = RemoveTextPattern(cleanName, @"\Bzhēn\b");
                    cleanName = RemoveTextPattern(cleanName, @"\Bzhuān\b");
                }
            }

            return cleanName;
        }

        private string RemoveTextPattern(string text, string pattern)
            => RemoveTextPattern(text, pattern, string.Empty);

        private string RemoveTextPattern(string text, string pattern, string replacement)
            => Regex.Replace(text, pattern, replacement);
    }
}
