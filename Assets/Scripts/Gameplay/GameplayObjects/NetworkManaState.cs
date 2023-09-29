using System;
using Unity.Netcode;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.GameplayObjects
{
    /// <summary>
    /// MonoBehaviour containing only one NetworkVariableInt which represents this object's mana.
    /// </summary>
    public class NetworkManaState : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<int> ManaPoints = new NetworkVariable<int>();

        // public subscribable event to be invoked when mana has been fully depleted
        public event Action ManaPointsDepleted;

        // public subscribable event to be invoked when mana has been replenished
        public event Action ManaPointsReplenished;

        void OnEnable()
        {
            ManaPoints.OnValueChanged += ManaPointsChanged;
        }

        void OnDisable()
        {
            ManaPoints.OnValueChanged -= ManaPointsChanged;
        }

        void ManaPointsChanged(int previousValue, int newValue)
        {
            if (previousValue > 0 && newValue <= 0)
            {
                // newly reached 0 Mana
                ManaPointsDepleted?.Invoke();
            }
            else if (previousValue <= 0 && newValue > 0)
            {
                ManaPointsReplenished?.Invoke();
            }
        }
    }
}
