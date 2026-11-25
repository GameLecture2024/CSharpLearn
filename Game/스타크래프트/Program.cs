using System;
using System.Collections.Generic;
using System.Threading;

namespace StarCraftSimulation
{
    class Program
    {
        // ==========================================
        // 1. 인터페이스 정의 (기능별 모듈화)
        // ==========================================
        public interface ICloakable
        {
            bool IsCloaked { get; }
            void ToggleCloak();
        }

        public interface IDetector
        {
            void Detect(List<Unit> allUnits);
        }

        public interface ITransport
        {
            int Capacity { get; }
            void Load(Unit target);
            void UnloadAll();
        }

        public interface IWorker
        {
            void GatherResource();
        }

        public interface IBurrowable
        {
            bool IsBurrowed { get; }
            void ToggleBurrow();
        }

        // ==========================================
        // 2. 열거형 정의 (유닛 속성)
        // ==========================================
        public enum MoveType { Ground, Air }
        public enum UnitAttribute { Biological, Mechanical }

        // ==========================================
        // 3. 추상 클래스: Unit (공통 속성 및 다형성 기반)
        // ==========================================
        public abstract class Unit
        {
            public string Name { get; protected set; }
            public int Hp { get; protected set; }
            public int MaxHp { get; protected set; }
            public MoveType Movement { get; protected set; }
            public UnitAttribute Attribute { get; protected set; }
            public int X { get; protected set; }
            public int Y { get; protected set; }

            protected Unit(string name, int maxHp, MoveType movement, UnitAttribute attribute, int x = 0, int y = 0)
            {
                Name = name;
                MaxHp = maxHp;
                Hp = maxHp;
                Movement = movement;
                Attribute = attribute;
                X = x;
                Y = y;
            }

            public virtual void Move(int dx, int dy)
            {
                X += dx;
                Y += dy;
            }

            public virtual void TakeDamage(int damage)
            {
                Hp = Math.Max(0, Hp - damage);
            }

            // 각 종족/유닛별 매 턴(프레임) 갱신 동작 (다형성)
            public abstract void Update();

            // 상태 출력용 문자열
            public virtual string GetStatus()
            {
                return $"[{Name}] HP: {Hp}/{MaxHp} | 위치: ({X},{Y}) | {Movement}/{Attribute}";
            }
        }

        // ==========================================
        // 4. 종족별 추상 클래스 (종족 고유 특성 구현)
        // ==========================================
        public abstract class TerranUnit : Unit
        {
            protected TerranUnit(string name, int maxHp, MoveType movement, UnitAttribute attribute, int x = 0, int y = 0)
                : base(name, maxHp, movement, attribute, x, y) { }

            public override void Update()
            {
                // 테란은 기본 자동 재생이 없음 (메딕/SCV 수리 필요)
            }
        }

        public abstract class ZergUnit : Unit
        {
            protected ZergUnit(string name, int maxHp, MoveType movement, UnitAttribute attribute, int x = 0, int y = 0)
                : base(name, maxHp, movement, attribute, x, y) { }

            public override void Update()
            {
                // 저그 고유 특성: 매 턴 체력 자동 재생
                if (Hp > 0 && Hp < MaxHp)
                {
                    Hp = Math.Min(MaxHp, Hp + 2);
                }
            }
        }

        public abstract class ProtossUnit : Unit
        {
            public int Shield { get; protected set; }
            public int MaxShield { get; protected set; }

            protected ProtossUnit(string name, int maxHp, int maxShield, MoveType movement, UnitAttribute attribute, int x = 0, int y = 0)
                : base(name, maxHp, movement, attribute, x, y)
            {
                MaxShield = maxShield;
                Shield = maxShield;
            }

            public override void TakeDamage(int damage)
            {
                if (Shield >= damage)
                {
                    Shield -= damage;
                }
                else
                {
                    int remaining = damage - Shield;
                    Shield = 0;
                    base.TakeDamage(remaining);
                }
            }

            public override void Update()
            {
                // 프로토스 고유 특성: 매 턴 보호막(Shield) 자동 재생
                if (Hp > 0 && Shield < MaxShield)
                {
                    Shield = Math.Min(MaxShield, Shield + 3);
                }
            }

