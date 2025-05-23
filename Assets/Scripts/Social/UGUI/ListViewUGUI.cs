using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public abstract class ListViewUGUI : MonoBehaviour
    {
        public void Show()
        {
            Refresh();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public abstract void Refresh();
    }
