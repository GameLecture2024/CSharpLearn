using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleTetris
{
    // 요구사항: 모든 기능을 하나의 메인 클래스(Program) 안에 구현 (내부 중첩 클래스 활용)
    class Program
    {
        // 기본 블록 클래스
        class 블록
        {
            public string shape = "■";

            public 블록(string shape)
            {
                this.shape = shape;
            }
        }

        // 블록을 상속받아 시간 제한 후 파괴되는 기능을 추가한 클래스
        class TimerBlock : 블록
        {
            public int deathTimer = 3; // 기본 3초
            private DateTime spawnTime;
            public bool IsDestroyed { get; private set; } = false;

            public TimerBlock(string shape, int timerSeconds = 3) : base(shape)
            {
                this.deathTimer = timerSeconds;
                this.spawnTime = DateTime.Now;
            }

            // 바닥에 고정된 시점부터 타이머 시작
            public void StartTimer()
            {
                this.spawnTime = DateTime.Now;
            }

            // 시간이 지났을 때 없어지는 기능 (영어로 변환: TimeDeath / CheckTimeDeath)
            public bool TimeDeath()
            {
                if (IsDestroyed) return true;

                double elapsed = (DateTime.Now - spawnTime).TotalSeconds;
                if (elapsed >= deathTimer)
                {
                    IsDestroyed = true;
                    return true;
                }
                return false;
            }

            // 남은 시간 계산 (UI 표시용)
            public int GetRemainingSeconds()
            {
                int rem = deathTimer - (int)(DateTime.Now - spawnTime).TotalSeconds;
                return rem > 0 ? rem : 0;
            }
        }

        // 게임 보드 크기 설정
        const int Width = 10;
        const int Height = 20;

        // 보드 배열 (null이면 빈 공간, 블록 객체가 있으면 채워진 공간)
        static 블록[,] board = new 블록[Height, Width];

        // 테트로미노 모양 정의 (7가지 기본 블록)
        static readonly int[][,] Shapes = new int[][,]
        {
            new int[,] { {1, 1, 1, 1} },                         // I
            new int[,] { {1, 1}, {1, 1} },                       // O
            new int[,] { {0, 1, 0}, {1, 1, 1} },                 // T
            new int[,] { {1, 0, 0}, {1, 1, 1} },                 // J
            new int[,] { {0, 0, 1}, {1, 1, 1} },                 // L
            new int[,] { {0, 1, 1}, {1, 1, 0} },                 // S
            new int[,] { {1, 1, 0}, {0, 1, 1} }                  // Z
        };

        // 현재 조작 중인 조각 상태
        static int[,] currentShape;
        static int currentX;
        static int currentY;
        static bool isCurrentTimerBlock;
        static int score = 0;
        static bool isGameOver = false;

        static Random rand = new Random();
        static DateTime lastFallTime = DateTime.Now;
        static int fallIntervalMs = 500; // 블록 자동 하강 주기 (0.5초)

        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            SpawnPiece();

            // 메인 게임 루프 (입력 -> 갱신 -> 렌더링)
            while (!isGameOver)
            {
                Input();
                Update();
                Render();

                Thread.Sleep(30); // CPU 점유율 조절 및 부드러운 프레임 유지
            }

            // 게임 오버 화면 출력
            Console.Clear();
            Console.WriteLine("================================");
            Console.WriteLine("         GAME OVER!             ");
            Console.WriteLine($"       최종 점수: {score}점      ");
            Console.WriteLine("================================");
            Console.WriteLine("아무 키나 누르면 종료합니다...");
            Console.ReadKey();
        }

        // 1. 입력 (Input)
        static void Input()
        {
            if (!Console.KeyAvailable) return;

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            switch (keyInfo.Key)
            {
                case ConsoleKey.LeftArrow:
                    if (CanMove(currentShape, currentX - 1, currentY))
                        currentX--;
                    break;

                case ConsoleKey.RightArrow:
                    if (CanMove(currentShape, currentX + 1, currentY))
                        currentX++;
                    break;

                case ConsoleKey.DownArrow:
                    if (CanMove(currentShape, currentX, currentY + 1))
                    {
                        currentY++;
                        lastFallTime = DateTime.Now;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    int[,] rotated = RotateMatrix(currentShape);
                    if (CanMove(rotated, currentX, currentY))
                        currentShape = rotated;
                    break;

                case ConsoleKey.Spacebar:
                    // 하드 드롭 (바닥까지 즉시 내리기)
                    while (CanMove(currentShape, currentX, currentY + 1))
                    {
                        currentY++;
                    }
                    LockPiece();
                    SpawnPiece();
                    break;
            }
        }

        // 2. 갱신 (Update)
        static void Update()
        {
            // (1) 현재 블록 자동 하강 처리
            if ((DateTime.Now - lastFallTime).TotalMilliseconds >= fallIntervalMs)
            {
                if (CanMove(currentShape, currentX, currentY + 1))
                {
                    currentY++;
                }
                else
                {
                    LockPiece();
                    SpawnPiece();
                }
                lastFallTime = DateTime.Now;
            }

            // (2) 보드에 고정된 TimerBlock 시간 체크 및 파괴 처리
            bool anyTimerBlockDestroyed = false;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (board[y, x] is TimerBlock tb)
                    {
                        // 시간이 다 되었는지 확인
                        if (tb.TimeDeath())
                        {
                            board[y, x] = null; // 블록 파괴
                            anyTimerBlockDestroyed = true;
                        }
                    }
                }
            }

            // (3) 파괴된 TimerBlock이 있다면 위에 있는 블록들을 자동으로 아래로 내림 (중력 적용)
            if (anyTimerBlockDestroyed)
            {
                ApplyGravity();
                ClearFullLines(); // 내려온 결과로 줄이 완성되었을 수도 있으므로 체크
            }
        }

        // 3. 렌더링 (Render)
        static void Render()
        {
            Console.SetCursorPosition(0, 0);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== 콘솔 테트리스 (TimerBlock 에디션) ===");
            sb.AppendLine($"점수: {score}   |   [←/→] 이동  [↑] 회전  [↓] 내리기  [Space] 즉시낙하");
            sb.AppendLine("※ ◈ 모양(숫자)은 설치 후 제한시간이 지나면 터지며 위 블록이 내려옵니다!");
            sb.AppendLine("┌" + new string('─', Width * 2) + "┐");

            for (int y = 0; y < Height; y++)
            {
                sb.Append("│");
                for (int x = 0; x < Width; x++)
                {
                    // 현재 조작 중인 블록 위치인지 확인
                    bool isCurrentPieceCell = false;
                    int py = y - currentY;
                    int px = x - currentX;

                    if (py >= 0 && py < currentShape.GetLength(0) &&
                        px >= 0 && px < currentShape.GetLength(1) &&
                        currentShape[py, px] == 1)
                    {
                        isCurrentPieceCell = true;
                    }

                    if (isCurrentPieceCell)
                    {
                        sb.Append(isCurrentTimerBlock ? "◈ " : "■ ");
                    }
                    else if (board[y, x] != null)
                    {
                        if (board[y, x] is TimerBlock tb)
                        {
                            // TimerBlock은 남은 초를 숫자로 표시해 직관성 향상
                            sb.Append($"{tb.GetRemainingSeconds()} ");
                        }
                        else
                        {
                            sb.Append(board[y, x].shape + " ");
                        }
                    }
                    else
                    {
                        sb.Append("  ");
                    }
                }
                sb.AppendLine("│");
            }

            sb.AppendLine("└" + new string('─', Width * 2) + "┘");
            Console.Write(sb.ToString());
        }

        // 새로운 블록 생성
        static void SpawnPiece()
        {
            int idx = rand.Next(Shapes.Length);
            currentShape = Shapes[idx];
            currentX = Width / 2 - currentShape.GetLength(1) / 2;
            currentY = 0;

            // 30% 확률로 TimerBlock 조각 생성
            isCurrentTimerBlock = rand.Next(100) < 30;

            if (!CanMove(currentShape, currentX, currentY))
            {
                isGameOver = true;
            }
        }

        // 이동 및 충돌 검사
        static bool CanMove(int[,] shape, int nextX, int nextY)
        {
            int rows = shape.GetLength(0);
            int cols = shape.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (shape[r, c] == 0) continue;

                    int boardX = nextX + c;
                    int boardY = nextY + r;

                    // 벽 충돌 검사
                    if (boardX < 0 || boardX >= Width || boardY < 0 || boardY >= Height)
                        return false;

                    // 기존 블록 충돌 검사
                    if (board[boardY, boardX] != null)
                        return false;
                }
            }
            return true;
        }

        // 현재 조각을 보드에 고정
        static void LockPiece()
        {
            int rows = currentShape.GetLength(0);
            int cols = currentShape.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (currentShape[r, c] == 1)
                    {
                        int bx = currentX + c;
                        int by = currentY + r;

                        if (by >= 0 && by < Height && bx >= 0 && bx < Width)
                        {
                            if (isCurrentTimerBlock)
                            {
                                // 5초 뒤 사라지는 TimerBlock 생성 및 타이머 시작
                                TimerBlock tb = new TimerBlock("◈", 5);
                                tb.StartTimer();
                                board[by, bx] = tb;
                            }
                            else
                            {
                                board[by, bx] = new 블록("■");
                            }
                        }
                    }
                }
            }

            ClearFullLines();
        }

        // TimerBlock 파괴 시 빈 공간 위의 블록들을 아래로 내리는 기능
        static void ApplyGravity()
        {
            for (int x = 0; x < Width; x++)
            {
                int writeRow = Height - 1;
                for (int y = Height - 1; y >= 0; y--)
                {
                    if (board[y, x] != null)
                    {
                        블록 temp = board[y, x];
                        board[y, x] = null;
                        board[writeRow, x] = temp;
                        writeRow--;
                    }
                }
            }
        }

        // 꽉 찬 줄 제거 및 점수 획득
        static void ClearFullLines()
        {
            int linesCleared = 0;

            for (int y = Height - 1; y >= 0; y--)
            {
                bool isFull = true;
                for (int x = 0; x < Width; x++)
                {
                    if (board[y, x] == null)
                    {
                        isFull = false;
                        break;
                    }
                }

                if (isFull)
                {
                    linesCleared++;
                    for (int moveY = y; moveY > 0; moveY--)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            board[moveY, x] = board[moveY - 1, x];
                        }
                    }
                    for (int x = 0; x < Width; x++)
                    {
                        board[0, x] = null;
                    }
                    y++; // 내려온 줄 다시 검사
                }
            }

            if (linesCleared > 0)
            {
                score += linesCleared * 100;
            }
        }

        // 2차원 배열 시계 방향 90도 회전
        static int[,] RotateMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int[,] rotated = new int[cols, rows];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    rotated[c, rows - 1 - r] = matrix[r, c];
                }
            }
            return rotated;
        }
    }
}
