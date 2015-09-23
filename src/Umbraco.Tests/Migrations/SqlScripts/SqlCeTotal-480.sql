CREATE TABLE [umbracoRelation] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[parentId] [int] NOT NULL, 
[childId] [int] NOT NULL, 
[relType] [int] NOT NULL, 
[datetime] [datetime] NOT NULL CONSTRAINT [DF_umbracoRelation_datetime] DEFAULT (getdate()), 
[comment] [nvarchar] (1000)  NOT NULL 
) 
 
; 
ALTER TABLE [umbracoRelation] ADD CONSTRAINT [PK_umbracoRelation] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [cmsDocument] 
( 
[nodeId] [int] NOT NULL, 
[published] [bit] NOT NULL, 
[documentUser] [int] NOT NULL, 
[versionId] [uniqueidentifier] NOT NULL, 
[text] [nvarchar] (255) NOT NULL, 
[releaseDate] [datetime] NULL, 
[expireDate] [datetime] NULL, 
[updateDate] [datetime] NOT NULL CONSTRAINT [DF_cmsDocument_updateDate] DEFAULT (getdate()), 
[templateId] [int] NULL, 
[alias] [nvarchar] (255)  NULL ,
[newest] [bit] NOT NULL CONSTRAINT [DF_cmsDocument_newest] DEFAULT (0)
) 
 
; 
ALTER TABLE [cmsDocument] ADD CONSTRAINT [PK_cmsDocument] PRIMARY KEY  ([versionId]) 
; 
CREATE TABLE [umbracoLog] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[userId] [int] NOT NULL, 
[NodeId] [int] NOT NULL, 
[Datestamp] [datetime] NOT NULL CONSTRAINT [DF_umbracoLog_Datestamp] DEFAULT (getdate()), 
[logHeader] [nvarchar] (50) NOT NULL, 
[logComment] [nvarchar] (1000)  NULL 
) 
 
; 
ALTER TABLE [umbracoLog] ADD CONSTRAINT [PK_umbracoLog] PRIMARY KEY  ([id]) 
; 

CREATE TABLE [umbracoApp] 
( 
[sortOrder] [tinyint] NOT NULL CONSTRAINT [DF_app_sortOrder] DEFAULT (0), 
[appAlias] [nvarchar] (50) NOT NULL, 
[appIcon] [nvarchar] (255) NOT NULL, 
[appName] [nvarchar] (255) NOT NULL, 
[appInitWithTreeAlias] [nvarchar] (255) NULL 
) 
 
; 
ALTER TABLE [umbracoApp] ADD CONSTRAINT [PK_umbracoApp] PRIMARY KEY  ([appAlias]) 
; 
CREATE TABLE [cmsPropertyData] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[contentNodeId] [int] NOT NULL, 
[versionId] [uniqueidentifier] NULL, 
[propertytypeid] [int] NOT NULL, 
[dataInt] [int] NULL, 
[dataDate] [datetime] NULL, 
[dataNvarchar] [nvarchar] (500) NULL, 
[dataNtext] [ntext] NULL 
) 
 
; 
ALTER TABLE [cmsPropertyData] ADD CONSTRAINT [PK_cmsPropertyData] PRIMARY KEY  ([id]) 
; 
CREATE INDEX [IX_cmsPropertyData] ON [cmsPropertyData] ([id]) 
; 
CREATE TABLE [cmsContent] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[contentType] [int] NOT NULL 
) 
 
; 
ALTER TABLE [cmsContent] ADD CONSTRAINT [PK_cmsContent] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsContentType] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[alias] [nvarchar] (255) NULL, 
[icon] [nvarchar] (255) NULL 
) 
 
; 
ALTER TABLE [cmsContentType] ADD CONSTRAINT [PK_cmsContentType] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsMacroPropertyType] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[macroPropertyTypeAlias] [nvarchar] (50) NULL, 
[macroPropertyTypeRenderAssembly] [nvarchar] (255) NULL, 
[macroPropertyTypeRenderType] [nvarchar] (255) NULL, 
[macroPropertyTypeBaseType] [nvarchar] (255) NULL 
) 
 
