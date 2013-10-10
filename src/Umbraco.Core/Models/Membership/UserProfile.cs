using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents the Profile implementation for a backoffice User
    /// </summary>
    /// <remarks>
    /// Should be internal until a proper user/membership implementation
    /// is part of the roadmap.
    /// </remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class UserProfile : Profile, IUserProfile
    {
        public UserProfile()
        {
            SessionTimeout = 60;
            _sectionCollection = new ObservableCollection<string>();
            _addedSections = new List<string>();
            _removedSections = new List<string>();
            _sectionCollection.CollectionChanged += SectionCollectionChanged;
        }

        private readonly List<string> _addedSections;
        private readonly List<string> _removedSections;
        private readonly ObservableCollection<string> _sectionCollection;
        private int _sessionTimeout;
        private int _startContentId;
        private int _startMediaId;

        private static readonly PropertyInfo SessionTimeoutSelector = ExpressionHelper.GetPropertyInfo<UserProfile, int>(x => x.SessionTimeout);
        private static readonly PropertyInfo StartContentIdSelector = ExpressionHelper.GetPropertyInfo<UserProfile, int>(x => x.StartContentId);
        private static readonly PropertyInfo StartMediaIdSelector = ExpressionHelper.GetPropertyInfo<UserProfile, int>(x => x.StartMediaId);
        private static readonly PropertyInfo AllowedSectionsSelector = ExpressionHelper.GetPropertyInfo<UserProfile, IEnumerable<string>>(x => x.AllowedSections);

        /// <summary>
        /// Gets or sets the session timeout.
        /// </summary>
        /// <value>
        /// The session timeout.
        /// </value>
        [DataMember]
        public int SessionTimeout
        {
            get
            {
                return _sessionTimeout;
            }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _sessionTimeout = value;
                    return _sessionTimeout;
                }, _sessionTimeout, SessionTimeoutSelector);
            }
        }

        /// <summary>
        /// Gets or sets the start content id.
        /// </summary>
        /// <value>
        /// The start content id.
        /// </value>
        [DataMember]
        public int StartContentId
        {
            get
            {
                return _startContentId;
            }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _startContentId = value;
                    return _startContentId;
                }, _startContentId, StartContentIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the start media id.
        /// </summary>
        /// <value>
        /// The start media id.
        /// </value>
        [DataMember]
        public int StartMediaId
        {
            get
            {
                return _startMediaId;
            }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _startMediaId = value;
                    return _startMediaId;
                }, _startMediaId, StartMediaIdSelector);
            }
        }

        public IEnumerable<string> AllowedSections
        {
            get { return _sectionCollection; }
        }

        public void RemoveAllowedSection(string sectionAlias)
        {
            _sectionCollection.Remove(sectionAlias);
        }

        public void AddAllowedSection(string sectionAlias)
        {
            if (!_sectionCollection.Contains(sectionAlias))
            {
                _sectionCollection.Add(sectionAlias);
            }
        }

        /// <summary>
        /// Whenever resetting occurs, clear the remembered add/removed collections, even if 
        /// rememberPreviouslyChangedProperties is true, the AllowedSections property will still
        /// be flagged as dirty.
        /// </summary>
        /// <param name="rememberPreviouslyChangedProperties"></param>
        public override void ResetDirtyProperties(bool rememberPreviouslyChangedProperties)
        {
            _addedSections.Clear();
            _removedSections.Clear();
            base.ResetDirtyProperties(rememberPreviouslyChangedProperties);
        }

        /// <summary>
        /// Used internally to check if we need to add a section in the repository to the db
        /// </summary>
        internal IEnumerable<string> AddedSections
        {
            get { return _addedSections; }
        }

        /// <summary>
        /// Used internally to check if we need to remove  a section in the repository to the db
        /// </summary>
        internal IEnumerable<string> RemovedSections
        {
            get { return _removedSections; }
        }

        /// <summary>
        /// Handles the collection changed event in order for us to flag the AllowedSections property as changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(AllowedSectionsSelector);

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //remove from the removed/added sections (since people could add/remove all they want in one request)
                _removedSections.RemoveAll(s => s == e.NewItems.Cast<string>().First());
                _addedSections.RemoveAll(s => s == e.NewItems.Cast<string>().First());

                //add to the added sections
                _addedSections.Add(e.NewItems.Cast<string>().First());
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove from the removed/added sections (since people could add/remove all they want in one request)
                _removedSections.RemoveAll(s => s == e.OldItems.Cast<string>().First());
                _addedSections.RemoveAll(s => s == e.OldItems.Cast<string>().First());

                //add to the added sections
                _removedSections.Add(e.OldItems.Cast<string>().First());
            }
        }
    }
}