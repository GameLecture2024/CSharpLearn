using GameData;

// '구현 전에 허락을 맡아라'


// 게임이 되려면 무엇이 필요할까?
// 1. 입력
// 2. 렌더링 (Rendering)
// 3. Update


// 초기 상태 설정
MonsterType monsterType = MonsterType.FIRE;
int monsterHp = 100;
Tile currentTile = Tile.EARTH;
string statusMessage = "게임을 시작합니다. 이동할 타일을 선택하세요.";
bool isRunning = true;

while (isRunning)
{
    // ==========================================
    // 1. 렌더링 (Rendering) - 현재 화면 그리기
    // ==========================================
    Console.Clear();
    Console.WriteLine("==========================================");
    Console.WriteLine($" [몬스터 정보] 속성: {monsterType} | 체력: {monsterHp}");
    Console.WriteLine($" [현재 위치]   {currentTile} 타일 위");
    Console.WriteLine("------------------------------------------");
    Console.WriteLine($" 알림: {statusMessage}");
    Console.WriteLine("==========================================");
    Console.WriteLine("이동할 타일 선택: [1] 불  [2] 물  [3] 땅  [Q] 종료");
    Console.Write("입력 > ");

    // ==========================================
    // 2. 입력 (Input) - 사용자 키 입력 받기
    // ==========================================
    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

    // ==========================================
    // 3. Update - 입력에 따른 데이터 및 상태 갱신
    // ==========================================
    if (keyInfo.Key == ConsoleKey.Q)
    {
        isRunning = false;
        continue;
    }
    else if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1)
    {
        currentTile = Tile.FIRE;
    }
    else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2)
    {
        currentTile = Tile.WATER;
    }
    else if (keyInfo.Key == ConsoleKey.D3 || keyInfo.Key == ConsoleKey.NumPad3)
    {
        currentTile = Tile.EARTH;
    }
    else
    {
        statusMessage = "잘못된 입력입니다. 1, 2, 3 또는 Q를 누르세요.";
        continue;
    }

    // 몬스터와 타일 상성 계산 (Update)
    if (monsterType == MonsterType.FIRE)
    {
        switch (currentTile)
        {
            case Tile.FIRE:
                monsterHp += 10;
                statusMessage = "불 타일을 밟아 체력이 10 회복되었습니다!";
                break;

            case Tile.WATER:
                monsterHp -= 30;
                statusMessage = "물 타일을 밟아 체력이 30 감소했습니다!";
                break;

            case Tile.EARTH:
                statusMessage = "땅 타일을 밟았습니다. 아무런 변화가 없습니다.";
                break;
        }
    }

    // 체력 0 이하 사망 체크 (Update)
    if (monsterHp <= 0)
    {
        Console.Clear();
        Console.WriteLine("==========================================");
        Console.WriteLine($" [몬스터 정보] 속성: {monsterType} | 체력: 0");
        Console.WriteLine("------------------------------------------");
        Console.WriteLine(" 몬스터의 체력이 다해 쓰러졌습니다. GAME OVER!");
        Console.WriteLine("==========================================");
        isRunning = false;
    }
}