            public override string GetStatus()
            {
                return $"[{Name}] HP: {Hp}/{MaxHp} | Shield: {Shield}/{MaxShield} | 위치: ({X},{Y}) | {Movement}/{Attribute}";
            }
        }

        // ==========================================
        // 5. 구체적 유닛 클래스 구현
        // ==========================================
        
        // [테란] SCV: 일꾼
        public class SCV : TerranUnit, IWorker
        {
            public int MinedMinerals { get; private set; }
            public SCV(int x, int y) : base("SCV", 60, MoveType.Ground, UnitAttribute.Mechanical, x, y) { }

            public void GatherResource()
            {
                MinedMinerals += 8;
            }

            public override string GetStatus() => base.GetStatus() + $" | 채취량: {MinedMinerals}";
        }

        // [테란] 레이스(Wraith): 클로킹 가능 공중 유닛
        public class Wraith : TerranUnit, ICloakable
        {
            public bool IsCloaked { get; private set; }
            public Wraith(int x, int y) : base("Wraith", 120, MoveType.Air, UnitAttribute.Mechanical, x, y) { }

            public void ToggleCloak() => IsCloaked = !IsCloaked;

            public override string GetStatus() => base.GetStatus() + (IsCloaked ? " [은폐중]" : "");
        }

        // [테란] 드랍쉽(Dropship): 수송선
        public class Dropship : TerranUnit, ITransport
        {
            public int Capacity => 8;
            private List<Unit> cargo = new List<Unit>();

            public Dropship(int x, int y) : base("Dropship", 150, MoveType.Air, UnitAttribute.Mechanical, x, y) { }

            public void Load(Unit target)
            {
                if (target.Movement == MoveType.Ground && cargo.Count < Capacity)
                {
                    cargo.Add(target);
                }
            }

            public void UnloadAll()
            {
                foreach (var u in cargo)
                {
                    u.Move(this.X - u.X, this.Y - u.Y);
                }
                cargo.Clear();
            }

            public override string GetStatus() => base.GetStatus() + $" | 탑승인원: {cargo.Count}/{Capacity}";
        }

        // [저그] 저글링(Zergling): 버로우 가능 지상 생체 유닛
        public class Zergling : ZergUnit, IBurrowable
        {
            public bool IsBurrowed { get; private set; }
            public Zergling(int x, int y) : base("Zergling", 35, MoveType.Ground, UnitAttribute.Biological, x, y) { }

            public void ToggleBurrow() => IsBurrowed = !IsBurrowed;

            public override void Move(int dx, int dy)
            {
                if (!IsBurrowed) base.Move(dx, dy); // 버로우 중에는 이동 불가
            }

            public override string GetStatus() => base.GetStatus() + (IsBurrowed ? " [잠복중]" : "");
        }

        // [프로토스] 옵저버(Observer): 영구 은폐 + 디텍터
        public class Observer : ProtossUnit, ICloakable, IDetector
        {
            public bool IsCloaked { get; private set; } = true;
            public string LastDetectedInfo { get; private set; } = "없음";

            public Observer(int x, int y) : base("Observer", 40, 20, MoveType.Air, UnitAttribute.Mechanical, x, y) { }

            public void ToggleCloak() { /* 옵저버는 패시브 영구 은폐 */ }

            public void Detect(List<Unit> allUnits)
            {
                List<string> detected = new List<string>();
                foreach (var u in allUnits)
                {
                    if (u == this) continue;
                    if (u is ICloakable c && c.IsCloaked) detected.Add($"{u.Name}(은폐)");
                    if (u is IBurrowable b && b.IsBurrowed) detected.Add($"{u.Name}(잠복)");
                }
                LastDetectedInfo = detected.Count > 0 ? string.Join(", ", detected) : "감지된 은신 유닛 없음";
            }

            public override string GetStatus() => base.GetStatus() + $" [은폐] | 탐지: {LastDetectedInfo}";
        }

        // ==========================================
        // 6. 게임 루프 (입력 -> 갱신 -> 렌더링)
        // ==========================================
        static List<Unit> units = new List<Unit>();
        static bool isRunning = true;
        static int turn = 1;
        static string lastActionLog = "게임이 시작되었습니다. 명령을 입력하세요.";

