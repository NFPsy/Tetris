using UnityEngine;

public static class SquareSprite
{
    private const int Size = 16;
    private const int BorderThickness = 2;

    private static Sprite cached;

    public static Sprite Get()
    {
        if (cached == null)
        {
            Texture2D texture = new Texture2D(Size, Size);
            texture.filterMode = FilterMode.Point;

            Color border = new Color(0f, 0f, 0f, 1f);
            Color fill = Color.white;

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    bool isBorder = x < BorderThickness || y < BorderThickness ||
                        x >= Size - BorderThickness || y >= Size - BorderThickness;
                    texture.SetPixel(x, y, isBorder ? border : fill);
                }
            }

            texture.Apply();
            cached = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }
        return cached;
    }
}
