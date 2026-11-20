// 아이템은 어떤 종류가 있을 가요? 열거형

// 갯수는 몇개가 있을 가요?
// 이를 콘솔에서 어떻게 보여지게 할거에요?
// |  ----- | ------ | -----| ----- |
// '조건문, 반복문, 배열, 열거형'

// 아이템 10개. 10개를 어떻게 표현할까? 배열의 선언

// 아이템 1개씩 데이터를 넣어야 한다. 배열의 초기화

// 반복적으로 코드를 그려야 한다.

// 입력 - 업데이트 - 렌더링 방식으로 상점 기능이 실행이 되게 만들어주세요.

// 어떤 입력을 해서 무엇이 변경해야하고 그것을 어떻게 보여줘야 한다.
// 숫자를 1~10까지를 클릭하고 예약되어 잇는 명령이 실행이 된다.
//      예약되어 있는 내용
//      [1] 아이템 구매
//      [2] 상점 나가기..


using System;

class Program
{
    // 1. 아이템 종류 열거형 선언
    enum ItemType
    {
        Consumable, // 소모품
        Weapon,     // 무기
        Armor,      // 방어구
        Misc        // 기타
    }

    static void Main(string[] args)
    {
        const int ITEM_MAX_COUNT = 10;

        // 2. 아이템 10개 표현을 위한 배열 선언 및 초기화
        string[] names = new string[ITEM_MAX_COUNT]
        {
            "체력 물약", "마나 물약", "해독제", "귀환 주문서", "낡은 단검",
            "철제 검", "나무 방패", "가죽 갑옷", "초급 마법서", "황금 열쇠"
        };

        ItemType[] types = new ItemType[ITEM_MAX_COUNT]
        {
            ItemType.Consumable, ItemType.Consumable, ItemType.Consumable, ItemType.Consumable, ItemType.Weapon,
            ItemType.Weapon, ItemType.Armor, ItemType.Armor, ItemType.Weapon, ItemType.Misc
        };

        int[] counts = new int[ITEM_MAX_COUNT]
        {
            5, 3, 2, 1, 1,
            1, 1, 1, 1, 2
        };

        int[] prices = new int[ITEM_MAX_COUNT];
        for (int i = 0; i < ITEM_MAX_COUNT; i++)
        {
            prices[i] = 10 + 10 * i;
        }

        // 플레이어 상태 및 상점 상태 변수
        int playerGold = 500;
        bool isShopOpen = true;
        string systemMessage = "상점에 오신 것을 환영합니다!";

        // 3. 입력 - 업데이트 - 렌더링 루프
        while (isShopOpen)
        {
            // ================= [렌더링 (Render)] =================
            Console.Clear();
            RenderShopTable(names, types, counts, prices, playerGold, systemMessage);

            // ================= [입력 (Input)] =================
            Console.Write("\n실행할 명령 번호(1~10)를 입력하세요: ");
            string input = Console.ReadLine();
            int command = 0;
            bool isValidNumber = int.TryParse(input, out command);

            // ================= [업데이트 (Update)] =================
            if (!isValidNumber || command < 1 || command > 10)
            {
                systemMessage = "[오류] 1부터 10 사이의 숫자만 입력해주세요.";
                continue;
            }

            switch (command)
            {
                case 1: // [1] 아이템 구매
                    Console.Write("구매할 아이템 번호(1~10)를 입력하세요: ");
                    if (int.TryParse(Console.ReadLine(), out int itemNo) && itemNo >= 1 && itemNo <= ITEM_MAX_COUNT)
                    {
                        int idx = itemNo - 1;
                        if (counts[idx] <= 0)
                        {
                            systemMessage = $"[구매 실패] '{names[idx]}' 재고가 모두 소진되었습니다.";
                        }
                        else if (playerGold < prices[idx])
                        {
                            systemMessage = $"[구매 실패] 골드가 부족합니다. (필요: {prices[idx]} G)";
                        }
                        else
                        {
                            counts[idx]--;
                            playerGold -= prices[idx];
                            systemMessage = $"[구매 완료] '{names[idx]}'을(를) 구매했습니다! (-{prices[idx]} G)";
                        }
                    }
                    else
                    {
                        systemMessage = "[오류] 잘못된 아이템 번호입니다.";
                    }
                    break;

                case 2: // [2] 상점 나가기
                    isShopOpen = false;
                    Console.WriteLine("\n상점을 나갑니다. 이용해 주셔서 감사합니다!");
                    break;

                case 3: // [3~10] 샘플 예약 명령
                    systemMessage = "[샘플 3] 아이템 판매 기능 (준비 중입니다.)";
                    break;
                case 4:
                    systemMessage = "[샘플 4] 아이템 상세 정보 보기 (준비 중입니다.)";
                    break;
                case 5:
                    systemMessage = "[샘플 5] 내 인벤토리 확인 (준비 중입니다.)";
                    break;
                case 6:
                    systemMessage = "[샘플 6] 상점 물품 재입고 요청 (준비 중입니다.)";
                    break;
                case 7:
                    systemMessage = "[샘플 7] 가격 흥정하기 (준비 중입니다.)";
                    break;
                case 8:
                    systemMessage = "[샘플 8] 상점 주인과 대화하기 (준비 중입니다.)";
                    break;
                case 9:
                    systemMessage = "[샘플 9] 골드 100 G 치트 획득! (샘플 테스트)";
                    playerGold += 100;
                    break;
                case 10:
                    systemMessage = "[샘플 10] 도움말: 1번을 눌러 아이템을 구매할 수 있습니다.";
                    break;
            }
        }
    }

