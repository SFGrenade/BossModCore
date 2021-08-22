using System.Collections.Generic;

namespace BossModCore
{
    class LanguageStrings
    {
        private const string Br = "\n";
        private const string Pg = Br + "<page>";
        #region Language Strings
        // Mechanics
        // Bosses
        // Places
        public const string AreaTitleEvent = "BossModCore_AreaTitle";
        public const string AreaTitleSuperKey = "BossModCore_AreaTitle_SUPER";
        private const string AreaTitleSuperVal = "";
        public const string AreaTitleMainKey = "BossModCore_AreaTitle_MAIN";
        private const string AreaTitleMainVal = "";
        public const string AreaTitleSubKey = "BossModCore_AreaTitle_SUB";
        private const string AreaTitleSubVal = "";
        // Credits
        public const string CreditsTabletTextKey = "BossModCore_CreditsTablet";
        private const string CreditsTabletTextVal = "Credits" + Pg +
                                                     "Person:" + Br + "Text" + Pg;
        #endregion
        #region Achievement Strings
        // Bosses
        #endregion

        private readonly Dictionary<string, string> _dict;

        public LanguageStrings()
        {
            // Initialize LangString Dictionary
            _dict = new Dictionary<string, string>();
            // Inventory Items
            // Boss Names
            // Places
            _dict.Add(AreaTitleSuperKey, AreaTitleSuperVal);
            _dict.Add(AreaTitleMainKey, AreaTitleMainVal);
            _dict.Add(AreaTitleSubKey, AreaTitleSubVal);
            _dict.Add(CreditsTabletTextKey, CreditsTabletTextVal);

            // Achievements
            // Bosses
        }

        public string Get(string key)
        {
            return _dict[key];
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }
    }
}