; 
ALTER TABLE [cmsMacroPropertyType] ADD CONSTRAINT [PK_macroPropertyType] PRIMARY KEY  ([id]) 
; 


CREATE TABLE [cmsTab] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[contenttypeNodeId] [int] NOT NULL, 
[text] [nvarchar] (255) NOT NULL, 
[sortorder] [int] NOT NULL 
) 
 
; 
ALTER TABLE [cmsTab] ADD CONSTRAINT [PK_cmsTab] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [cmsTemplate] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[master] [int] NULL, 
[alias] [nvarchar] (100) NULL, 
[design] [ntext]  NOT NULL 
) 
 
; 
ALTER TABLE [cmsTemplate] ADD CONSTRAINT [PK_templates] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [umbracoUser2app] 
( 
[user] [int] NOT NULL, 
[app] [nvarchar] (50) NOT NULL 
) 
 
; 
ALTER TABLE [umbracoUser2app] ADD CONSTRAINT [PK_user2app] PRIMARY KEY  ([user], [app]) 
; 
CREATE TABLE [umbracoUserType] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[userTypeAlias] [nvarchar] (50) NULL, 
[userTypeName] [nvarchar] (255) NOT NULL, 
[userTypeDefaultPermissions] [nvarchar] (50) NULL 
) 
 
; 
ALTER TABLE [umbracoUserType] ADD CONSTRAINT [PK_userType] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [umbracoUser] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[userDisabled] [bit] NOT NULL CONSTRAINT [DF_umbracoUser_userDisabled] DEFAULT (0), 
[userNoConsole] [bit] NOT NULL CONSTRAINT [DF_umbracoUser_userNoConsole] DEFAULT (0), 
[userType] [int] NOT NULL, 
[startStructureID] [int] NOT NULL, 
[startMediaID] [int] NULL, 
[userName] [nvarchar] (255) NOT NULL, 
[userLogin] [nvarchar] (125) NOT NULL, 
[userPassword] [nvarchar] (125) NOT NULL, 
[userEmail] [nvarchar] (255) NOT NULL, 
[userDefaultPermissions] [nvarchar] (50) NULL, 
[userLanguage] [nvarchar] (10) NULL 
) 
 
; 
ALTER TABLE [umbracoUser] ADD CONSTRAINT [PK_user] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [cmsDocumentType] 
( 
[contentTypeNodeId] [int] NOT NULL, 
[templateNodeId] [int] NOT NULL, 
[IsDefault] [bit] NOT NULL CONSTRAINT [DF_cmsDocumentType_IsDefault] DEFAULT (0) 
) 
 
; 
ALTER TABLE [cmsDocumentType] ADD CONSTRAINT [PK_cmsDocumentType] PRIMARY KEY  ([contentTypeNodeId], [templateNodeId]) 
; 
CREATE TABLE [cmsMemberType] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[NodeId] [int] NOT NULL, 
[propertytypeId] [int] NOT NULL, 
[memberCanEdit] [bit] NOT NULL CONSTRAINT [DF_cmsMemberType_memberCanEdit] DEFAULT (0), 
[viewOnProfile] [bit] NOT NULL CONSTRAINT [DF_cmsMemberType_viewOnProfile] DEFAULT (0) 
) 
 
; 
ALTER TABLE [cmsMemberType] ADD CONSTRAINT [PK_cmsMemberType] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsMember] 
( 
[nodeId] [int] NOT NULL, 
[Email] [nvarchar] (1000) NOT NULL CONSTRAINT [DF_cmsMember_Email] DEFAULT (''), 
[LoginName] [nvarchar] (1000) NOT NULL CONSTRAINT [DF_cmsMember_LoginName] DEFAULT (''), 
[Password] [nvarchar] (1000) NOT NULL CONSTRAINT [DF_cmsMember_Password] DEFAULT ('') 
) 
 
