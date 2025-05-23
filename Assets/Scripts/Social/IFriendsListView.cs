using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public interface IFriendsListView : IListView
    {
        Action<string> onRemove { get; set; }
        Action<string> onBlock { get; set; }
        void BindList(List<FriendsEntryData> friendEntryDatas);
    }
