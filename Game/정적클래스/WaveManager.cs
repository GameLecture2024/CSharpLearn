using System;
using System.Collections.Generic;

// (1) 정적 클래스: 몬스터 생존 수 및 웨이브 관리

public static class WaveManager
{
    public static int CurrentWave { get; private set; } = 0;
    public static int AliveCount { get; private set; } = 0;

    // 몬스터 사망 시 호출되어 생존 수 감소
    public static void OnMonsterDied()
    {
        if (AliveCount > 0)
        {
            AliveCount--;
        }
    }

    // 다음 웨이브 발생 및 n마리 몬스터 생성
    public static void SpawnNextWave(List<Monster> monsters, int mapWidth)
    {
        CurrentWave++;
        int spawnCount = CurrentWave + 2; // 웨이브마다 소환 수 증가 (n마리)
        AliveCount = spawnCount;

        Random rand = new Random();
        for (int i = 0; i < spawnCount; i++)
        {
            int x = rand.Next(2, mapWidth - 2);
            int y = rand.Next(2, 6); // 상단 영역에 배치
            monsters.Add(new Monster(x, y));
        }
    }
}
