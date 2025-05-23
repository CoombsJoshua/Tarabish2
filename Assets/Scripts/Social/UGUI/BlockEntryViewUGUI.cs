using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlockEntryViewUGUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_NameText = null;

        public Button unblockButton = null;

        public void Init(string playerName)
        {
            m_NameText.text = playerName;
        }
    }
