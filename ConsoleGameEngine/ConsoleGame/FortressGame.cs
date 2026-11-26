using System;

namespace ConsoleTetris
{
    public class FortressGame : MyGameEngine
    {
        // 지형 높이 배열 (0 ~ Height-1)
        private int[] terrain;
        
        // 플레이어 정보 (0: 1P, 1: 2P)
        private int[] playerX = new int[2];
        private int[] playerHp = { 100, 100 };
        private int[] angle = { 45, 135 }; // 발사 각도 (도)
        private int[] power = { 15, 15 };  // 발사 파워

        private int currentTurn = 0;       // 현재 턴 (0 또는 1)
        private double wind = 0;           // 바람 (-3.0 ~ +3.0)
        private bool isGameOver = false;

        // 포탄 물리 계산용 변수
        private bool isFiring = false;
        private double bulletX, bulletY;
        private double bulletVx, bulletVy;
        private const double Gravity = 9.8;
        private const double TimeStep = 0.08; // 프레임당 시간 흐름

        private Random rand = new Random();

        // 40x25 크기의 넓은 전장 생성
        public FortressGame() : base(width: 40, height: 25)
        {
            InitGame();
        }

        private void InitGame()
        {
            terrain = new int[Width];
            playerHp[0] = 100;
            playerHp[1] = 100;
            isGameOver = false;
            isFiring = false;
            currentTurn = 0;

            GenerateTerrain();
            PlacePlayers();
            RandomizeWind();
        }

        // 사인파를 이용한 랜덤 언덕 지형 생성
        private void GenerateTerrain()
        {
            double offset = rand.NextDouble() * 10;
            double freq = 0.12 + rand.NextDouble() * 0.08;
            for (int x = 0; x < Width; x++)
            {
                // 화면 하단부(12 ~ 22행 사이)에 지형 형성
                int h = (int)(17 + Math.Sin(x * freq + offset) * 4);
                terrain[x] = Math.Clamp(h, 10, Height - 2);
            }
        }

        // 양쪽 끝에 탱크 배치 및 발판 평탄화
        private void PlacePlayers()
        {
            playerX[0] = rand.Next(3, 8);
            playerX[1] = rand.Next(Width - 8, Width - 3);

            // 탱크가 서 있는 곳 주변 평탄화
            for (int i = -1; i <= 1; i++)
            {
                terrain[playerX[0] + i] = terrain[playerX[0]];
                terrain[playerX[1] + i] = terrain[playerX[1]];
            }
        }

        private void RandomizeWind()
        {
            // -2.5 ~ +2.5 사이의 바람 생성
            wind = Math.Round((rand.NextDouble() * 5.0) - 2.5, 1);
        }

        public override void Update()
        {
            if (isGameOver)
            {
                HandleGameOverInput();
                return;
            }

            if (isFiring)
            {
                UpdateBullet();
            }
            else
            {
                HandlePlayerInput();
            }

            UpdateUI();
            DrawScene();
        }

        private void HandleGameOverInput()
        {
            if (CurrentKey?.Key == ConsoleKey.R)
            {
                Console.Clear();
                InitGame();
            }
        }

        private void HandlePlayerInput()
        {
            if (CurrentKey == null) return;

            int p = currentTurn;
            switch (CurrentKey.Value.Key)
            {
                // 좌우 이동 (지형 단차가 1칸 이하일 때만 이동 가능)
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    MoveTank(p, -1);
                    break;
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    MoveTank(p, 1);
                    break;

                // 각도 조절
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    angle[p] = Math.Clamp(angle[p] + 3, 10, 170);
                    break;
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    angle[p] = Math.Clamp(angle[p] - 3, 10, 170);
                    break;

                // 파워 조절 (Q / E)
                case ConsoleKey.E:
                    power[p] = Math.Clamp(power[p] + 1, 5, 30);
                    break;
                case ConsoleKey.Q:
                    power[p] = Math.Clamp(power[p] - 1, 5, 30);
                    break;

                // 발사!
                case ConsoleKey.Spacebar:
                    FireBullet(p);
                    break;
            }
        }

        private void MoveTank(int p, int dir)
        {
            int nextX = playerX[p] + dir;
            if (nextX < 1 || nextX >= Width - 1) return;

            // 언덕 경사가 너무 가파르면(2칸 이상 차이) 올라가지 못함
            if (Math.Abs(terrain[nextX] - terrain[playerX[p]]) <= 1)
            {
                playerX[p] = nextX;
            }
        }

        private void FireBullet(int p)
        {
            isFiring = true;
            bulletX = playerX[p];
            bulletY = terrain[playerX[p]] - 1.5; // 탱크 포신 위에서 발사

            double rad = angle[p] * Math.PI / 180.0;
            bulletVx = Math.Cos(rad) * power[p];
            bulletVy = -Math.Sin(rad) * power[p]; // 콘솔 Y축은 아래가 +이므로 음수 적용
        }

