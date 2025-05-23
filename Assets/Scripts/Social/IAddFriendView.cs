using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public interface IAddFriendView
    {
        void FriendRequestSuccess();
        void FriendRequestFailed();
        Action<string> onFriendRequestSent { get; set; }
        void Show();
        void Hide();
    }