        static void Main(string[] args)
        {
            InitGame();

            while (isRunning)
            {
                Render();
                ProcessInput();
                Update();
                turn++;
            }

            Console.WriteLine("게임을 종료합니다.");
        }

        static void InitGame()
        {
            units.Add(new SCV(1, 1));
            units.Add(new Wraith(3, 4));
            units.Add(new Dropship(2, 2));
            units.Add(new Zergling(5, 5));
            units.Add(new Observer(4, 4));

            // 데미지 테스트용 초기 피해 부여 (재생 확인용)
            units[3].TakeDamage(15); // 저글링 HP 35 -> 20
            units[4].TakeDamage(15); // 옵저버 Shield 20 -> 5
        }

        // [1] 렌더링 (Rendering)
        static void Render()
        {
            Console.Clear();
            Console.WriteLine("=========================================================================");
            Console.WriteLine($"  StarCraft OOP 시뮬레이션 (Turn: {turn})");
            Console.WriteLine("=========================================================================");
            
            for (int i = 0; i < units.Count; i++)
            {
                Console.WriteLine($" {i + 1}. {units[i].GetStatus()}");
            }

            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine($" [로그] {lastActionLog}");
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine(" [명령어 메뉴]");
            Console.WriteLine("  1: 모든 유닛 이동 (+1, +1)      2: 일꾼 자원 채취 (IWorker)");
            Console.WriteLine("  3: 클로킹 전환 (ICloakable)     4: 저글링 잠복 전환 (IBurrowable)");
            Console.WriteLine("  5: 드랍쉽 탑승/하차 (ITransport) 6: 디텍터 스캔 실행 (IDetector)");
            Console.WriteLine("  7: 전체 유닛 데미지 (-10)       0: 게임 종료");
            Console.Write(" 선택 > ");
        }

        // [2] 입력 처리 (Input)
        static void ProcessInput()
        {
            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    foreach (var u in units) u.Move(1, 1);
                    lastActionLog = "모든 이동 가능한 유닛이 (1, 1)만큼 이동했습니다.";
                    break;

                case "2":
                    foreach (var u in units)
                    {
                        if (u is IWorker worker) worker.GatherResource();
                    }
                    lastActionLog = "IWorker 인터페이스를 가진 유닛들이 자원을 채취했습니다.";
                    break;

                case "3":
                    foreach (var u in units)
                    {
                        if (u is ICloakable cloakable) cloakable.ToggleCloak();
                    }
                    lastActionLog = "ICloakable 유닛들의 은폐 상태를 전환했습니다.";
                    break;

                case "4":
                    foreach (var u in units)
                    {
                        if (u is IBurrowable burrowable) burrowable.ToggleBurrow();
                    }
                    lastActionLog = "IBurrowable 유닛들의 잠복 상태를 전환했습니다.";
                    break;

                case "5":
                    Dropship dropship = units.Find(u => u is Dropship) as Dropship;
                    SCV scv = units.Find(u => u is SCV) as SCV;
                    if (dropship != null && scv != null)
                    {
                        dropship.Load(scv);
                        lastActionLog = "Dropship(ITransport)에 SCV를 탑승시켰습니다.";
                    }
                    break;

                case "6":
                    foreach (var u in units)
                    {
                        if (u is IDetector detector) detector.Detect(units);
                    }
                    lastActionLog = "IDetector 유닛이 주변의 은폐/잠복 유닛을 탐지했습니다.";
                    break;

                case "7":
                    foreach (var u in units) u.TakeDamage(10);
                    lastActionLog = "모든 유닛이 10의 피해를 입었습니다. (다음 턴부터 저그 HP / 프로토스 쉴드 재생)";
                    break;

                case "0":
                    isRunning = false;
                    break;

                default:
                    lastActionLog = "잘못된 입력입니다. 턴만 경과합니다.";
                    break;
            }
        }

        // [3] 게임 상태 갱신 (Update)
        static void Update()
        {
            if (!isRunning) return;

            // 다형성을 통해 각 종족의 고유 패시브(저그 HP 재생, 프로토스 Shield 재생) 일괄 실행
            foreach (var unit in units)
            {
                unit.Update();
            }
        }
    }
}
