public class Bullet
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public Bullet(int x, int y)
    {
        X = x;
        Y = y;
    }

    // 스페이스바 발사 후 직선(위쪽 방향)으로 이동
    public void Update()
    {
        Y--;
    }
}
