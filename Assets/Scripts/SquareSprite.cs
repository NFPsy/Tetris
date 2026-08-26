using UnityEngine;

// 블록 한 칸을 표현하는 네모 스프라이트를 코드로 직접 만들어주는 스크립트입니다.
// 이미지 파일(png 등)을 따로 준비하지 않고, 16x16 픽셀 텍스처를 실행 중에 그려서
// 테두리는 검은색, 안쪽은 흰색으로 채웁니다.
// 이 흰색 부분에 SpriteRenderer/Image의 color를 곱하면(틴트) 원하는 색 블록이 되고,
// 테두리는 항상 검게 남아서 옆에 붙은 블록끼리도 칸 구분이 잘 보입니다.
public static class SquareSprite
{
    private const int Size = 16;           // 텍스처 한 변의 픽셀 크기
    private const int BorderThickness = 2; // 테두리 두께(픽셀)

    // 한 번 만든 스프라이트를 재사용하기 위한 캐시.
    // 모든 블록(낙하 중인 블록, 바닥에 쌓인 블록, 미리보기 블록)이 이 하나의 스프라이트를 공유합니다.
    private static Sprite cached;

    public static Sprite Get()
    {
        if (cached == null)
        {
            Texture2D texture = new Texture2D(Size, Size);
            texture.filterMode = FilterMode.Point; // 확대해도 흐려지지 않고 각지게 보이도록 설정

            Color border = new Color(0f, 0f, 0f, 1f);
            Color fill = Color.white;

            // 픽셀 하나하나를 돌면서, 가장자리 영역이면 검정, 아니면 흰색으로 칠합니다.
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    bool isBorder = x < BorderThickness || y < BorderThickness ||
                        x >= Size - BorderThickness || y >= Size - BorderThickness;
                    texture.SetPixel(x, y, isBorder ? border : fill);
                }
            }

            texture.Apply(); // 위에서 설정한 픽셀들을 실제로 텍스처에 반영

            // pivot(0.5, 0.5): 스프라이트의 중심이 기준점이 되도록 설정
            // pixelsPerUnit = Size: 텍스처 16픽셀이 월드 공간에서 정확히 1칸(1 unit) 크기가 되도록 설정
            cached = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }
        return cached;
    }
}
