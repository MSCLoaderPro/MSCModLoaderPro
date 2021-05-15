using System;
using System.Globalization;
using System.Net;

namespace MSCLoader
{
    /// <summary>Class containing useful features pertaining to Early Access for mods.</summary>
    public class ModEarlyAccess
    {
        /// <summary> Checks the current date and disables the mod if the date is outside specified date interval. </summary>
        /// <param name="mod">Mod in question.</param>
        /// <param name="startDate">Starting date of the interval.</param>
        /// <param name="endDate">End date of the interval.</param>
        /// <param name="disableMessage">Message to show the user if outside the date interval.</param>
        /// <param name="onlineTimeOnly">Check online time only.</param>
        public static void CheckDateAndDisable(Mod mod, string startDate, string endDate, string disableMessage = "", bool onlineTimeOnly = false)
        {
            if (!CheckDate(startDate, endDate, onlineTimeOnly)) 
                DisableMod(mod, disableMessage);
        }
        /// <summary> Check if the current date is outside the specified date interval</summary>
        /// <param name="startDate">Starting date of the interval.</param>
        /// <param name="endDate">End date of the interval.</param>
        /// <param name="onlineTimeOnly">Check online time only.</param>
        /// <returns>Whether or not the current date is outside the specified date interval.</returns>
        public static bool CheckDate(string startDate, string endDate, bool onlineTimeOnly = false)
        {
            if (onlineTimeOnly)
            {
                DateTime? date = GetOnlineDate();
                if (date == null) return false;

                if (date >= DateTime.Parse($"{startDate} 00:00:00Z") && date <= DateTime.Parse($"{endDate} 23:59:59Z"))
                    return true;

                return false;
            }

            if (ModLoader.Date >= DateTime.Parse($"{startDate} 00:00:00Z") && ModLoader.Date <= DateTime.Parse($"{endDate} 23:59:59Z"))
                return true;

            return false;
        }
        /// <summary> Disables a mod completely, hiding it from the mod list.</summary>
        /// <param name="mod">Mod in question.</param>
        /// <param name="disableMessage">Message to show the user.</param>
        public static void DisableMod(Mod mod, string disableMessage = "")
        {
            mod.modListElement.gameObject.SetActive(false);
            mod.modSettings.gameObject.SetActive(false);
            mod.enabled = false;
            ModPrompt.CreatePrompt($"{mod.Name}\n\nTHIS MOD IS AN EARLY ACCESS MOD AND ITS TESTING PERIOD HAS ENDED.\n\nIT HAS BEEN DISABLED.\n\n{disableMessage}", "EARLY ACCESS MOD!");
        }
        /// <summary> Get the current date, online with offline backup.</summary>
        /// <returns>The current date.</returns>
        public static DateTime GetDate()
        {
            try
            {
                using (var response = WebRequest.Create("http://www.google.com").GetResponse())
                    return DateTime.ParseExact(response.Headers["date"], "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal).ToLocalTime();
            }
            catch { return DateTime.Now; }
        }
        /// <summary> Get the current date, online.</summary>
        /// <returns>The current date.</returns>
        public static DateTime? GetOnlineDate()
        {
            try
            {
                using (var response = WebRequest.Create("http://www.google.com").GetResponse())
                    return DateTime.ParseExact(response.Headers["date"], "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal).ToLocalTime();
            }
            catch { return null; }
        }
    }
}
