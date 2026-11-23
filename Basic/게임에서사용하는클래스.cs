// AI를 활용한 배운 문법을 작성하는 페이지 입니다.

// 클래스. 생성자.
// 클래스 무엇을 할래? 

// RPG 턴제. 전투. 포켓몬스터
// '플레이어'(똑같은 녀석) '적' (도감에 있는 모든 몬스터)
// Unit - 전투를 한다. 플레이어도하고 상대방도 똑같은 방식으로 한다.

// AI '읽을 수 있는 범위' 만큼만 만들어달라.
// 클래스 한개만 만들어라.
// 내가 어떻게 만들건지 문법을 설명할거다.

// AI. 정해지지 않은 내용은 자기가 알아서 만든다. 다음 똑같이 프롬프트를 입력해도 결과가 다르게 나온다.
// 클래스를 직접 설계 하신다음에.
// 그 클래스로 while반복문 입력,조작,rendering만들어줘.

public class Unit
{
    // 구분을 위해 이름 속성 추가
    public string Name { get; private set; }
    public int Health { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public int Speed { get; private set; }

    public bool IsAlive => Health > 0;

    public Unit(string name, int health, int attack, int defense, int speed)
    {
        Name = name;
        Health = health;
        Attack = attack;
        Defense = defense;
        Speed = speed;
    }

    // 1턴의 전투만 진행하도록 수정 (Update 단계에서 호출)
    public void BattleTurn(Unit target)
    {
        Unit firstAttacker = this.Speed >= target.Speed ? this : target;
        Unit secondAttacker = firstAttacker == this ? target : this;

        firstAttacker.AttackTarget(secondAttacker);

        if (!secondAttacker.IsAlive)
            return;

        secondAttacker.AttackTarget(firstAttacker);
    }

    private void AttackTarget(Unit defender)
    {
        int damage = Math.Max(1, this.Attack - defender.Defense);
        defender.Health = Math.Max(0, defender.Health - damage);

        Console.WriteLine($"[{this.Name}]이(가) [{defender.Name}]에게 {damage} 피해를 줍니다. (남은 체력: {defender.Health})");
    }
}
