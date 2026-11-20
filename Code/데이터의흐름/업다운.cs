// 정답을 한번 생각해봐. 단 숫자 범위는 1 ~ 100
// 언제까지 횟수 몇번? 5번 -> for반복문 5번만 해라
// 숫자를 틀렷네
// 숫자가 더 크네? up을 말해
// 숫자가 작네 -> down을 말해
// 숫자가 같네 -> 정답.
// 5번안에 못맞췄으면? -> 너 졌다.
using System;

class Program
{
    static void Main()
    {
        // 정답을 한번 생각해봐. 단 숫자 범위는 1 ~ 100
        Random random = new Random();
        int answer = random.Next(1, 101);

        // 언제까지 횟수 몇번? 5번 -> for반복문 5번만 해라
        for (int i = 0; i < 5; i++)
        {
            int guess = int.Parse(Console.ReadLine());

            // 숫자를 틀렷네
            if (guess != answer)
            {
                // 숫자가 더 크네? up을 말해
                if (guess < answer)
                {
                    Console.WriteLine("up");
                }
                // 숫자가 작네 -> down을 말해
                else if (guess > answer)
                {
                    Console.WriteLine("down");
                }
            }
            // 숫자가 같네 -> 정답
            else
            {
                Console.WriteLine("정답");
                break;
            }
        }
    }
}
