using System;
using System.Text.RegularExpressions;

namespace Hp.Merlin.Services
{
    public static class NamingConventions
    {
        private static readonly Regex ValidStrategyNameRegex = new Regex("^[_a-zA-Z][_a-zA-Z0-9]{0,31}$");

        public static bool IsValidIdentifier(string identifier)
        {
            return ValidStrategyNameRegex.IsMatch(identifier);
        }

        public static void CheckIdentifier(string identifier, string name)
        {
            if (identifier == null) throw new ArgumentNullException(name);
            if (!IsValidIdentifier(identifier)) throw new ArgumentException(string.Format("'{0}' is not a valid identifier for {1}", identifier, name));
        }
    }
}
