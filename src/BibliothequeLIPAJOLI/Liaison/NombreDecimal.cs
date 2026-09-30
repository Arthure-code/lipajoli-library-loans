using System.Globalization;

namespace BibliothequeLIPAJOLI.Liaison
{
    public static class NombreDecimal
    {
        private const char Virgule = ',';
        private const char Point = '.';

        // Les quatre façons d'espacer les milliers.
        private static readonly char[] Espaces = { ' ', ' ', ' ', '\'' };

        public static bool EssayerDeLire(string? texte, out decimal valeur)
        {
            valeur = 0m;

            if (string.IsNullOrWhiteSpace(texte))
            {
                return false;
            }

            string reste = SansEspaces(texte.Trim());

            string signe = string.Empty;
            if (reste.StartsWith('-') || reste.StartsWith('+'))
            {
                signe = reste[0] == '-' ? "-" : string.Empty;
                reste = reste[1..];
            }

            if (reste.Length == 0 || reste.Any(c => !char.IsAsciiDigit(c) && c != Virgule && c != Point))
            {
                return false;
            }

            if (!EssayerDeNormaliser(reste, out string normalise))
            {
                return false;
            }

            return decimal.TryParse(signe + normalise, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out valeur);
        }

        private static string SansEspaces(string texte)
        {
            foreach (char espace in Espaces)
            {
                texte = texte.Replace(espace.ToString(), string.Empty);
            }

            return texte;
        }

        private static bool EssayerDeNormaliser(string reste, out string normalise)
        {
            int virgules = reste.Count(c => c == Virgule);
            int points = reste.Count(c => c == Point);

            if (virgules > 0 && points > 0)
            {
                // Les deux signes sont présents : le dernier écrit est le
                // séparateur décimal, l'autre groupe les milliers.
                char decimalSeparateur = reste.LastIndexOf(Virgule) > reste.LastIndexOf(Point) ? Virgule : Point;
                char millierSeparateur = decimalSeparateur == Virgule ? Point : Virgule;
                normalise = reste.Replace(millierSeparateur.ToString(), string.Empty)
                                 .Replace(decimalSeparateur, Point);
                return true;
            }

            if (virgules + points == 0)
            {
                normalise = reste;
                return true;
            }

            char separateur = virgules > 0 ? Virgule : Point;

            if (virgules + points > 1)
            {
                // Répété, il ne peut que grouper les milliers : 1.000.000
                normalise = reste.Replace(separateur.ToString(), string.Empty);
                return true;
            }

            return EssayerAvecUnSeulSeparateur(reste, separateur, out normalise);
        }

        private static bool EssayerAvecUnSeulSeparateur(string reste, char separateur, out string normalise)
        {
            normalise = string.Empty;

            int position = reste.IndexOf(separateur);
            int decimales = reste.Length - position - 1;

            if (decimales == 0)
            {
                return false;
            }

            // 10,000 vaut dix mille pour un anglophone et dix pour un
            // francophone : personne ne peut trancher, donc on refuse.
            if (decimales == 3 && position > 0)
            {
                return false;
            }

            normalise = reste.Replace(separateur, Point);
            return true;
        }
    }
}
