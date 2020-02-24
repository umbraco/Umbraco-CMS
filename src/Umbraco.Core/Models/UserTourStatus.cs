﻿using System;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// A model representing the tours a user has taken/completed
    /// </summary>
    [DataContract(Name = "userTourStatus", Namespace = "")]
    public class UserTourStatus : IEquatable<UserTourStatus>
    {
        /// <summary>
        /// The tour alias
        /// </summary>
        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        /// <summary>
        /// If the tour is completed
        /// </summary>
        [DataMember(Name = "completed")]
        public bool Completed { get; set; }

        /// <summary>
        /// If the tour is disabled
        /// </summary>
        [DataMember(Name = "disabled")]
        public bool Disabled { get; set; }

        public bool Equals(UserTourStatus other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Alias, other.Alias);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserTourStatus) obj);
        }

        public override int GetHashCode()
        {
            return Alias.GetHashCode();
        }

        public static bool operator ==(UserTourStatus left, UserTourStatus right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UserTourStatus left, UserTourStatus right)
        {
            return !Equals(left, right);
        }
    }
}