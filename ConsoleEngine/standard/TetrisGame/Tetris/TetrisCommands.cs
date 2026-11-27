using ConsoleEngine;

namespace TetrisGame
{
    class MoveHorizontalCommand : ICommand
    {
        private readonly Tetris _game;
        private readonly int _dx;

        public MoveHorizontalCommand(Tetris game, int dx)
        {
            _game = game;
            _dx = dx;
        }

        public void Execute() => _game.MovePiece(_dx, 0);
    }

    class SoftDropCommand : ICommand
    {
        private readonly Tetris _game;
        public SoftDropCommand(Tetris game) => _game = game;
        public void Execute() => _game.MovePiece(0, 1);
    }

    class RotateCommand : ICommand
    {
        private readonly Tetris _game;
        public RotateCommand(Tetris game) => _game = game;
        public void Execute() => _game.RotatePiece();
    }

    class HardDropCommand : ICommand
    {
        private readonly Tetris _game;
        public HardDropCommand(Tetris game) => _game = game;
        public void Execute() => _game.HardDrop();
    }
}
