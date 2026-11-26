using System;
using System.Text;
using System.Threading;

namespace ConsoleTetris
{

    // 3. MyGameEngine을 수정하지 않고 별도로 상속받아 구현한 테트리스 게임
    public class TetrisGame : MyGameEngine
    {
        private const int BoardWidth = 12;  // 내부 10칸 + 좌우 벽 2칸
        private const int BoardHeight = 22; // 내부 20칸 + 상하 벽 2칸

        private int[,] board = new int[BoardHeight, BoardWidth]; // 0: 빈칸, 1: 벽, 2: 고정된 블록
        private Random rand = new Random();

        // 7가지 테트로미노 블록 정의 [7종, 4행, 4열]
        private readonly int[,,] shapes = new int[7, 4, 4]
        {
            { {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} }, // I
            { {0,0,0,0}, {0,1,1,0}, {0,1,1,0}, {0,0,0,0} }, // O
            { {0,0,0,0}, {0,1,0,0}, {1,1,1,0}, {0,0,0,0} }, // T
            { {0,0,0,0}, {0,1,1,0}, {1,1,0,0}, {0,0,0,0} }, // S
            { {0,0,0,0}, {1,1,0,0}, {0,1,1,0}, {0,0,0,0} }, // Z
            { {0,0,0,0}, {1,0,0,0}, {1,1,1,0}, {0,0,0,0} }, // J
            { {0,0,0,0}, {0,0,1,0}, {1,1,1,0}, {0,0,0,0} }  // L
        };

        private int[,] currentShape = new int[4, 4];
        private int curX;
        private int curY;
        private int dropCounter = 0;
        private int dropInterval = 15;
        private int score = 0;
        private bool isGameOver = false;

        public TetrisGame() : base(BoardWidth, BoardHeight)
        {
            InitGame();
        }

        private void InitGame()
        {
            score = 0;
            isGameOver = false;

            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (y == 0 || y == BoardHeight - 1 || x == 0 || x == BoardWidth - 1)
                        board[y, x] = 1;
                    else
                        board[y, x] = 0;
                }
            }

            SpawnNewBlock();
        }

        private void SpawnNewBlock()
        {
            int type = rand.Next(0, 7);
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    currentShape[y, x] = shapes[type, y, x];
                }
            }

            curX = BoardWidth / 2 - 2;
            curY = 1;

            if (CheckCollision(curX, curY, currentShape))
            {
                isGameOver = true;
            }
        }

        public override void Update()
        {
            base.Update();

            if (isGameOver)
            {
                if (CurrentKey != null && CurrentKey.Value.Key == ConsoleKey.R)
                {
                    InitGame();
                }
                return;
            }

            if (CurrentKey != null)
            {
                switch (CurrentKey.Value.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (!CheckCollision(curX - 1, curY, currentShape))
                            curX--;
                        break;
                    case ConsoleKey.RightArrow:
                        if (!CheckCollision(curX + 1, curY, currentShape))
                            curX++;
                        break;
                    case ConsoleKey.DownArrow:
                        if (!CheckCollision(curX, curY + 1, currentShape))
                            curY++;
                        break;
                    case ConsoleKey.UpArrow:
                        RotateBlock();
                        break;
                    case ConsoleKey.Spacebar:
                        while (!CheckCollision(curX, curY + 1, currentShape))
                        {
                            curY++;
                        }
                        LockAndClear();
                        break;
                }
            }

            dropCounter++;
            if (dropCounter >= dropInterval)
            {
                dropCounter = 0;
                if (!CheckCollision(curX, curY + 1, currentShape))
                {
                    curY++;
                }
                else
                {
                    LockAndClear();
                }
            }
        }

        public override void Rendering()
        {
            ClearBuffer();

            if (isGameOver)
            {
                SetStatusText($"SCORE: {score} | GAME OVER! (R키를 눌러 재시작)   ");
            }
            else
            {
                SetStatusText($"SCORE: {score} | ←→:이동  ↑:회전  Space:드롭      ");
            }

            // 고정된 보드 그리기
            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (board[y, x] == 1)
                        SetPixel(x, y, "■");
                    else if (board[y, x] == 2)
                        SetPixel(x, y, "□");
                }
            }

            // 현재 떨어지는 블록 그리기
            if (!isGameOver)
            {
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        if (currentShape[y, x] == 1)
                        {
                            SetPixel(curX + x, curY + y, "▣");
                        }
                    }
                }
            }

            base.Rendering();
        }

        private void LockAndClear()
        {
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (currentShape[y, x] == 1)
                    {
                        board[curY + y, curX + x] = 2;
                    }
                }
            }

            ClearLines();
            SpawnNewBlock();
        }

        private void ClearLines()
        {
            for (int y = BoardHeight - 2; y >= 1; y--)
            {
                bool isFull = true;
                for (int x = 1; x < BoardWidth - 1; x++)
                {
                    if (board[y, x] == 0)
                    {
                        isFull = false;
                        break;
                    }
                }

                if (isFull)
                {
                    score += 100;
                    for (int moveY = y; moveY > 1; moveY--)
                    {
                        for (int x = 1; x < BoardWidth - 1; x++)
                        {
                            board[moveY, x] = board[moveY - 1, x];
                        }
                    }

                    for (int x = 1; x < BoardWidth - 1; x++)
                    {
                        board[1, x] = 0;
                    }

                    y++;
                }
            }
        }

        private void RotateBlock()
        {
            int[,] rotated = new int[4, 4];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    rotated[x, 3 - y] = currentShape[y, x];
                }
            }

            if (!CheckCollision(curX, curY, rotated))
            {
                currentShape = rotated;
            }
        }

        private bool CheckCollision(int nextX, int nextY, int[,] shape)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (shape[y, x] == 1)
                    {
                        int targetX = nextX + x;
                        int targetY = nextY + y;

                        if (targetX < 0 || targetX >= BoardWidth || targetY < 0 || targetY >= BoardHeight)
                            return true;

                        if (board[targetY, targetX] != 0)
                            return true;
                    }
                }
            }
            return false;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            BaseGameEngine game = new TetrisGame();

            while (true)
            {
                game.Input();
                game.Update();
                game.Rendering();

                Thread.Sleep(25);
            }
        }
    }
}