; 
CREATE TABLE [umbracoNode] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[trashed] [bit] NOT NULL CONSTRAINT [DF_umbracoNode_trashed] DEFAULT (0), 
[parentID] [int] NOT NULL, 
[nodeUser] [int] NULL, 
[level] [int] NOT NULL, 
[path] [nvarchar] (150)  NOT NULL, 
[sortOrder] [int] NOT NULL, 
[uniqueID] [uniqueidentifier] NULL, 
[text] [nvarchar] (255) NULL, 
[nodeObjectType] [uniqueidentifier] NULL, 
[createDate] [datetime] NOT NULL CONSTRAINT [DF_umbracoNode_createDate] DEFAULT (getdate()) 
) 
 
; 
ALTER TABLE [umbracoNode] ADD CONSTRAINT [PK_structure] PRIMARY KEY  ([id]) 
; 
; 
CREATE TABLE [cmsPropertyType] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[dataTypeId] [int] NOT NULL, 
[contentTypeId] [int] NOT NULL, 
[tabId] [int] NULL, 
[Alias] [nvarchar] (255) NOT NULL, 
[Name] [nvarchar] (255) NULL, 
[helpText] [nvarchar] (1000) NULL, 
[sortOrder] [int] NOT NULL CONSTRAINT [DF__cmsProper__sortO__1EA48E88] DEFAULT (0), 
[mandatory] [bit] NOT NULL CONSTRAINT [DF__cmsProper__manda__2180FB33] DEFAULT (0), 
[validationRegExp] [nvarchar] (255)  NULL, 
[Description] [nvarchar] (2000)  NULL 
) 
 
; 
ALTER TABLE [cmsPropertyType] ADD CONSTRAINT [PK_cmsPropertyType] PRIMARY KEY  ([id]) 
; 

CREATE TABLE [cmsMacroProperty] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[macroPropertyHidden] [bit] NOT NULL CONSTRAINT [DF_macroProperty_macroPropertyHidden] DEFAULT (0), 
[macroPropertyType] [int] NOT NULL, 
[macro] [int] NOT NULL, 
[macroPropertySortOrder] [tinyint] NOT NULL CONSTRAINT [DF_macroProperty_macroPropertySortOrder] DEFAULT (0), 
[macroPropertyAlias] [nvarchar] (50) NOT NULL, 
[macroPropertyName] [nvarchar] (255) NOT NULL 
) 
 
; 
ALTER TABLE [cmsMacroProperty] ADD CONSTRAINT [PK_macroProperty] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [cmsMacro] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[macroUseInEditor] [bit] NOT NULL CONSTRAINT [DF_macro_macroUseInEditor] DEFAULT (0), 
[macroRefreshRate] [int] NOT NULL CONSTRAINT [DF_macro_macroRefreshRate] DEFAULT (0), 
[macroAlias] [nvarchar] (255) NOT NULL, 
[macroName] [nvarchar] (255) NULL, 
[macroScriptType] [nvarchar] (255) NULL, 
[macroScriptAssembly] [nvarchar] (255) NULL, 
[macroXSLT] [nvarchar] (255) NULL, 
[macroCacheByPage] [bit] NOT NULL CONSTRAINT [DF_cmsMacro_macroCacheByPage] DEFAULT (1), 
[macroCachePersonalized] [bit] NOT NULL CONSTRAINT [DF_cmsMacro_macroCachePersonalized] DEFAULT (0), 
[macroDontRender] [bit] NOT NULL CONSTRAINT [DF_cmsMacro_macroDontRender] DEFAULT (0) 
) 
 
; 
ALTER TABLE [cmsMacro] ADD CONSTRAINT [PK_macro] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [cmsContentVersion] 
( 
[id] [int] NOT NULL IDENTITY(1, 1) PRIMARY KEY, 
[ContentId] [int] NOT NULL, 
[VersionId] [uniqueidentifier] NOT NULL, 
[VersionDate] [datetime] NOT NULL CONSTRAINT [DF_cmsContentVersion_VersionDate] DEFAULT (getdate()) 
) 
 
