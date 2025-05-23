using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    public interface IBlockedListView : IListView
    {
        Action<string> onUnblock { get; set; }
        void BindList(List<PlayerProfile> playerProfiles);
    }
