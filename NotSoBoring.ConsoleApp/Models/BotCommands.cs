using System.Collections.Generic;
using System.Linq;

namespace NotSoBoring.ConsoleApp.Models
{
    public static class BotCommands
    {
        private static readonly Dictionary<string, string> _commands = new Dictionary<string, string>()
        {
            {"Profile 🧑‍💻", "profile" },
            {"Contacts 📖", "contacts" },
        };

        public static IEnumerable<KeyValuePair<string, string>> Commands => _commands.ToList();
    }
}