; 
CREATE TABLE [umbracoAppTree] 
( 
[treeSilent] [bit] NOT NULL CONSTRAINT [DF_umbracoAppTree_treeSilent] DEFAULT (0), 
[treeInitialize] [bit] NOT NULL CONSTRAINT [DF_umbracoAppTree_treeInitialize] DEFAULT (1), 
[treeSortOrder] [tinyint] NOT NULL, 
[appAlias] [nvarchar] (50) NOT NULL, 
[treeAlias] [nvarchar] (150) NOT NULL, 
[treeTitle] [nvarchar] (255) NOT NULL, 
[treeIconClosed] [nvarchar] (255) NOT NULL, 
[treeIconOpen] [nvarchar] (255) NOT NULL, 
[treeHandlerAssembly] [nvarchar] (255) NOT NULL, 
[treeHandlerType] [nvarchar] (255) NOT NULL,
[action] [nvarchar] (255) NULL  
) 
 
; 
ALTER TABLE [umbracoAppTree] ADD CONSTRAINT [PK_umbracoAppTree] PRIMARY KEY  ([appAlias], [treeAlias]) 
; 

CREATE TABLE [cmsContentTypeAllowedContentType] 
( 
[Id] [int] NOT NULL, 
[AllowedId] [int] NOT NULL 
) 
 
; 
ALTER TABLE [cmsContentTypeAllowedContentType] ADD CONSTRAINT [PK_cmsContentTypeAllowedContentType] PRIMARY KEY  ([Id], [AllowedId]) 
; 
CREATE TABLE [cmsContentXml] 
( 
[nodeId] [int] NOT NULL, 
[xml] [ntext]  NOT NULL 
) 
 
; 
ALTER TABLE [cmsContentXml] ADD CONSTRAINT [PK_cmsContentXml] PRIMARY KEY  ([nodeId]) 
; 
CREATE TABLE [cmsDataType] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[controlId] [uniqueidentifier] NOT NULL, 
[dbType] [nvarchar] (50) NOT NULL 
) 
 
; 
ALTER TABLE [cmsDataType] ADD CONSTRAINT [PK_cmsDataType] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsDataTypePreValues] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[datatypeNodeId] [int] NOT NULL, 
[value] [nvarchar] (255) NULL, 
[sortorder] [int] NOT NULL, 
[alias] [nvarchar] (50)  NULL 
) 
 
; 
ALTER TABLE [cmsDataTypePreValues] ADD CONSTRAINT [PK_cmsDataTypePreValues] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [cmsDictionary] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[id] [uniqueidentifier] NOT NULL, 
[parent] [uniqueidentifier] NOT NULL, 
[key] [nvarchar] (1000)  NOT NULL 
) 
 
; 
ALTER TABLE [cmsDictionary] ADD CONSTRAINT [PK_cmsDictionary] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsLanguageText] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[languageId] [int] NOT NULL, 
[UniqueId] [uniqueidentifier] NOT NULL, 
[value] [nvarchar] (1000)  NOT NULL 
) 
 
; 
ALTER TABLE [cmsLanguageText] ADD CONSTRAINT [PK_cmsLanguageText] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsMember2MemberGroup] 
( 
[Member] [int] NOT NULL, 
[MemberGroup] [int] NOT NULL 
) 
 
; 
ALTER TABLE [cmsMember2MemberGroup] ADD CONSTRAINT [PK_cmsMember2MemberGroup] PRIMARY KEY  ([Member], [MemberGroup]) 
; 
CREATE TABLE [cmsStylesheet] 
( 
[nodeId] [int] NOT NULL, 
[filename] [nvarchar] (100) NOT NULL, 
[content] [ntext] NULL 
) 
 
; 
CREATE TABLE [cmsStylesheetProperty] 
( 
[nodeId] [int] NOT NULL, 
[stylesheetPropertyEditor] [bit] NULL, 
[stylesheetPropertyAlias] [nvarchar] (50) NULL, 
[stylesheetPropertyValue] [nvarchar] (400) NULL 
) 
 
; 
CREATE TABLE [umbracoDomains] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[domainDefaultLanguage] [int] NULL, 
[domainRootStructureID] [int] NULL, 
[domainName] [nvarchar] (255) NOT NULL 
) 
 
; 
ALTER TABLE [umbracoDomains] ADD CONSTRAINT [PK_domains] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [umbracoLanguage] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[languageISOCode] [nvarchar] (10) NULL, 
[languageCultureName] [nvarchar] (100) NULL 
) 
 
