using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class ABlockManager : MonoBehaviour
    {
        public List<ABlockController> ABlocks { get; private set; }
        public int NumberOfEnemiesTotal { get; private set; }
        public int NumberOfEnemiesRemaining => ABlocks.Count;

        void Awake()
        {
            ABlocks = new List<ABlockController>();
        }

        public void RegisterABlock(ABlockController enemy)
        {
            ABlocks.Add(enemy);

            NumberOfEnemiesTotal++;
        }

        public void UnregisterABlock(ABlockController enemyKilled)
        {
            int enemiesRemainingNotification = NumberOfEnemiesRemaining - 1;

            EnemyKillEvent evt = Events.EnemyKillEvent;
            evt.Enemy = enemyKilled.gameObject;
            evt.RemainingEnemyCount = enemiesRemainingNotification;
            // EventManager.Broadcast(evt);

            // removes the enemy from the list, so that we can keep track of how many are left on the map
            ABlocks.Remove(enemyKilled);
        }
    }
}