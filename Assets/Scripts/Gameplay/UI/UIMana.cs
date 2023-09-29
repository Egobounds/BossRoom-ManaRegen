using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.BossRoom.Gameplay.UI
{
    /// <summary>
    /// UI object that visually represents an object's mana. Visuals are updated when NetworkVariable is modified.
    /// </summary>
    public class UIMana : MonoBehaviour
    {
        [SerializeField]
        Slider m_ManaPointsSlider;

        NetworkVariable<int> m_NetworkedMana;

        public void Initialize(NetworkVariable<int> networkedMana, int maxValue)
        {
            m_NetworkedMana = networkedMana;

            m_ManaPointsSlider.minValue = 0;
            m_ManaPointsSlider.maxValue = maxValue;
            ManaChanged(maxValue, maxValue);

            m_NetworkedMana.OnValueChanged += ManaChanged;
        }

        void ManaChanged(int previousValue, int newValue)
        {
            m_ManaPointsSlider.value = newValue;
            // disable slider when we're at full mana!
            m_ManaPointsSlider.gameObject.SetActive(m_ManaPointsSlider.value != m_ManaPointsSlider.maxValue);
        }

        void OnDestroy()
        {
            m_NetworkedMana.OnValueChanged -= ManaChanged;
        }
    }
}
