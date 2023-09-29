using System;
using System.Collections.Generic;
using Unity.BossRoom.Gameplay.GameplayObjects;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.Actions
{
    /// <summary>
    /// Action that represents mana regeneration aura.
    /// </summary>
    /// <remarks>
    /// Q: Why do we DetectFoe twice, once in Start, once when we actually connect?
    /// A: The weapon swing doesn't happen instantaneously. We want to broadcast the action to other clients as fast as possible to minimize latency,
    ///    but this poses a conundrum. At the moment the swing starts, you don't know for sure if you've hit anybody yet. There are a few possible resolutions to this:
    ///      1. Do the DetectFoe operation once--in Start.
    ///         Pros: Simple! Only one physics cast per swing--saves on perf.
    ///         Cons: Is unfair. You can step out of the swing of an attack, but no matter how far you go, you'll still be hit. The reverse is also true--you can
    ///               "step into an attack", and it won't affect you. This will feel terrible to the attacker.
    ///      2. Do the DetectFoe operation once--in Update. Send a separate RPC to the targeted entity telling it to play its hit react.
    ///         Pros: Always shows the correct behavior. The entity that gets hit plays its hit react (if any).
    ///         Cons: You need another RPC. Adds code complexity and bandwidth. You also don't have enough information when you start visualizing the swing on
    ///               the client to do any intelligent animation handshaking. If your server->client latency is even a little uneven, your "attack" animation
    ///               won't line up correctly with the hit react, making combat look floaty and disjointed.
    ///      3. Do the DetectFoe operation twice, once in Start and once in Update.
    ///         Pros: Is fair--you do the hit-detect at the moment of the swing striking home. And will generally play the hit react on the right target.
    ///         Cons: Requires more complicated visualization logic. The initial broadcast foe can only ever be treated as a "hint". The graphics logic
    ///               needs to do its own range checking to pick the best candidate to play the hit react on.
    ///
    /// As so often happens in networked games (and games in general), there's no perfect solution--just sets of tradeoffs. For our example, we're showing option "3".
    /// </remarks>
    [CreateAssetMenu(menuName = "BossRoom/Actions/Mana Aura Action")]
    public partial class ManaAuraAction : Action
    {     
        public float m_TickPeriod;

        private float m_LastTimeFired;
        private List<ManaReceiver> receivers = new List<ManaReceiver>();
        

        public override bool OnStart(ServerCharacter serverCharacter)
        {
            DetectReceivers(serverCharacter, ref receivers);
            m_LastTimeFired = Time.time;

            serverCharacter.serverAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
            serverCharacter.clientCharacter.RecvDoActionClientRPC(Data);
            return true;
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            int manaRegenTicks = (int)((Time.time - m_LastTimeFired) / m_TickPeriod);

            if (Time.time - m_LastTimeFired > m_TickPeriod && (Time.time - TimeStarted) >= Config.ExecTimeSeconds)
            {
                DetectReceivers(clientCharacter, ref receivers);
                foreach (var receiver in receivers)
                {
                    receiver.ReceiveMana(clientCharacter, Config.Amount * manaRegenTicks);
                }

                m_LastTimeFired = Time.time;
            }

            return true;
        }

        /// <summary>
        /// Saves allies in aura range in the receivers list
        /// </summary>
        /// <returns></returns>
        private void DetectReceivers(ServerCharacter parent, ref List<ManaReceiver> receivers)
        {
            receivers.Clear();
            RaycastHit[] results;
            int numResults = ActionUtils.DetectEntitiesInRange(true, false, parent.physicsWrapper.DamageCollider, Config.Range, out results);

            for (int i = 0; i < numResults; i++)
            {
                var receiver = results[i].collider.GetComponent<ManaReceiver>();
                if (receiver != null)
                {
                    receivers.Add(receiver);                
                }
            }
        }

    }
}
