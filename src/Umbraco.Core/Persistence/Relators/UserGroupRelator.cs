using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Relators
{
    internal class UserGroupRelator
    {
        private UserDto _currentUser;

        internal UserDto Map(UserDto user, UserGroupDto group, UserGroup2AppDto section)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (user == null)
                return _currentUser;

            // Is this the same User as the current one we're processing
            if (_currentUser != null && _currentUser.Id == user.Id)
            {
                AddOrUpdateGroup(group, section);

                // Return null to indicate we're not done with this object yet
                return null;
            }

            // This is a different user to the current one, or this is the 
            // first time through and we don't have one yet

            // Save the current user
            var prev = _currentUser;

            // Setup the new current user
            _currentUser = user;
            _currentUser.UserGroupDtos = new List<UserGroupDto>();

            AddOrUpdateGroup(group, section);

            // Return the now populated previous user (or null if first time through)
            return prev;
        }

        private void AddOrUpdateGroup(UserGroupDto group, UserGroup2AppDto section)
        {
            //I don't even think this situation can happen but if it could, we'd want the section added to the latest group check if this is a new group
            if (group == null && section != null)
            {                
                AddSection(section);
            }

            //this can be null since we are doing a left join
            if (group != null && group.Alias.IsNullOrWhiteSpace() == false)
            {
                //check if this is a new group
                var latestGroup = _currentUser.UserGroupDtos.Count > 0
                    ? _currentUser.UserGroupDtos[_currentUser.UserGroupDtos.Count - 1]
                    : null;

                if (latestGroup == null || latestGroup.Id != group.Id)
                {
                    //add the current (new) group
                    _currentUser.UserGroupDtos.Add(group);
                }

                AddSection(section);
            }
        }

        private void AddSection(UserGroup2AppDto section)
        {
            var latestGroup = _currentUser.UserGroupDtos.Count > 0
                ? _currentUser.UserGroupDtos[_currentUser.UserGroupDtos.Count - 1]
                : null;

            if (latestGroup == null || latestGroup.Id != section.UserGroupId)
                throw new InvalidOperationException("The user group and section info don't match");

            if (latestGroup.UserGroup2AppDtos == null)
                latestGroup.UserGroup2AppDtos = new List<UserGroup2AppDto>();

            //add it if it doesn't exist
            if (latestGroup.UserGroup2AppDtos.TrueForAll(dto => dto.AppAlias != section.AppAlias))
                latestGroup.UserGroup2AppDtos.Add(section);
        }
        
    }
}