        private void UpdateBullet()
        {
            // 포물선 운동 공식 (바람과 중력 적용)
            bulletVx += wind * TimeStep;
            bulletVy += Gravity * TimeStep;

            bulletX += bulletVx * TimeStep;
            bulletY += bulletVy * TimeStep;

            int bx = (int)Math.Round(bulletX);
            int by = (int)Math.Round(bulletY);

            // 1. 화면 좌우 또는 바닥 밖으로 나간 경우
            if (bx < 0 || bx >= Width || by >= Height)
            {
                EndTurn();
                return;
            }

            // 2. 적 또는 내 탱크에 직격했는지 판정
            for (int i = 0; i < 2; i++)
            {
                if (bx == playerX[i] && by >= terrain[playerX[i]] - 1)
                {
                    Explode(bx, by);
                    EndTurn();
                    return;
                }
            }

            // 3. 땅(지형)에 충돌했는지 판정
            if (by >= 0 && by >= terrain[bx])
            {
                Explode(bx, terrain[bx]);
                EndTurn();
            }
        }

        // 폭발 판정 (지형 파괴 및 스플래시 데미지)
        private void Explode(int impactX, int impactY)
        {
            int blastRadius = 3;

            // 지형 파내기
            for (int x = impactX - blastRadius; x <= impactX + blastRadius; x++)
            {
                if (x >= 0 && x < Width)
                {
                    int dist = Math.Abs(x - impactX);
                    int depth = blastRadius - dist + 1;
                    terrain[x] = Math.Min(Height - 1, terrain[x] + depth);
                }
            }

            // 탱크 데미지 계산 (거리에 비례)
            for (int i = 0; i < 2; i++)
            {
                double dist = Math.Sqrt(Math.Pow(playerX[i] - impactX, 2) + Math.Pow(terrain[playerX[i]] - impactY, 2));
                if (dist <= blastRadius + 1)
                {
                    int damage = (int)((blastRadius + 2 - dist) * 15);
                    playerHp[i] = Math.Max(0, playerHp[i] - damage);
                }
            }

            // 승패 확인
            if (playerHp[0] <= 0 || playerHp[1] <= 0)
            {
                isGameOver = true;
            }
        }

        private void EndTurn()
        {
            isFiring = false;
            if (!isGameOver)
            {
                currentTurn = 1 - currentTurn; // 턴 교대 (0 -> 1, 1 -> 0)
                RandomizeWind();
            }
        }

        private void UpdateUI()
        {
            if (isGameOver)
            {
                string winner = playerHp[0] > 0 ? "1P(적색 탱크)" : "2P(청색 탱크)";
                SetStatusText($"★ 게임 종료! {winner} 승리! [R키: 재시작] ★");
                return;
            }

            string windStr = wind == 0 ? "무풍" : (wind > 0 ? $"바람: → {wind:F1}" : $"바람: ← {Math.Abs(wind):F1}");
            int p = currentTurn;
            
            string info = $"[{(p + 1)}P 턴]  1P HP:{playerHp[0]} | 2P HP:{playerHp[1]} | {windStr}\n" +
                          $"각도: {angle[p]}° (W/S) | 파워: {power[p]} (Q/E) | 이동: A/D | 발사: Space";
            SetStatusText(info);
        }

        private void DrawScene()
        {
            ClearBuffer();

            // 1. 지형 그리기
            for (int x = 0; x < Width; x++)
            {
                for (int y = terrain[x]; y < Height; y++)
                {
                    // 표면은 잔디(▒▒), 땅속은 흙(██)으로 표현
                    SetPixel(x, y, y == terrain[x] ? "▒▒" : "██");
                }
            }

            // 2. 플레이어 탱크 그리기 (지형 바로 위에 배치)
            if (playerHp[0] > 0)
                SetPixel(playerX[0], terrain[playerX[0]] - 1, "◀■"); // 1P 탱크
            
            if (playerHp[1] > 0)
                SetPixel(playerX[1], terrain[playerX[1]] - 1, "■▶"); // 2P 탱크

            // 3. 조준선 가이드 (발사 전 현재 턴 플레이어 주변에 조준점 표시)
            if (!isFiring && !isGameOver)
            {
                int p = currentTurn;
                double rad = angle[p] * Math.PI / 180.0;
                int aimX = playerX[p] + (int)Math.Round(Math.Cos(rad) * 2);
                int aimY = (terrain[playerX[p]] - 1) - (int)Math.Round(Math.Sin(rad) * 2);
                SetPixel(aimX, aimY, "· ");
            }

            // 4. 날아가는 포탄 그리기
            if (isFiring)
            {
                int bx = (int)Math.Round(bulletX);
                int by = (int)Math.Round(bulletY);
                SetPixel(bx, by, "● ");
            }
        }
    }
}