    // 상점 화면 렌더링 메서드
    static void RenderShopTable(string[] names, ItemType[] types, int[] counts, int[] prices, int gold, string message)
    {
        int colNoWidth = 4;
        int colTypeWidth = 8;
        int colNameWidth = 14;
        int colCountWidth = 6;
        int colPriceWidth = 8;

        Console.WriteLine($"==================== [ RPG 상점 ] ==================== (보유 골드: {gold} G)");
        Console.WriteLine("| ------ | ---------- | ---------------- | -------- | ---------- |");
        Console.WriteLine(
            $"| {PadRightDisplay("번호", colNoWidth)} " +
            $"| {PadRightDisplay("종류", colTypeWidth)} " +
            $"| {PadRightDisplay("아이템 이름", colNameWidth)} " +
            $"| {PadLeftDisplay("갯수", colCountWidth)} " +
            $"| {PadLeftDisplay("가격", colPriceWidth)} |"
        );
        Console.WriteLine("| ------ | ---------- | ---------------- | -------- | ---------- |");

        for (int i = 0; i < names.Length; i++)
        {
            string noText = (i + 1).ToString();
            string typeText = GetTypeName(types[i]);
            string nameText = names[i];
            string countText = counts[i] > 0 ? $"{counts[i]}개" : "품절";
            string priceText = $"{prices[i]} G";

            Console.WriteLine(
                $"| {PadRightDisplay(noText, colNoWidth)} " +
                $"| {PadRightDisplay(typeText, colTypeWidth)} " +
                $"| {PadRightDisplay(nameText, colNameWidth)} " +
                $"| {PadLeftDisplay(countText, colCountWidth)} " +
                $"| {PadLeftDisplay(priceText, colPriceWidth)} |"
            );
        }

        Console.WriteLine("| ------ | ---------- | ---------------- | -------- | ---------- |");
        Console.WriteLine($"알림: {message}");
        Console.WriteLine("--------------------------------------------------------------------");
        Console.WriteLine("[1] 아이템 구매   [2] 상점 나가기   [3] 판매(샘플)   [4] 정보(샘플)   [5] 가방(샘플)");
        Console.WriteLine("[6] 재입고(샘플)  [7] 흥정(샘플)    [8] 대화(샘플)   [9] +100G(샘플)  [10] 도움말(샘플)");
    }

    // 열거형 값을 한글 이름으로 변환
    static string GetTypeName(ItemType type)
    {
        switch (type)
        {
            case ItemType.Consumable: return "소모품";
            case ItemType.Weapon: return "무기";
            case ItemType.Armor: return "방어구";
            default: return "기타";
        }
    }

    // 한글 폭(2칸)을 반영한 좌측 정렬 공백 채우기
    static string PadRightDisplay(string text, int totalWidth)
    {
        int padding = totalWidth - GetDisplayWidth(text);
        return text + new string(' ', padding > 0 ? padding : 0);
    }

    // 한글 폭(2칸)을 반영한 우측 정렬 공백 채우기
    static string PadLeftDisplay(string text, int totalWidth)
    {
        int padding = totalWidth - GetDisplayWidth(text);
        return new string(' ', padding > 0 ? padding : 0) + text;
    }

    static int GetDisplayWidth(string text)
    {
        int width = 0;
        foreach (char c in text)
        {
            if ((c >= 0xAC00 && c <= 0xD7A3) || (c >= 0x1100 && c <= 0x11FF) || (c >= 0x3130 && c <= 0x318F))
                width += 2;
            else
                width += 1;
        }
        return width;
    }
}
