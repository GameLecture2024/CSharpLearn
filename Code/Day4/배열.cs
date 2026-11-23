// 배열

// 왜 쓸까요? 
// 반복문. 갯수가 정해졋을 때 쓰면 'for'

// 어떻게 배열을 사용하면 n개를 하나의 코드로 표현할 수 잇을까요?
// 배열의 구현 원리를 알면 설명할 수 있다. -> 자료구조.

int[] numbers = new int[10];

for (int i =0; i < 10; i++)
{
    numbers[i] = i;
    Console.WriteLine($"현재 값은 : {numbers[i]}" );
}

// 배열은 같은 자료형을 한번에 표현할 수 있다.
// 갯수를 정하면 시작부터 끝까지 데이터를 저장하고 언제든지 꺼내서 쓸 수 있다.

// 가격은 가격인데... (xx)가격 너무 많은대? price 마음껏 써도 되나?

const int ITEM_MAX_COUNT = 10;

string[] names = new string[ITEM_MAX_COUNT]
{
            "체력 물약", "마나 물약", "해독제", "귀환 주문서", "낡은 단검",
            "철제 검", "나무 방패", "가죽 갑옷", "초급 마법서", "황금 열쇠"
};

int[] counts = new int[ITEM_MAX_COUNT]
{
            5, 3, 2, 1, 1,
            1, 1, 1, 1, 2
};

int[] prices = new int[ITEM_MAX_COUNT];

// 각 열의 목표 표시 너비 (한글=2칸, 영문/숫자/공백=1칸 기준)
int colNoWidth = 6;
int colNameWidth = 16;
int colCountWidth = 8;
int colPriceWidth = 10;

string headerNo = "번호";
string headerName = "아이템 이름";
string headerCount = "갯수";
string headerPrice = "가격";

Console.WriteLine("=================================================");
Console.WriteLine(
    $"{headerNo}{new string(' ', colNoWidth - GetDisplayWidth(headerNo))} | " +
    $"{headerName}{new string(' ', colNameWidth - GetDisplayWidth(headerName))} | " +
    $"{new string(' ', colCountWidth - GetDisplayWidth(headerCount))}{headerCount} | " +
    $"{new string(' ', colPriceWidth - GetDisplayWidth(headerPrice))}{headerPrice}"
);
Console.WriteLine("=================================================");

for (int i = 0; i < ITEM_MAX_COUNT; i++)
{
    prices[i] = 10 + 10 * i;

    string noText = (i + 1).ToString();
    string nameText = names[i];
    string countText = $"{counts[i]}개";
    string priceText = $"{prices[i]} G";

    // 부족한 너비만큼 공백을 직접 계산해 좌/우 정렬
    string padNo = new string(' ', colNoWidth - GetDisplayWidth(noText));
    string padName = new string(' ', colNameWidth - GetDisplayWidth(nameText));
    string padCount = new string(' ', colCountWidth - GetDisplayWidth(countText));
    string padPrice = new string(' ', colPriceWidth - GetDisplayWidth(priceText));

    Console.WriteLine($"{noText}{padNo} | {nameText}{padName} | {padCount}{countText} | {padPrice}{priceText}");
}

Console.WriteLine("=================================================");
    

    // 문자열의 실제 콘솔 출력 너비 계산 (한글 2칸, 그 외 1칸)
    static int GetDisplayWidth(string text)
{
    int width = 0;
    foreach (char c in text)
    {
        // 한글 음절 및 자모 범위 체크
        if ((c >= 0xAC00 && c <= 0xD7A3) || (c >= 0x1100 && c <= 0x11FF) || (c >= 0x3130 && c <= 0x318F))
            width += 2;
        else
            width += 1;
    }
    return width;
}
