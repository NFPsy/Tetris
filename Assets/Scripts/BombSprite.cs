using UnityEngine;

// 폭탄 칸에 표시할 스프라이트를 불러오는 스크립트입니다.
// Assets/Resources/Bomb.png를 불러와서 캐시해두고 재사용합니다.
public static class BombSprite
{
    private static Sprite cached;

    public static Sprite Get()
    {
        if (cached == null)
        {
            cached = Resources.Load<Sprite>("Bomb");
        }
        return cached;
    }
}
