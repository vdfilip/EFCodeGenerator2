using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EFCodeGenerator.Logic
{
    public static class Inflector
    {
        #region Rules

        private static readonly List<InflectorRule> Plurals;
        private static readonly List<InflectorRule> Singulars;
        private static readonly List<string> Uncountables;

        private sealed class InflectorRule
        {
            private readonly Regex _regex;
            private readonly string _replacement;

            public InflectorRule(string regexPattern, string replacementText)
            {
                _regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
                _replacement = replacementText;
            }

            public string Apply(string word)
            {
                if (!_regex.IsMatch(word))
                    return null;

                var replace = _regex.Replace(word, _replacement);
                return replace;
            }
        }

        #endregion

        #region Construction and initialization

        static Inflector()
        {
            Plurals = new List<InflectorRule>();
            Singulars = new List<InflectorRule>();
            Uncountables = new List<string>();

            AddPluralRule("$", "s");
            AddPluralRule("s$", "s");
            AddPluralRule("(ax|test)is$", "$1es");
            AddPluralRule("(octop|vir)us$", "$1i");
            AddPluralRule("(alias|status)$", "$1es");
            AddPluralRule("(bu)s$", "$1ses");
            AddPluralRule("(buffal|tomat)o$", "$1oes");
            AddPluralRule("([ti])um$", "$1a");
            AddPluralRule("sis$", "ses");
            AddPluralRule("(?:([^f])fe|([lr])f)$", "$1$2ves");
            AddPluralRule("(hive)$", "$1s");
            AddPluralRule("([^aeiouy]|qu)y$", "$1ies");
            AddPluralRule("(x|ch|ss|sh)$", "$1es");
            AddPluralRule("(matr|vert|ind)ix|ex$", "$1ices");
            AddPluralRule("([m|l])ouse$", "$1ice");
            AddPluralRule("^(ox)$", "$1en");
            AddPluralRule("(quiz)$", "$1zes");

            AddSingularRule("s$", "");
            AddSingularRule("ss$", "ss");
            AddSingularRule("(n)ews$", "$1ews");
            AddSingularRule("([ti])a$", "$1um");
            AddSingularRule("((a)naly|(b)a|(d)iagno|(p)arenthe|(p)rogno|(s)ynop|(t)he)ses$", "$1$2sis");
            AddSingularRule("(^analy)ses$", "$1sis");
            AddSingularRule("([^f])ves$", "$1fe");
            AddSingularRule("(hive)s$", "$1");
            AddSingularRule("(tive)s$", "$1");
            AddSingularRule("([lr])ves$", "$1f");
            AddSingularRule("([^aeiouy]|qu)ies$", "$1y");
            AddSingularRule("(s)eries$", "$1eries");
            AddSingularRule("(m)ovies$", "$1ovie");
            AddSingularRule("(x|ch|ss|sh)es$", "$1");
            AddSingularRule("([m|l])ice$", "$1ouse");
            AddSingularRule("(bus)es$", "$1");
            AddSingularRule("(o)es$", "$1");
            AddSingularRule("(shoe)s$", "$1");
            AddSingularRule("(cris|ax|test)es$", "$1is");
            AddSingularRule("(octop|vir)i$", "$1us");
            AddSingularRule("(alias|status)$", "$1");
            AddSingularRule("(alias|status)es$", "$1");
            AddSingularRule("^(ox)en", "$1");
            AddSingularRule("(vert|ind)ices$", "$1ex");
            AddSingularRule("(matr)ices$", "$1ix");
            AddSingularRule("(quiz)zes$", "$1");

            AddIrregularRule("agendum", "agenda");
            AddIrregularRule("child", "children");
            AddIrregularRule("man", "men");
            AddIrregularRule("move", "moves");
            AddIrregularRule("person", "people");
            AddIrregularRule("sex", "sexes");
            AddIrregularRule("tax", "taxes");
            AddIrregularRule("tooth", "teeth");

            AddUnknownCountRule("equipment");
            AddUnknownCountRule("information");
            AddUnknownCountRule("rice");
            AddUnknownCountRule("money");
            AddUnknownCountRule("species");
            AddUnknownCountRule("series");
            AddUnknownCountRule("fish");
            AddUnknownCountRule("sheep");
        }

        private static void AddPluralRule(string rule, string replacement)
        {
            Plurals.Add(new InflectorRule(rule, replacement));
        }

        private static void AddSingularRule(string rule, string replacement)
        {
            Singulars.Add(new InflectorRule(rule, replacement));
        }

        private static void AddIrregularRule(string singular, string plural)
        {
            AddPluralRule("(" + singular[0] + ")" + singular.Substring(1) + "$",
                "$1" + plural.Substring(1));

            AddSingularRule("(" + plural[0] + ")" + plural.Substring(1) + "$",
                "$1" + singular.Substring(1));
        }

        private static void AddUnknownCountRule(string word)
        {
            Uncountables.Add(word.ToLower());
        }

        #endregion

        #region Inflection

        public static string ToPlural(string word)
        {
            return ApplyRules(Plurals, word);
        }

        public static string ToSingular(string word)
        {
            try
            {
                return ApplyRules(Singulars, word);
            }
            catch (ArgumentNullException ex)
            {
                throw CustomException.Create(ex, "Failed to Singularize Word: {0}", word ?? "(null)");
            }
        }

        private static string ApplyRules(IList<InflectorRule> rules, string word)
        {
            // Set the preconditions for this function.

            if (rules == null)
                throw new ArgumentNullException("rules");

            if (word == null)
                throw new ArgumentNullException("word");

            // Apply the inflection rules to the word.

            var result = word;
            if (Uncountables == null || !Uncountables.Contains(word.ToLower()))
            {
                for (var i = rules.Count - 1; i >= 0; i--)
                {
                    var temp = rules[i].Apply(word);
                    if (temp != null)
                    {
                        result = temp;
                        break;
                    }
                }
            }
            return result;
        }

        #endregion
    }
}