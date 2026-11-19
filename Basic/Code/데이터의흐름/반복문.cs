// 반복문
// for 반복문
// 언제 for 반복문 쓰면 더 좋을까요? 반복하는 횟수가 정해져있을 때 명확할 때

for (int dan = 2; dan <= 9; dan++)
{
    Console.WriteLine($"\n=== {dan}단 ===");
    for (int num = 1; num <= 9; num++)
    {
        Console.WriteLine($"{dan} × {num} = {dan * num}");
    }
}

// while 반복문
// 왜 써야하는가? 무한 반복문 표현. 무한히 반복할 때 표현.
// 어떤 로직이던 간에 일단 이 형태로 다 출발할 수 있다.
bool gameFinish = false;
int hp = 0;
while(true)
{

    // 게임이 종료되는 조건!
    if (hp <= 0) gameFinish = true;

    if(gameFinish)
    {
        break;
    }
}

// 게임은 항상 끝임없이 실행을 해야한다.

// 게임이다.
// 마우스를 한번 왼클릭 한다.
// 화면에서 망치가 움직이고
// 확률에 따라서 성공할 수 도 있고 안할 수 도 있다.
// 강화게임

// 반복문을 사용하는 게임 추천 예제.
// 반드시 내가 주석으로 지정한 표현 방식쓰고 너가 추가적으로 기능을 구현하지는 말아라.
// 만약에 그 상태에서 실행이 안된다면 안되는 이유를 나에게 말해라.