; 
ALTER TABLE [umbracoLanguage] ADD CONSTRAINT [PK_language] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [umbracoRelationType] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[dual] [bit] NOT NULL, 
[parentObjectType] [uniqueidentifier] NOT NULL, 
[childObjectType] [uniqueidentifier] NOT NULL, 
[name] [nvarchar] (255)  NOT NULL, 
[alias] [nvarchar] (100)  NULL 
) 
 
; 
ALTER TABLE [umbracoRelationType] ADD CONSTRAINT [PK_umbracoRelationType] PRIMARY KEY  ([id]) 
; 


CREATE TABLE [umbracoUser2NodeNotify] 
( 
[userId] [int] NOT NULL, 
[nodeId] [int] NOT NULL, 
[action] [nchar] (1)  NOT NULL 
) 
 
; 
ALTER TABLE [umbracoUser2NodeNotify] ADD CONSTRAINT [PK_umbracoUser2NodeNotify] PRIMARY KEY  ([userId], [nodeId], [action]) 
; 
CREATE TABLE [umbracoUser2NodePermission] 
( 
[userId] [int] NOT NULL, 
[nodeId] [int] NOT NULL, 
[permission] [nchar] (1) NOT NULL 
) 
 
; 
ALTER TABLE [umbracoUser2NodePermission] ADD CONSTRAINT [PK_umbracoUser2NodePermission] PRIMARY KEY  ([userId], [nodeId], [permission]) 
; 
 
ALTER TABLE [umbracoAppTree] ADD 
CONSTRAINT [FK_umbracoAppTree_umbracoApp] FOREIGN KEY ([appAlias]) REFERENCES [umbracoApp] ([appAlias]) 
; 
ALTER TABLE [cmsPropertyData] ADD 
CONSTRAINT [FK_cmsPropertyData_umbracoNode] FOREIGN KEY ([contentNodeId]) REFERENCES [umbracoNode] ([id]) 
; 


