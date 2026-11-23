/*
 *  클래스의 생성자까지 사용 문법 실습한다.
 *  
 *  -게임 폴더
 *      -Program.cs
 *      -Unit.cs  
 *      
 *  AI코드를 하나의 Main 작성을 하면 가독성이 매우 떨어집니다. AI가 무엇을 만들었는지 1도 알아볼 수가 없다.
 *  이해 가능한 범위까지 코드를 줄이면됩니다.
 *  1번째로. Main으로만 게임 만들어. -> 공통된 속성과 기능을 찾아서 클래스로 표현하고 그 클래스를 분할시켜라.
 */

class Program
{
    static void Main(string[] args)
    {
        // 1. 초기화 (Initialize)
        Unit player = new Unit("용사", 100, 20, 5, 15);
        Unit monster = new Unit("슬라임", 80, 15, 2, 10);

        bool isRunning = true;
        int turn = 1;

        Console.WriteLine("=== 전투 시작! ===");
        Render(player, monster, 0);

        // 2. 메인 게임 루프 (while)
        while (isRunning)
        {
            // [1단계: 입력 (Input)]
            Console.WriteLine("\n다음 턴을 진행하려면 [Enter]를 누르세요. (종료: Q)");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.Q)
            {
                Console.WriteLine("전투를 포기하고 종료합니다.");
                break;
            }
            if (keyInfo.Key != ConsoleKey.Enter)
            {
                continue; // Enter나 Q가 아니면 다시 입력 대기
            }

            Console.Clear(); // 화면 정리 (선택 사항)
            Console.WriteLine($"--- {turn} 턴 결과 ---");

            // [2단계: 업데이트 (Update)]
            player.BattleTurn(monster);

            // 게임 종료 조건 확인
            if (!player.IsAlive || !monster.IsAlive)
            {
                isRunning = false;
            }

            // [3단계: 렌더링 (Render)]
            Render(player, monster, turn);
            turn++;
        }

        // 3. 종료 처리
        Console.WriteLine("\n=== 전투 종료 ===");
        if (player.IsAlive && !monster.IsAlive)
            Console.WriteLine($"{player.Name} 승리!");
        else if (!player.IsAlive)
            Console.WriteLine($"{monster.Name} 승리...");
    }

    // 현재 상태를 화면에 그리는 함수
    static void Render(Unit p, Unit m, int turn)
    {
        Console.WriteLine("=========================================");
        Console.WriteLine($" [{p.Name}] HP: {p.Health} | ATK: {p.Attack} | DEF: {p.Defense}");
        Console.WriteLine($" [{m.Name}] HP: {m.Health} | ATK: {m.Attack} | DEF: {m.Defense}");
        Console.WriteLine("=========================================");
    }
}