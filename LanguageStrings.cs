using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BossModCore
{
    class LanguageStrings
    {
        private const string br = "\n";
        private const string pg = br + "<page>";
        #region Language Strings
        // Mechanics
        // Bosses
        // Places
        public const string AreaTitle_Event = "BossModCore_AreaTitle";
        public const string AreaTitleSuper_Key = "BossModCore_AreaTitle_SUPER";
        private const string AreaTitleSuper_Val = "Test of";
        public const string AreaTitleMain_Key = "BossModCore_AreaTitle_MAIN";
        private const string AreaTitleMain_Val = "Teamwork";
        public const string AreaTitleSub_Key = "BossModCore_AreaTitle_SUB";
        private const string AreaTitleSub_Val = "";
        // Credits
        public const string CreditsTabletText_Key = "BossModCore_CreditsTablet";
        private const string CreditsTabletText_Val = "Credits" + pg +
                                                     "Person:" + br + "Text" + pg;
        #endregion
        #region Achievement Strings
        // Bosses
        #endregion

        private Dictionary<string, string> dict;

        public LanguageStrings()
        {
            // Initialize LangString Dictionary
            dict = new Dictionary<string, string>();
            // Inventory Items
            // Boss Names
            // Places
            dict.Add(AreaTitleSuper_Key, AreaTitleSuper_Val);
            dict.Add(AreaTitleMain_Key, AreaTitleMain_Val);
            dict.Add(AreaTitleSub_Key, AreaTitleSub_Val);
            dict.Add(CreditsTabletText_Key, CreditsTabletText_Val);

            // Achievements
            // Bosses
        }

        public string Get(string key)
        {
            return dict[key];
        }

        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(key);
        }
    }
}
