using UnityEngine;

public static class SquareSprite
{
    private static Sprite cached;

    public static Sprite Get()
    {
        if (cached == null)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            cached = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
        return cached;
    }
}
