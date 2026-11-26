// 클래스를 만들어야 합니다.


// L  

// 추상 클래스. 공통적인 기능들을 모두 빼낸다.
// 테트리스블록이 공통적으로 가져야 할 요소가 무엇일까요? 
// 회전, 떨어질 수 있어야 한다, 한줄이 채워지면 사라져야한다, 겹치지 않는다, 
// 색깔, 모양

// I미노 is 블록 -> 상속
// 정사각형 -> 회전을 할 수 도 있다. => 회전가능한 IRollable 인터페이스


class 블록
{
    public string shape = "■";

    public 블록(string shape)
    {
        this.shape = shape;
    }
}

// 블록의 모든 성질을 가지면서 새로운 독특한 기능을 한번 추가하고 싶다.
// 시간 제한이 있어서 일정시간 후에 블록이 파괴된다. 위에 있는 블록은 자동으로 아래로 내려온다.

class TimerBlock : 블록
{
    public int deathTimer = 3;

    public TimerBlock(string shape) : base(shape)
    {
    }

    public void TimeDeath() // 시간이 지났을 때 없어지는 기능을 영어로 바꿔서 ()
    {
        // 몇초뒤에 블록이 사라질지 있어야 한다.
    }
}

// 또다른 블록 클래스 만들어서 색다른 테트리스 게임을 만들어보기
// 땅에 떨어지면 위로 블록이 점프를 한다. 막혀잇지않으면 떨어지고 있는 블록과 부딪혀서 내려온다.
class JumpBlock : 블록
{
    public int jumpHeight = 3;      // 최대 점프할 수 있는 칸 수
    public int currentJump = 0;     // 현재 점프해서 올라간 칸 수
    public bool isJumping = false;  // 현재 위로 점프 중인지 여부
    public bool hasJumped = false;  // 땅에 닿은 후 점프를 이미 실행했는지 여부

    public JumpBlock(string shape) : base(shape)
    {
    }

    // 땅(바닥이나 다른 고정 블록)에 처음 떨어졌을 때 점프 시작
    public void StartJump()
    {
        if (!hasJumped)
        {
            isJumping = true;
            hasJumped = true;
            currentJump = 0;
        }
    }

    // 매 프레임(Update) 호출: 위로 올라가거나, 장애물과 부딪히면 하강 상태로 전환
    // 반환값: 위(-1)로 이동, 아래(+1)로 이동, 제자리(0)
    public int UpdateJump(bool isBlockedAbove, bool hitFallingBlock)
    {
        if (isJumping)
        {
            // 위쪽이 막혀 있거나, 위에서 떨어지고 있는 블록과 부딪혔거나, 최대 높이에 도달한 경우
            if (isBlockedAbove || hitFallingBlock || currentJump >= jumpHeight)
            {
                isJumping = false; // 점프를 멈추고 다시 내려오도록 전환
                return 0;
            }

            // 막혀있지 않으면 위로 1칸 이동
            currentJump++;
            return -1;
        }

        // 점프가 끝난 뒤 공중에 떠 있는 상태라면 아래로 1칸 내려옴
        if (hasJumped && currentJump > 0)
        {
            currentJump--;
            return 1;
        }

        return 0;
    }
}