using System;
using System.Collections.Generic;
using System.Threading;

// [게임 루프 실행 설정]
Console.CursorVisible = false; // static CursorVisible -커서 안보이게한다.
int mapWidth = 40;
int mapHeight = 20;

Player player = new Player(mapWidth / 2, mapHeight - 2);
List<Bullet> bullets = new List<Bullet>();
List<Monster> monsters = new List<Monster>();

// 첫 번째 웨이브 몬스터 소환
WaveManager.SpawnNextWave(monsters, mapWidth);

bool isRunning = true;

// [최상위문 게임 루프: 입력 -> 업데이트 -> 렌더링]
while (isRunning)
{
    ProcessInput(player, bullets, mapWidth, mapHeight, ref isRunning);
    UpdateGame(bullets, monsters, mapWidth);
    RenderScreen(player, bullets, monsters, mapWidth, mapHeight);

    Thread.Sleep(50); // 약 20 FPS 유지
}

Console.Clear();
Console.CursorVisible = true;
Console.WriteLine("게임이 종료되었습니다.");

// ==========================================
// 1. 입력 (Input)
// ==========================================
static void ProcessInput(Player player, List<Bullet> bullets, int width, int height, ref bool isRunning)
{
    while (Console.KeyAvailable)
    {
        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        switch (keyInfo.Key)
        {
            case ConsoleKey.W: player.Move(0, -1, width, height); break;
            case ConsoleKey.S: player.Move(0, 1, width, height); break;
            case ConsoleKey.A: player.Move(-1, 0, width, height); break;
            case ConsoleKey.D: player.Move(1, 0, width, height); break;
            case ConsoleKey.Spacebar:
                bullets.Add(new Bullet(player.X, player.Y - 1));
                break;
            case ConsoleKey.Escape:
                isRunning = false;
                break;
        }
    }
}

// ==========================================
// 2. 업데이트 (Update)
// ==========================================
static void UpdateGame(List<Bullet> bullets, List<Monster> monsters, int mapWidth)
{
    // 총알 이동 및 충돌 판정
    for (int i = bullets.Count - 1; i >= 0; i--)
    {
        bullets[i].Update();

        // 화면 밖으로 나간 총알 제거
        if (bullets[i].Y < 1)
        {
            bullets.RemoveAt(i);
            continue;
        }

        // 몬스터와 충돌 체크
        for (int j = monsters.Count - 1; j >= 0; j--)
        {
            if (monsters[j].IsAlive && bullets[i].X == monsters[j].X && bullets[i].Y == monsters[j].Y)
            {
                monsters[j].Die();
                WaveManager.OnMonsterDied();
                bullets.RemoveAt(i);
                break;
            }
        }
    }

    // 모든 몬스터 사망 시 다음 웨이브 발생
    if (WaveManager.AliveCount <= 0)
    {
        monsters.Clear();
        WaveManager.SpawnNextWave(monsters, mapWidth);
    }
}

// ==========================================
// 3. 렌더링 (Render)
// ==========================================
static void RenderScreen(Player player, List<Bullet> bullets, List<Monster> monsters, int width, int height)
{
    Console.SetCursorPosition(0, 0);
    Console.WriteLine($"[WAVE: {WaveManager.CurrentWave}] 남은 몬스터: {WaveManager.AliveCount}마리   (이동: WASD / 발사: Space / 종료: ESC)");

    char[,] board = new char[height, width];

    // 빈 공간 및 테두리 초기화
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            if (y == 0 || y == height - 1 || x == 0 || x == width - 1)
                board[y, x] = '#';
            else
                board[y, x] = ' ';
        }
    }

    // 몬스터 배치
    foreach (var m in monsters)
    {
        if (m.IsAlive)
            board[m.Y, m.X] = 'M';
    }

    // 총알 배치
    foreach (var b in bullets)
    {
        if (b.Y > 0 && b.Y < height - 1)
            board[b.Y, b.X] = '|';
    }

    // 플레이어 배치
    board[player.Y, player.X] = 'A';

    // 화면 출력
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            Console.Write(board[y, x]);
        }
        Console.WriteLine();
    }
}
