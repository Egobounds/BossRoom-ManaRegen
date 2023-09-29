using System;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using Unity.Netcode;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.GameplayObjects
{
    public class ManaReceiver : NetworkBehaviour
    {
        public event Action<ServerCharacter, int> ManaReceived;

        public void ReceiveMana(ServerCharacter inflicter, int mana)
        {
            ManaReceived?.Invoke(inflicter, mana);           
        }
    }
}
