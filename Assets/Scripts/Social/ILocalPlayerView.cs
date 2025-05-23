    
using System;
using Unity.Services.Friends.Models;

	public interface ILocalPlayerView
    {
        Action<(Availability, string)> onPresenceChanged { get; set; }

        void Refresh(string name, string activity,
            Availability presenceAvailabilityOptions);
    }