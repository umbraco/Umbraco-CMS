using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Initial
{
    /// <summary>
    /// Represents the initial database creation by running CreateTable for all DTOs against the db.
    /// </summary>
    internal class DatabaseCreation
    {
        private readonly Database _database;

        public DatabaseCreation(Database database)
        {
            _database = database;
        }

        public void InitializeDatabase()
        {
            //NOTE Please mind the order of the table creation, as some references requires other tables.
            using(var transaction = _database.GetTransaction())
            {
                _database.CreateTable<NodeDto>();

                _database.CreateTable<TemplateDto>();

                _database.CreateTable<ContentDto>();
                _database.CreateTable<ContentVersionDto>();
                _database.CreateTable<DocumentDto>();

                _database.CreateTable<ContentTypeDto>();
                _database.CreateTable<DocumentTypeDto>();

                _database.CreateTable<AppDto>();
                _database.CreateTable<AppTreeDto>();
                
                _database.CreateTable<DataTypeDto>();
                _database.CreateTable<DataTypePreValueDto>();

                _database.CreateTable<DictionaryDto>();
                _database.CreateTable<LanguageTextDto>();

                _database.CreateTable<LanguageDto>();
                _database.CreateTable<DomainDto>();

                _database.CreateTable<LogDto>();

                _database.CreateTable<MacroDto>();
                _database.CreateTable<MacroPropertyTypeDto>();
                _database.CreateTable<MacroPropertyDto>();
                
                _database.CreateTable<MemberTypeDto>();
                _database.CreateTable<MemberDto>();
                _database.CreateTable<Member2MemberGroupDto>();

                _database.CreateTable<ContentXmlDto>();
                _database.CreateTable<PreviewXmlDto>();
                
                _database.CreateTable<PropertyTypeGroupDto>();
                _database.CreateTable<PropertyTypeDto>();
                _database.CreateTable<PropertyDataDto>();

                _database.CreateTable<RelationTypeDto>();
                _database.CreateTable<RelationDto>();
                
                _database.CreateTable<StylesheetDto>();
                _database.CreateTable<StylesheetPropertyDto>();
                
                _database.CreateTable<TagDto>();
                _database.CreateTable<TagRelationshipDto>();

                _database.CreateTable<UserLoginDto>();
                _database.CreateTable<UserTypeDto>();
                _database.CreateTable<UserDto>();

                _database.CreateTable<TaskTypeDto>();
                _database.CreateTable<TaskDto>();

                _database.CreateTable<ContentType2ContentTypeDto>();
                _database.CreateTable<ContentTypeAllowedContentTypeDto>();

                _database.CreateTable<User2AppDto>();
                _database.CreateTable<User2NodeNotifyDto>();
                _database.CreateTable<User2NodePermissionDto>();
                
                transaction.Complete();
            }
        }
    }
}