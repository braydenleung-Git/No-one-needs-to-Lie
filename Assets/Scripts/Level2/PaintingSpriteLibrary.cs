using System.IO;
using UnityEngine;

// Loads the full-screen painting sprites by letter (J/O/H/N).
// Works best if you put the PNGs under Assets/Resources/L1/ (so Resources.Load can find them).
// In the editor, also tries to load them from Assets/Sprites/L1/ via direct file read.
// basically this is just a helper so we're not copy-pasting the same load logic in every painting script
public static class PaintingSpriteLibrary
{
    public static Sprite LoadLetter(char letter)
    {
        // uppercase the letter so passing 'j' or 'J' both work, avoids bugs from typos
        string L = char.ToUpperInvariant(letter).ToString();

        // Preferred: Resources/L1/J.png etc.
        // Use Texture2D so it works even if the importer slices as Multiple (J_0) or renames sub-sprites.
        // tried loading as Sprite directly first but the importer naming was inconsistent, Texture2D is safer
        var texRes = Resources.Load<Texture2D>($"L1/{L}");
        if (texRes != null)
        {
            // wrap the texture in a sprite manually, pivot center, 100 PPU to match everything else in the project
            return Sprite.Create(
                texRes,
                new Rect(0, 0, texRes.width, texRes.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        // Alternate: try Sprite load (works if the main sprite is named exactly "J")
        // some people might have imported them as single sprites, so try that too before giving up
        var sprRes = Resources.Load<Sprite>($"L1/{L}");
        if (sprRes != null) return sprRes;

        // Fallback: read from Assets/Sprites/L1/J.png at runtime/editor.
        // Note: for builds, this only works if the file exists on disk next to the game.
        // this is mainly useful during dev when you haven't moved things to Resources yet
        string path = Path.Combine(Application.dataPath, "Sprites", "L1", $"{L}.png");
        if (!File.Exists(path)) return null; // file just isn't there, return null and let the caller handle it

        try
        {
            // read the raw bytes and shove them into a Texture2D - works for any standard PNG
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            if (!tex.LoadImage(bytes)) return null; // LoadImage returns false if the bytes aren't a valid image
            tex.filterMode = FilterMode.Point; // pixel art style, no blurring
            tex.wrapMode = TextureWrapMode.Clamp; // clamp so edges don't tile weirdly
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
        catch
        {
            // something went wrong reading or decoding the file, just return null instead of crashing
            return null;
        }
    }
}