ALTER TABLE [cmsDocument] ADD 
CONSTRAINT [FK_cmsDocument_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
; 
ALTER TABLE [cmsMacroProperty] ADD 
CONSTRAINT [FK_umbracoMacroProperty_umbracoMacroPropertyType] FOREIGN KEY ([macroPropertyType]) REFERENCES [cmsMacroPropertyType] ([id]) 
; 
ALTER TABLE [umbracoUser] ADD 
CONSTRAINT [FK_user_userType] FOREIGN KEY ([userType]) REFERENCES [umbracoUserType] ([id]) 
; 
ALTER TABLE [umbracoNode] ADD 
CONSTRAINT [FK_umbracoNode_umbracoNode] FOREIGN KEY ([parentID]) REFERENCES [umbracoNode] ([id]) 
; 
ALTER TABLE [cmsTemplate] ADD 
CONSTRAINT [FK_cmsTemplate_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
; 
ALTER TABLE [cmsContentType] ADD 
CONSTRAINT [FK_cmsContentType_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
; 
ALTER TABLE [cmsPropertyType] ADD 
CONSTRAINT [FK_cmsPropertyType_cmsTab] FOREIGN KEY ([tabId]) REFERENCES [cmsTab] ([id]) 
; 
ALTER TABLE [cmsContent] ADD 
CONSTRAINT [FK_cmsContent_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
; 
ALTER TABLE [umbracoUser2app] ADD 
CONSTRAINT [FK_umbracoUser2app_umbracoApp] FOREIGN KEY ([app]) REFERENCES [umbracoApp] ([appAlias]), 
CONSTRAINT [FK_umbracoUser2app_umbracoUser] FOREIGN KEY ([user]) REFERENCES [umbracoUser] ([id]) 
; 
 
ALTER TABLE [cmsTemplate] DROP CONSTRAINT [FK_cmsTemplate_umbracoNode] 
; 
ALTER TABLE [cmsPropertyType] DROP CONSTRAINT [FK_cmsPropertyType_cmsTab] 
; 
ALTER TABLE [cmsContent] DROP CONSTRAINT [FK_cmsContent_umbracoNode] 
; 
ALTER TABLE [cmsMacroProperty] DROP CONSTRAINT [FK_umbracoMacroProperty_umbracoMacroPropertyType] 
; 
ALTER TABLE [umbracoAppTree] DROP CONSTRAINT [FK_umbracoAppTree_umbracoApp] 
; 
ALTER TABLE [umbracoUser2app] DROP CONSTRAINT [FK_umbracoUser2app_umbracoApp] 
; 
ALTER TABLE [umbracoUser2app] DROP CONSTRAINT [FK_umbracoUser2app_umbracoUser] 
; 
ALTER TABLE [cmsPropertyData] DROP CONSTRAINT [FK_cmsPropertyData_umbracoNode] 
; 

ALTER TABLE [umbracoUser] DROP CONSTRAINT [FK_user_userType] 
; 
ALTER TABLE [cmsContentType] DROP CONSTRAINT [FK_cmsContentType_umbracoNode] 
; 
ALTER TABLE [cmsDocument] DROP CONSTRAINT [FK_cmsDocument_umbracoNode] 
; 
ALTER TABLE [umbracoNode] DROP CONSTRAINT [FK_umbracoNode_umbracoNode] 
; 

ALTER TABLE [cmsTemplate] ADD CONSTRAINT [FK_cmsTemplate_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
;
ALTER TABLE [cmsPropertyType] ADD CONSTRAINT [FK_cmsPropertyType_cmsTab] FOREIGN KEY ([tabId]) REFERENCES [cmsTab] ([id]) 
;
ALTER TABLE [cmsContent] ADD CONSTRAINT [FK_cmsContent_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
;
ALTER TABLE [cmsMacroProperty] ADD CONSTRAINT [FK_umbracoMacroProperty_umbracoMacroPropertyType] FOREIGN KEY ([macroPropertyType]) REFERENCES [cmsMacroPropertyType] ([id]) 
;
ALTER TABLE [umbracoAppTree] ADD CONSTRAINT [FK_umbracoAppTree_umbracoApp] FOREIGN KEY ([appAlias]) REFERENCES [umbracoApp] ([appAlias]) 
;
ALTER TABLE [umbracoUser2app] ADD CONSTRAINT [FK_umbracoUser2app_umbracoApp] FOREIGN KEY ([app]) REFERENCES [umbracoApp] ([appAlias]) 
;
ALTER TABLE [umbracoUser2app] ADD CONSTRAINT [FK_umbracoUser2app_umbracoUser] FOREIGN KEY ([user]) REFERENCES [umbracoUser] ([id]) 
;
ALTER TABLE [cmsPropertyData] ADD CONSTRAINT [FK_cmsPropertyData_umbracoNode] FOREIGN KEY ([contentNodeId]) REFERENCES [umbracoNode] ([id]) 
;
ALTER TABLE [umbracoUser] ADD CONSTRAINT [FK_user_userType] FOREIGN KEY ([userType]) REFERENCES [umbracoUserType] ([id]) 
;
ALTER TABLE [cmsContentType] ADD CONSTRAINT [FK_cmsContentType_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
;
ALTER TABLE [cmsDocument] ADD CONSTRAINT [FK_cmsDocument_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
;
ALTER TABLE [umbracoNode] ADD CONSTRAINT [FK_umbracoNode_umbracoNode] FOREIGN KEY ([parentID]) REFERENCES [umbracoNode] ([id]) 
;

ALTER TABLE cmsDataTypePreValues ALTER COLUMN value NVARCHAR(2500) NULL
;
CREATE TABLE [cmsTask]
(
[closed] [bit] NOT NULL CONSTRAINT [DF__cmsTask__closed__04E4BC85] DEFAULT ((0)),
[id] [int] NOT NULL IDENTITY(1, 1),
[taskTypeId] [int] NOT NULL,
[nodeId] [int] NOT NULL,
[parentUserId] [int] NOT NULL,
[userId] [int] NOT NULL,
[DateTime] [datetime] NOT NULL CONSTRAINT [DF__cmsTask__DateTim__05D8E0BE] DEFAULT (getdate()),
[Comment] [nvarchar] (500) NULL
)
;
ALTER TABLE [cmsTask] ADD CONSTRAINT [PK_cmsTask] PRIMARY KEY  ([id])
;
CREATE TABLE [cmsTaskType]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[alias] [nvarchar] (255) NOT NULL
)
;
ALTER TABLE [cmsTaskType] ADD CONSTRAINT [PK_cmsTaskType] PRIMARY KEY  ([id])
;
ALTER TABLE [cmsTask] ADD
CONSTRAINT [FK_cmsTask_cmsTaskType] FOREIGN KEY ([taskTypeId]) REFERENCES [cmsTaskType] ([id])
;
insert into cmsTaskType (alias) values ('toTranslate')
;

update umbracoUserType set userTypeDefaultPermissions = userTypeDefaultPermissions + '5' where userTypeAlias in ('editor','admin')
;

;
insert into umbracoRelationType (dual, parentObjectType, childObjectType, name, alias) values (1, 'c66ba18e-eaf3-4cff-8a22-41b16d66a972', 'c66ba18e-eaf3-4cff-8a22-41b16d66a972', 'Relate Document On Copy','relateDocumentOnCopy')
;
ALTER TABLE cmsMacro ADD macroPython nvarchar(255)
;


UPDATE umbracoUserType SET userTypeDefaultPermissions = userTypeDefaultPermissions + 'F' WHERE CHARINDEX('A',userTypeDefaultPermissions,0) >= 1
AND CHARINDEX('F',userTypeDefaultPermissions,0) < 1
;

UPDATE umbracoUserType SET userTypeDefaultPermissions = userTypeDefaultPermissions + 'H' WHERE userTypeAlias = 'writer'
AND CHARINDEX('F',userTypeDefaultPermissions,0) < 1
;

INSERT INTO umbracoUser2NodePermission (userID, nodeId, permission) 
SELECT userID, nodeId, 'F' FROM umbracoUser2NodePermission WHERE permission='A'
;

INSERT INTO umbracoUser2NodePermission (userID, nodeId, permission) 
SELECT DISTINCT userID, nodeId, 'H' FROM umbracoUser2NodePermission WHERE userId IN
(SELECT umbracoUser.id FROM umbracoUserType INNER JOIN umbracoUser ON umbracoUserType.id = umbracoUser.userType WHERE (umbracoUserType.userTypeAlias = 'writer'))
;

alter TABLE [cmsContentType]
add [masterContentType] int NULL CONSTRAINT
[DF_cmsContentType_masterContentType] DEFAULT (0)
;
CREATE TABLE [cmsTagRelationship](
	[nodeId] [int] NOT NULL,
	[tagId] [int] NOT NULL)
;

CREATE TABLE [cmsTags](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[tag] [nvarchar](200) NULL,
	[parentId] [int] NULL,
	[group] [nvarchar](100) NULL)
;
alter TABLE [umbracoUser]
add [defaultToLiveEditing] bit NOT NULL CONSTRAINT
[DF_umbracoUser_defaultToLiveEditing] DEFAULT (0)
;

CREATE TABLE [cmsPreviewXml](
	[nodeId] [int] NOT NULL,
	[versionId] [uniqueidentifier] NOT NULL,
	[timestamp] [datetime] NOT NULL,
	[xml] [ntext] NOT NULL)
;


ALTER TABLE [umbracoNode] ALTER COLUMN id IDENTITY(1042,1)
;
ALTER TABLE [cmsContentType] ALTER COLUMN pk IDENTITY(535,1)
;
ALTER TABLE [umbracoUser] ALTER COLUMN id IDENTITY(1,1)
;
ALTER TABLE [umbracoUserType] ALTER COLUMN id IDENTITY(5,1)
;
ALTER TABLE [cmsMacroPropertyType] ALTER COLUMN id IDENTITY(26,1)
;
ALTER TABLE [cmsTab] ALTER COLUMN id IDENTITY(6,1)
;
ALTER TABLE [cmsPropertyType] ALTER COLUMN id IDENTITY(28,1)
;
ALTER TABLE [umbracoLanguage] ALTER COLUMN id IDENTITY(2,1)
;
ALTER TABLE [cmsDataType] ALTER COLUMN pk IDENTITY(39,1)
;
ALTER TABLE [cmsDataTypePreValues] ALTER COLUMN id IDENTITY(5,1)