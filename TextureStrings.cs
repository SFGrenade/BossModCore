using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BossModCore
{
    public class TextureStrings
    {
        #region Texture Keys and Files
        // Inventory Items
        public const string InvHornetKey = "Inv_Hornet";
        private const string InvHornetFile = "BossModCore.Resources.Inv_Hornet.png";
        // Bosses
        public const string WeavernPrincessKey = "WeavernPrincess";
        private const string WeavernPrincessFile = "BossModCore.Resources.WeavernPrincess.png";
        // Achievements
        public const string AchievementItemKey = "Achievement_Item";
        private const string AchievementItemFile = "BossModCore.Resources.Achievement_Item.png";
        public const string AchievementBossKey = "Achievement_Boss";
        private const string AchievementBossFile = "BossModCore.Resources.Achievement_Boss.png";
        public const string AchievementWeaverPrincessKey = "Achievement_WeaverPrincess";
        private const string AchievementWeaverPrincessFile = "BossModCore.Resources.Achievement_WeaverPrincess.png";
        #endregion

        private Dictionary<string, Sprite> dict;

        public TextureStrings()
        {
            Assembly _asm = Assembly.GetExecutingAssembly();
            dict = new Dictionary<string, Sprite>();
            string[] tmpTextureFiles = {
                InvHornetFile,
                AchievementItemFile,
                AchievementBossFile,
                AchievementWeaverPrincessFile
            };
            string[] tmpTextureKeys = {
                InvHornetKey,
                AchievementItemKey,
                AchievementBossKey,
                AchievementWeaverPrincessKey
            };
            for (int i = 0; i < tmpTextureFiles.Length; i++)
            {
                using (Stream s = _asm.GetManifestResourceStream(tmpTextureFiles[i]))
                {
                    if (s != null)
                    {
                        byte[] buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                        s.Dispose();

                        //Create texture from bytes
                        var tex = new Texture2D(2, 2);

                        tex.LoadImage(buffer, true);

                        // Create sprite from texture
                        // Split is to cut off the BossModCore.Resources. and the .png
                        dict.Add(tmpTextureKeys[i], Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
                    }
                }
            }
        }

        public Sprite Get(string key)
        {
            return dict[key];
        }
    }
}
