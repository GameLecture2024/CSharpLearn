using ConsoleEngine;

namespace TetrisGame
{
    enum GameState { Playing, GameOver }

    class Tetris : GameBase
    {
        private const int BoardWidth = 10;
        private const int BoardHeight = 20;
        private const int ScreenWidth = 28;
        private const int ScreenHeight = 22;
        private const float DropInterval = 0.5f;

        private readonly int[,] _board = new int[BoardHeight, BoardWidth];
        private int[,] _currentShape = new int[0, 0];
        private int _pieceX;
        private int _pieceY;

        private float _dropTimer;
        private int _score;
        private GameState _state = GameState.Playing;
        private bool _useFForDrop = false;

        private static readonly int[][,] Tetrominoes =
        {
            new int[,] { {1, 1, 1, 1} },
            new int[,] { {1, 1}, {1, 1} },
            new int[,] { {0, 1, 0}, {1, 1, 1} },
            new int[,] { {1, 0, 0}, {1, 1, 1} },
            new int[,] { {0, 0, 1}, {1, 1, 1} },
            new int[,] { {0, 1, 1}, {1, 1, 0} },
            new int[,] { {1, 1, 0}, {0, 1, 1} }
        };

        public Tetris() : base(ScreenWidth, ScreenHeight)
        {
            Keys.Bind(ConsoleKey.LeftArrow, new MoveHorizontalCommand(this, -1));
            Keys.Bind(ConsoleKey.RightArrow, new MoveHorizontalCommand(this, 1));
            Keys.Bind(ConsoleKey.DownArrow, new SoftDropCommand(this));
            Keys.Bind(ConsoleKey.UpArrow, new RotateCommand(this));
            Keys.Bind(ConsoleKey.Spacebar, new HardDropCommand(this));

            SpawnPiece();
        }

        protected override void Input(ConsoleKey key)
        {
            if (key == ConsoleKey.Escape) Quit();
            if (key == ConsoleKey.K) SwapDropKey();
        }

        private void SwapDropKey()
        {
            if (_useFForDrop)
            {
                Keys.Rebind(ConsoleKey.F, ConsoleKey.Spacebar);
            }
            else
            {
                Keys.Rebind(ConsoleKey.Spacebar, ConsoleKey.F);
            }
            _useFForDrop = !_useFForDrop;
        }

        public void MovePiece(int dx, int dy)
        {
            if (_state != GameState.Playing) return;

            if (IsValidPosition(_currentShape, _pieceX + dx, _pieceY + dy))
            {
                _pieceX += dx;
                _pieceY += dy;
            }
            else if (dy > 0)
            {
                LockPiece();
            }
        }

        public void RotatePiece()
        {
            if (_state != GameState.Playing) return;

            int rows = _currentShape.GetLength(0);
            int cols = _currentShape.GetLength(1);
            int[,] rotated = new int[cols, rows];

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    rotated[c, rows - 1 - r] = _currentShape[r, c];

            if (IsValidPosition(rotated, _pieceX, _pieceY))
            {
                _currentShape = rotated;
            }
        }

        public void HardDrop()
        {
            if (_state != GameState.Playing) return;

            while (IsValidPosition(_currentShape, _pieceX, _pieceY + 1))
            {
                _pieceY++;
            }
            LockPiece();
        }

        private bool IsValidPosition(int[,] shape, int testX, int testY)
        {
            int rows = shape.GetLength(0);
            int cols = shape.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (shape[r, c] == 0) continue;

                    int boardX = testX + c;
                    int boardY = testY + r;

                    if (boardX < 0 || boardX >= BoardWidth || boardY >= BoardHeight)
                        return false;

                    if (boardY >= 0 && _board[boardY, boardX] != 0)
                        return false;
                }
            }
            return true;
        }

        private void LockPiece()
        {
            int rows = _currentShape.GetLength(0);
            int cols = _currentShape.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (_currentShape[r, c] != 0)
                    {
                        int bx = _pieceX + c;
                        int by = _pieceY + r;
                        if (by >= 0 && by < BoardHeight && bx >= 0 && bx < BoardWidth)
                            _board[by, bx] = 1;
                    }
                }
            }

            ClearLines();
            SpawnPiece();
        }

        private void ClearLines()
        {
            for (int y = BoardHeight - 1; y >= 0; y--)
            {
                bool full = true;
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (_board[y, x] == 0)
                    {
                        full = false;
                        break;
                    }
                }

                if (full)
                {
                    _score += 100;
                    for (int pullY = y; pullY > 0; pullY--)
                        for (int x = 0; x < BoardWidth; x++)
                            _board[pullY, x] = _board[pullY - 1, x];

                    for (int x = 0; x < BoardWidth; x++)
                        _board[0, x] = 0;

                    y++;
                }
            }
        }

        private void SpawnPiece()
        {
            _currentShape = Tetrominoes[Random.Shared.Next(Tetrominoes.Length)];
            _pieceX = (BoardWidth - _currentShape.GetLength(1)) / 2;
            _pieceY = 0;
            _dropTimer = DropInterval;

            if (!IsValidPosition(_currentShape, _pieceX, _pieceY))
            {
                _state = GameState.GameOver;
            }
        }

        private void Restart()
        {
            Array.Clear(_board, 0, _board.Length);
            _score = 0;
            _state = GameState.Playing;
            SpawnPiece();
        }

        protected override void Update(float deltaTime)
        {
            if (_state != GameState.Playing)
            {
                if (Keyboard.WasPressed(ConsoleKey.R)) Restart();
                return;
            }

            _dropTimer -= deltaTime;
            if (_dropTimer <= 0f)
            {
                MovePiece(0, 1);
                _dropTimer = DropInterval;
            }
        }

        protected override void Render(Screen screen)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                screen.Draw(0, y, '|');
                screen.Draw(BoardWidth + 1, y, '|');
            }
            for (int x = 0; x <= BoardWidth + 1; x++)
            {
                screen.Draw(x, BoardHeight, '-');
            }

            for (int y = 0; y < BoardHeight; y++)
                for (int x = 0; x < BoardWidth; x++)
                    if (_board[y, x] != 0)
                        screen.Draw(x + 1, y, '#');

            if (_state == GameState.Playing)
            {
                int rows = _currentShape.GetLength(0);
                int cols = _currentShape.GetLength(1);
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                        if (_currentShape[r, c] != 0)
                            screen.Draw(_pieceX + c + 1, _pieceY + r, '@');
            }

            int uiX = BoardWidth + 3;
            screen.DrawText(uiX, 1, $"SCORE: {_score}");
            screen.DrawText(uiX, 3, "MOVE: <-/->");
            screen.DrawText(uiX, 4, "ROT : UP");
            screen.DrawText(uiX, 5, "SOFT: DOWN");
            screen.DrawText(uiX, 7, _useFForDrop ? "DROP: F (K)" : "DROP: SPACE (K)");

            if (_state == GameState.GameOver)
            {
                screen.DrawText(uiX, 10, "GAME OVER!");
                screen.DrawText(uiX, 11, "R: RETRY");
            }
        }
    }
}
