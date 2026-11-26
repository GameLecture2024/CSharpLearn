using System;
using System.Text;
using System.Threading;

// 다형성을 활용해 BaseGameEngine 타입으로 게임 실행
BaseGameEngine game = new TetrisGame();

while (true)
{
    game.Input();
    game.Update();
    game.Rendering();

    Thread.Sleep(25);
}

// 1. 최상위 추상 엔진
abstract class BaseGameEngine
{
    abstract public void Input();
    abstract public void Update();
    abstract public void Rendering();
}

// 2. 핵심 렌더링 및 입력 처리를 담당하는 게임 엔진 (수정 없이 재사용)
class MyGameEngine : BaseGameEngine
{
    protected int Width { get; private set; }
    protected int Height { get; private set; }

    private string[,] screenBuffer;
    private StringBuilder renderBuilder = new StringBuilder();
    private string statusText = "";

    // 하위 클래스에서 읽을 수 있는 현재 입력 키
    protected ConsoleKeyInfo? CurrentKey { get; private set; }

    public MyGameEngine(int width = 20, int height = 20)
    {
        Width = width;
        Height = height;
        screenBuffer = new string[Height, Width];
        Console.CursorVisible = false;
        ClearBuffer();
    }

    // 버퍼 초기화 (2칸 공백으로 채워 밀림 방지)
    protected void ClearBuffer()
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                screenBuffer[y, x] = "  ";
    }

    // 하위 게임 클래스가 화면에 2칸짜리 타일을 그릴 때 사용하는 엔진 API
    protected void SetPixel(int x, int y, string twoWidthChar)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            screenBuffer[y, x] = twoWidthChar;
        }
    }

    // 상단 UI 텍스트 설정 API
    protected void SetStatusText(string text)
    {
        statusText = text;
    }

    public override void Input()
    {
        if (Console.KeyAvailable)
            CurrentKey = Console.ReadKey(true);
        else
            CurrentKey = null;
    }

    public override void Update()
    {
        // 하위 클래스에서 오버라이드하여 게임 로직 구현
    }

    public override void Rendering()
    {
        // 깜빡임 해결: 커서를 (0, 0)으로 이동해 덮어쓰기
        Console.SetCursorPosition(0, 0);
        renderBuilder.Clear();

        if (!string.IsNullOrEmpty(statusText))
        {
            renderBuilder.AppendLine(statusText);
        }

        // 2칸 밀림 해결: 버퍼에 담긴 2칸 단위 문자열을 한 번에 조립
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                renderBuilder.Append(screenBuffer[y, x]);
            }
            renderBuilder.AppendLine();
        }

        Console.Write(renderBuilder.ToString());
    }
}

// 3. MyGameEngine을 상속받아 구현한 테트리스 게임
class TetrisGame : MyGameEngine
{
    private const int BoardWidth = 12;  // 벽 포함 12칸
    private const int BoardHeight = 22; // 벽 포함 22칸

    private int[,] board = new int[BoardHeight, BoardWidth]; // 0:빈칸, 1:벽, 2:고정블록
    private Random rand = new Random();

    private readonly int[,,] shapes = new int
    {
        { {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} }, // I
        { {0,0,0,0}, { 0,1,1,0}, { 0,1,1,0}, { 0,0,0,0} }, // O
        { { 0,0,0,0}, { 0,1,0,0}, { 1,1,1,0}, { 0,0,0,0} }, // T
        { { 0,0,0,0}, { 0,1,1,0}, { 1,1,0,0}, { 0,0,0,0} }, // S
        { { 0,0,0,0}, { 1,1,0,0}, { 0,1,1,0}, { 0,0,0,0} }, // Z
        { { 0,0,0,0}, { 1,0,0,0}, { 1,1,1,0}, { 0,0,0,0} }, // J
        { { 0,0,0,0}, { 0,0,1,0}, { 1,1,1,0}, { 0,0,0,0} }  // L
    };

private int[,] currentShape = new int;
private int curX, curY;
private int dropCounter = 0;
private int dropInterval = 15;
private int score = 0;
private bool isGameOver = false;

// 부모 엔진(MyGameEngine)의 해상도를 테트리스 크기에 맞춰 초기화
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
            board[y, x] = (y == 0 || y == BoardHeight - 1 || x == 0 || x == BoardWidth - 1) ? 1 : 0;
        }
    }
    SpawnNewBlock();
}

private void SpawnNewBlock()
{
    int type = rand.Next(0, 7);
    for (int y = 0; y < 4; y++)
        for (int x = 0; x < 4; x++)
            currentShape[y, x] = shapes[type, y, x];

    curX = BoardWidth / 2 - 2;
    curY = 1;

    if (CheckCollision(curX, curY, currentShape))
        isGameOver = true;
}

public override void Update()
{
    base.Update();

    if (isGameOver)
    {
        if (CurrentKey?.Key == ConsoleKey.R)
            InitGame();
        return;
    }

    // 부모 엔진이 받아둔 CurrentKey 활용
    if (CurrentKey != null)
    {
        switch (CurrentKey.Value.Key)
        {
            case ConsoleKey.LeftArrow:
                if (!CheckCollision(curX - 1, curY, currentShape)) curX--;
                break;
            case ConsoleKey.RightArrow:
                if (!CheckCollision(curX + 1, curY, currentShape)) curX++;
                break;
            case ConsoleKey.DownArrow:
                if (!CheckCollision(curX, curY + 1, currentShape)) curY++;
                break;
            case ConsoleKey.UpArrow:
                RotateBlock();
                break;
            case ConsoleKey.Spacebar:
                while (!CheckCollision(curX, curY + 1, currentShape)) curY++;
                LockAndClear();
                break;
        }
    }

    // 자동 낙하
    dropCounter++;
    if (dropCounter >= dropInterval)
    {
        dropCounter = 0;
        if (!CheckCollision(curX, curY + 1, currentShape))
            curY++;
        else
            LockAndClear();
    }
}

public override void Rendering()
{
    // 1. 엔진 버퍼 초기화
    ClearBuffer();

    // 2. 상단 상태 텍스트 갱신
    if (isGameOver)
        SetStatusText($"SCORE: {score} | GAME OVER! (Press R to Restart)   ");
    else
        SetStatusText($"SCORE: {score} | ←→:이동 ↑:회전 Space:드롭       ");

    // 3. 고정된 보드 데이터를 엔진의 SetPixel로 그리기
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

    // 4. 현재 조작 중인 블록 그리기
    if (!isGameOver)
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (currentShape[y, x] == 1)
                    SetPixel(curX + x, curY + y, "▣");
            }
        }
    }

    // 5. 실제 콘솔 출력은 부모 엔진의 Rendering()에 위임
    base.Rendering();
}

private void LockAndClear()
{
    for (int y = 0; y < 4; y++)
        for (int x = 0; x < 4; x++)
            if (currentShape[y, x] == 1)
                board[curY + y, curX + x] = 2;

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
                for (int x = 1; x < BoardWidth - 1; x++)
                    board[moveY, x] = board[moveY - 1, x];

            for (int x = 1; x < BoardWidth - 1; x++)
                board[1, x] = 0;

            y++;
        }
    }
}

private void RotateBlock()
{
    int[,] rotated = new int;
    for (int y = 0; y < 4; y++)
        for (int x = 0; x < 4; x++)
            rotated[x, 3 - y] = currentShape[y, x];

    if (!CheckCollision(curX, curY, rotated))
        currentShape = rotated;
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


