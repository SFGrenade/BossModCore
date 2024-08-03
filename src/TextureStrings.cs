using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BossModCore;

public class TextureStrings
{

    private Dictionary<string, Sprite> _dict;

    public TextureStrings()
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        _dict = new Dictionary<string, Sprite>();
        string[] tmpTextureFiles = {
        };
        string[] tmpTextureKeys = {
        };
        for (var i = 0; i < tmpTextureFiles.Length; i++)
        {
            using var s = asm.GetManifestResourceStream(tmpTextureFiles[i]);
            if (s == null) continue;
            var buffer = new byte[s.Length];
            s.Read(buffer, 0, buffer.Length);
            s.Dispose();

            //Create texture from bytes
            var tex = new Texture2D(2, 2);

            tex.LoadImage(buffer, true);

            // Create sprite from texture
            // Split is to cut off the BossModCore.Resources. and the .png
            _dict.Add(tmpTextureKeys[i], Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
        }
    }

    public Sprite Get(string key)
    {
        return _dict[key];
    }
}