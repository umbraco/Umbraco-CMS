using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Initial
{
    /// <summary>
    /// Represents the initial database schema creation by running CreateTable for all DTOs against the db.
    /// </summary>
    internal class DatabaseSchemaCreation
    {
        #region Private Members
        private readonly Database _database;
        private static readonly Dictionary<int, Type> OrderedTables = new Dictionary<int, Type>
                                                                          {
                                                                              {0, typeof (NodeDto)},
                                                                              {1, typeof (TemplateDto)},
                                                                              {2, typeof (ContentDto)},
                                                                              {3, typeof (ContentVersionDto)},
                                                                              {4, typeof (DocumentDto)},
                                                                              {5, typeof (ContentTypeDto)},
                                                                              {6, typeof (DocumentTypeDto)},
                                                                              {7, typeof (DataTypeDto)},
                                                                              {8, typeof (DataTypePreValueDto)},
                                                                              {9, typeof (DictionaryDto)},
                                                                              {10, typeof (LanguageTextDto)},
                                                                              {11, typeof (LanguageDto)},
                                                                              {12, typeof (DomainDto)},
                                                                              {13, typeof (LogDto)},
                                                                              {14, typeof (MacroDto)},
                                                                              {15, typeof (MacroPropertyTypeDto)},
                                                                              {16, typeof (MacroPropertyDto)},
                                                                              {17, typeof (MemberTypeDto)},
                                                                              {18, typeof (MemberDto)},
                                                                              {19, typeof (Member2MemberGroupDto)},
                                                                              {20, typeof (ContentXmlDto)},
                                                                              {21, typeof (PreviewXmlDto)},
                                                                              {22, typeof (PropertyTypeGroupDto)},
                                                                              {23, typeof (PropertyTypeDto)},
                                                                              {24, typeof (PropertyDataDto)},
                                                                              {25, typeof (RelationTypeDto)},
                                                                              {26, typeof (RelationDto)},
                                                                              {27, typeof (StylesheetDto)},
                                                                              {28, typeof (StylesheetPropertyDto)},
                                                                              {29, typeof (TagDto)},
                                                                              {30, typeof (TagRelationshipDto)},
                                                                              {31, typeof (UserLoginDto)},
                                                                              {32, typeof (UserTypeDto)},
                                                                              {33, typeof (UserDto)},
                                                                              {34, typeof (TaskTypeDto)},
                                                                              {35, typeof (TaskDto)},
                                                                              {36, typeof (ContentType2ContentTypeDto)},
                                                                              {
                                                                                  37,
                                                                                  typeof (ContentTypeAllowedContentTypeDto)
                                                                              },
                                                                              {38, typeof (User2AppDto)},
                                                                              {39, typeof (User2NodeNotifyDto)},
                                                                              {40, typeof (User2NodePermissionDto)}
                                                                          };
        #endregion

        public DatabaseSchemaCreation(Database database)
        {
            _database = database;
        }

        /// <summary>
        /// Initialize the database by creating the umbraco db schema
        /// </summary>
        public void InitializeDatabaseSchema()
        {
            var e = new DatabaseCreationEventArgs();
            FireBeforeCreation(e);

            if (!e.Cancel)
            {
                foreach (var item in OrderedTables.OrderBy(x => x.Key))
                {
                    _database.CreateTable(false, item.Value);
                }
            }

            FireAfterCreation(e);
        }

        /// <summary>
        /// Validates the schema of the current database
        /// </summary>
        public DatabaseSchemaResult ValidateSchema()
        {
            var result = new DatabaseSchemaResult();

            foreach (var item in OrderedTables.OrderBy(x => x.Key))
            {
                var tableNameAttribute = item.Value.FirstAttribute<TableNameAttribute>();
                if (tableNameAttribute != null)
                {
                    var tableExist = _database.TableExist(tableNameAttribute.Value);
                    if (tableExist)
                    {
                        result.Successes.Add(tableNameAttribute.Value, "Table exists");
                    }
                    else
                    {
                        result.Errors.Add(tableNameAttribute.Value, "Table does not exist");
                    }
                }
            }

            return result;
        }

        #region Events

        /// <summary>
        /// The save event handler
        /// </summary>
        internal delegate void DatabaseEventHandler(DatabaseCreationEventArgs e);

        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        internal static event DatabaseEventHandler BeforeCreation;
        /// <summary>
        /// Raises the <see cref="BeforeCreation"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected internal virtual void FireBeforeCreation(DatabaseCreationEventArgs e)
        {
            if (BeforeCreation != null)
            {
                BeforeCreation(e);
            }
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        internal static event DatabaseEventHandler AfterCreation;
        /// <summary>
        /// Raises the <see cref="AfterCreation"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterCreation(DatabaseCreationEventArgs e)
        {
            if (AfterCreation != null)
            {
                AfterCreation(e);
            }
        }

        #endregion
    }
}