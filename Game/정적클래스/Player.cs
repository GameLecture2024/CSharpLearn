using System;

public class Player
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public Player(int startX, int startY)
    {
        X = startX;
        Y = startY;
    }

    // WASD 입력에 따른 이동 (맵 테두리 내부로 제한)
    public void Move(int dx, int dy, int mapWidth, int mapHeight)
    {
        int nextX = X + dx;
        int nextY = Y + dy;

        if (nextX > 0 && nextX < mapWidth - 1)
            X = nextX;

        if (nextY > 0 && nextY < mapHeight - 1)
            Y = nextY;
    }
}
