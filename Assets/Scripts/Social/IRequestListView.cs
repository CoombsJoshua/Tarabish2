using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public interface IRequestListView : IListView
    {
        Action<string> onAccept { get; set; }
        Action<string> onDecline { get; set; }
        Action<string> onBlock { get; set; }
        void BindList(List<PlayerProfile> playerProfiles);
    }
