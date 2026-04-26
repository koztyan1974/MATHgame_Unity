using UnityEngine;

/// <summary>
/// Provides runtime-generated sprites for minimal pixel-style visuals.
/// </summary>
public static class SpriteFactory
{
    private static Sprite cachedSquare;
    private static Sprite cachedPlayer;
    private static Sprite cachedNpc;

    public static Sprite GetSquareSprite()
    {
        if (cachedSquare != null)
        {
            return cachedSquare;
        }

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        cachedSquare = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return cachedSquare;
    }

    public static Sprite GetPlayerSprite()
    {
        if (cachedPlayer != null)
        {
            return cachedPlayer;
        }

        Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        FillTransparent(tex);

        Color skin = new Color32(255, 220, 177, 255);
        Color blue = new Color32(37, 144, 255, 255);
        Color darkBlue = new Color32(24, 100, 178, 255);
        Color black = new Color32(28, 32, 43, 255);
        Color white = new Color32(245, 247, 255, 255);

        // Head
        FillRect(tex, 5, 10, 6, 4, skin);
        Set(tex, 6, 11, black);
        Set(tex, 9, 11, black);

        // Body hoodie
        FillRect(tex, 4, 5, 8, 5, blue);
        FillRect(tex, 5, 4, 6, 1, darkBlue);
        Set(tex, 7, 8, white);
        Set(tex, 8, 8, white);

        // Arms
        FillRect(tex, 3, 6, 1, 3, blue);
        FillRect(tex, 12, 6, 1, 3, blue);

        // Legs
        FillRect(tex, 5, 1, 2, 4, black);
        FillRect(tex, 9, 1, 2, 4, black);

        tex.Apply();
        cachedPlayer = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        return cachedPlayer;
    }

    public static Sprite GetNpcSprite()
    {
        if (cachedNpc != null)
        {
            return cachedNpc;
        }

        Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        FillTransparent(tex);

        Color skin = new Color32(255, 218, 170, 255);
        Color orange = new Color32(247, 158, 46, 255);
        Color brown = new Color32(121, 74, 39, 255);
        Color dark = new Color32(35, 34, 41, 255);
        Color white = new Color32(242, 244, 248, 255);

        // Hair and head
        FillRect(tex, 5, 12, 6, 2, brown);
        FillRect(tex, 5, 9, 6, 4, skin);
        Set(tex, 6, 10, dark);
        Set(tex, 9, 10, dark);

        // Shirt
        FillRect(tex, 4, 5, 8, 4, orange);
        Set(tex, 7, 7, white);
        Set(tex, 8, 7, white);

        // Pants
        FillRect(tex, 5, 1, 2, 4, brown);
        FillRect(tex, 9, 1, 2, 4, brown);

        // Arms
        FillRect(tex, 3, 6, 1, 2, skin);
        FillRect(tex, 12, 6, 1, 2, skin);

        tex.Apply();
        cachedNpc = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        return cachedNpc;
    }

    private static void FillTransparent(Texture2D texture)
    {
        Color transparent = new Color(0f, 0f, 0f, 0f);
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, transparent);
            }
        }
    }

    private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int px = x; px < x + width; px++)
        {
            for (int py = y; py < y + height; py++)
            {
                Set(texture, px, py, color);
            }
        }
    }

    private static void Set(Texture2D texture, int x, int y, Color color)
    {
        if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
        {
            texture.SetPixel(x, y, color);
        }
    }
}
