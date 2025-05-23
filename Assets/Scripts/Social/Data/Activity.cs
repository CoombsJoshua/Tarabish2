using System.Runtime.Serialization;
using UnityEngine.Scripting;

    /// <summary>
    /// Activity represent the activity containing the status of a player.
    /// </summary>
    [Preserve]
    [DataContract]
    public class Activity
    {
        /// <summary>
        /// Status of the player.
        /// </summary>
        [Preserve]
        [DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
        public string Status { get; set; }
    }

