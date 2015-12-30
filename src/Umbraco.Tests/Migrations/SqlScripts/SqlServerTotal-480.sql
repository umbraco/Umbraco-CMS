/*******************************************************************************************







    Umbraco database installation script for SQL Server
 
IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
 
    Database version: 4.8.0.0
    
    Please increment this version number if ANY change is made to this script,
    so compatibility with scripts for other database systems can be verified easily.
    The first 3 digits depict the Umbraco version, the last digit is the database version.
    (e.g. version 4.0.0.3 means "Umbraco version 4.0.0, database version 3")
    
    Check-in policy: only commit this script if
     * you ran the Umbraco installer completely;
     * you ran it on the targetted database system;
     * you ran the Runway and Module installations;
     * you were able to browse the Boost site;
     * you were able to open the Umbraco administration panel;
     * you have documented the code change in this script;
     * you have incremented the version number in this script.
 
IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT







********************************************************************************************/

CREATE TABLE [umbracoRelation] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[parentId] [int] NOT NULL, 
[childId] [int] NOT NULL, 
[relType] [int] NOT NULL, 
[datetime] [datetime] NOT NULL CONSTRAINT [DF_umbracoRelation_datetime] DEFAULT (getdate()), 
[comment] [nvarchar] (1000) COLLATE Danish_Norwegian_CI_AS NOT NULL 
) 
 
; 
ALTER TABLE [umbracoRelation] ADD CONSTRAINT [PK_umbracoRelation] PRIMARY KEY CLUSTERED  ([id]) 
; 
CREATE TABLE [cmsDocument] 
( 
[nodeId] [int] NOT NULL, 
[published] [bit] NOT NULL, 
[documentUser] [int] NOT NULL, 
[versionId] [uniqueidentifier] NOT NULL, 
[text] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[releaseDate] [datetime] NULL, 
[expireDate] [datetime] NULL, 
[updateDate] [datetime] NOT NULL CONSTRAINT [DF_cmsDocument_updateDate] DEFAULT (getdate()), 
[templateId] [int] NULL, 
[alias] [nvarchar] (255) COLLATE Danish_Norwegian_CI_AS NULL ,
[newest] [bit] NOT NULL CONSTRAINT [DF_cmsDocument_newest] DEFAULT (0)
) 
 
; 
ALTER TABLE [cmsDocument] ADD CONSTRAINT [PK_cmsDocument] PRIMARY KEY CLUSTERED  ([versionId]) 
; 
CREATE TABLE [umbracoLog] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[userId] [int] NOT NULL, 
[NodeId] [int] NOT NULL, 
[Datestamp] [datetime] NOT NULL CONSTRAINT [DF_umbracoLog_Datestamp] DEFAULT (getdate()), 
[logHeader] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[logComment] [nvarchar] (1000) COLLATE Danish_Norwegian_CI_AS NULL 
) 
 
; 
ALTER TABLE [umbracoLog] ADD CONSTRAINT [PK_umbracoLog] PRIMARY KEY CLUSTERED  ([id]) 
; 

/* TABLES ARE NEVER USED, REMOVED FOR 4.1

CREATE TABLE [umbracoUserGroup] 
( 
[id] [smallint] NOT NULL IDENTITY(1, 1), 
[userGroupName] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) 
; 
ALTER TABLE [umbracoUserGroup] ADD CONSTRAINT [PK_userGroup] PRIMARY KEY CLUSTERED  ([id]) 
; 
CREATE TABLE [umbracoUser2userGroup] 
( 
[user] [int] NOT NULL, 
[userGroup] [smallint] NOT NULL 
)  
; 
ALTER TABLE [umbracoUser2userGroup] ADD CONSTRAINT [PK_user2userGroup] PRIMARY KEY CLUSTERED  ([user], [userGroup]) 
; 

*/

CREATE TABLE [umbracoApp] 
( 
[sortOrder] [tinyint] NOT NULL CONSTRAINT [DF_app_sortOrder] DEFAULT (0), 
[appAlias] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[appIcon] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[appName] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[appInitWithTreeAlias] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) 
 
; 
ALTER TABLE [umbracoApp] ADD CONSTRAINT [PK_umbracoApp] PRIMARY KEY CLUSTERED  ([appAlias]) 
; 
CREATE TABLE [cmsPropertyData] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[contentNodeId] [int] NOT NULL, 
[versionId] [uniqueidentifier] NULL, 
[propertytypeid] [int] NOT NULL, 
[dataInt] [int] NULL, 
[dataDate] [datetime] NULL, 
[dataNvarchar] [nvarchar] (500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[dataNtext] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) 
 
; 
ALTER TABLE [cmsPropertyData] ADD CONSTRAINT [PK_cmsPropertyData] PRIMARY KEY CLUSTERED  ([id]) 
; 
CREATE NONCLUSTERED INDEX [IX_cmsPropertyData] ON [cmsPropertyData] ([id]) 
; 
CREATE NONCLUSTERED INDEX [IX_cmsPropertyData_1] ON [cmsPropertyData] ([contentNodeId]) 
; 
CREATE NONCLUSTERED INDEX [IX_cmsPropertyData_2] ON [cmsPropertyData] ([versionId]) 
; 
CREATE NONCLUSTERED INDEX [IX_cmsPropertyData_3] ON [cmsPropertyData] ([propertytypeid]) 
; 
CREATE TABLE [cmsContent] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[contentType] [int] NOT NULL 
) 
 
; 
ALTER TABLE [cmsContent] ADD CONSTRAINT [PK_cmsContent] PRIMARY KEY CLUSTERED  ([pk]) 
; 
CREATE TABLE [cmsContentType] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[alias] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[icon] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) 
 
; 
ALTER TABLE [cmsContentType] ADD CONSTRAINT [PK_cmsContentType] PRIMARY KEY CLUSTERED  ([pk]) 
; 
CREATE TABLE [cmsMacroPropertyType] 
( 
[id] [smallint] NOT NULL IDENTITY(1, 1), 
[macroPropertyTypeAlias] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[macroPropertyTypeRenderAssembly] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[macroPropertyTypeRenderType] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[macroPropertyTypeBaseType] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) 
 
; 
ALTER TABLE [cmsMacroPropertyType] ADD CONSTRAINT [PK_macroPropertyType] PRIMARY KEY CLUSTERED  ([id]) 
; 

/* TABLE IS NEVER USED, REMOVED FOR 4.1 

CREATE TABLE [umbracoStylesheetProperty] 
( 
[id] [smallint] NOT NULL IDENTITY(1, 1), 
[stylesheetPropertyEditor] [bit] NOT NULL CONSTRAINT [DF_stylesheetProperty_stylesheetPropertyEditor] DEFAULT (0), 
[stylesheet] [tinyint] NOT NULL, 
[stylesheetPropertyAlias] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[stylesheetPropertyName] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[stylesheetPropertyValue] [nvarchar] (400) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) 
; 

ALTER TABLE [umbracoStylesheetProperty] ADD CONSTRAINT [PK_stylesheetProperty] PRIMARY KEY CLUSTERED  ([id]) 
;

*/
 
CREATE TABLE [cmsTab] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[contenttypeNodeId] [int] NOT NULL, 
[text] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[sortorder] [int] NOT NULL 
) 
 
; 
ALTER TABLE [cmsTab] ADD CONSTRAINT [PK_cmsTab] PRIMARY KEY CLUSTERED  ([id]) 
; 
CREATE TABLE [cmsTemplate] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[master] [int] NULL, 
[alias] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[design] [ntext] COLLATE Danish_Norwegian_CI_AS NOT NULL 
) 
 
; 
ALTER TABLE [cmsTemplate] ADD CONSTRAINT [PK_templates] PRIMARY KEY CLUSTERED  ([pk]) 
; 
CREATE TABLE [umbracoUser2app] 
( 
[user] [int] NOT NULL, 
[app] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) 
 
; 
ALTER TABLE [umbracoUser2app] ADD CONSTRAINT [PK_user2app] PRIMARY KEY CLUSTERED  ([user], [app]) 
; 
CREATE TABLE [umbracoUserType] 
( 
[id] [smallint] NOT NULL IDENTITY(1, 1), 
[userTypeAlias] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[userTypeName] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[userTypeDefaultPermissions] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) 
 
; 
ALTER TABLE [umbracoUserType] ADD CONSTRAINT [PK_userType] PRIMARY KEY CLUSTERED  ([id]) 
; 
CREATE TABLE [umbracoUser] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[userDisabled] [bit] NOT NULL CONSTRAINT [DF_umbracoUser_userDisabled] DEFAULT (0), 
[userNoConsole] [bit] NOT NULL CONSTRAINT [DF_umbracoUser_userNoConsole] DEFAULT (0), 
[userType] [smallint] NOT NULL, 
[startStructureID] [int] NOT NULL, 
[startMediaID] [int] NULL, 
[userName] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[userLogin] [nvarchar] (125) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[userPassword] [nvarchar] (125) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[userEmail] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[userDefaultPermissions] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[userLanguage] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) 
 
; 
ALTER TABLE [umbracoUser] ADD CONSTRAINT [PK_user] PRIMARY KEY CLUSTERED  ([id]) 
; 
CREATE TABLE [cmsDocumentType] 
( 
[contentTypeNodeId] [int] NOT NULL, 
[templateNodeId] [int] NOT NULL, 
[IsDefault] [bit] NOT NULL CONSTRAINT [DF_cmsDocumentType_IsDefault] DEFAULT (0) 
) 
 
; 
ALTER TABLE [cmsDocumentType] ADD CONSTRAINT [PK_cmsDocumentType] PRIMARY KEY CLUSTERED  ([contentTypeNodeId], [templateNodeId]) 
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
ALTER TABLE [cmsMemberType] ADD CONSTRAINT [PK_cmsMemberType] PRIMARY KEY CLUSTERED  ([pk]) 
; 
CREATE TABLE [cmsMember] 
( 
[nodeId] [int] NOT NULL, 
[Email] [nvarchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_cmsMember_Email] DEFAULT (''), 
[LoginName] [nvarchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_cmsMember_LoginName] DEFAULT (''), 
[Password] [nvarchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_cmsMember_Password] DEFAULT ('') 
) 
 
; 
CREATE TABLE [umbracoNode] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[trashed] [bit] NOT NULL CONSTRAINT [DF_umbracoNode_trashed] DEFAULT (0), 
[parentID] [int] NOT NULL, 
[nodeUser] [int] NULL, 
[level] [smallint] NOT NULL, 
[path] [nvarchar] (150) COLLATE Danish_Norwegian_CI_AS NOT NULL, 
[sortOrder] [int] NOT NULL, 
[uniqueID] [uniqueidentifier] NULL, 
[text] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[nodeObjectType] [uniqueidentifier] NULL, 
[createDate] [datetime] NOT NULL CONSTRAINT [DF_umbracoNode_createDate] DEFAULT (getdate()) 
) 
 
; 
ALTER TABLE [umbracoNode] ADD CONSTRAINT [PK_structure] PRIMARY KEY CLUSTERED  ([id]) 
; 
CREATE NONCLUSTERED INDEX [IX_umbracoNodeParentId] ON [umbracoNode] ([parentID]) 
; 
CREATE NONCLUSTERED INDEX [IX_umbracoNodeObjectType] ON [umbracoNode] ([nodeObjectType]) 
; 
CREATE TABLE [cmsPropertyType] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[dataTypeId] [int] NOT NULL, 
[contentTypeId] [int] NOT NULL, 
[tabId] [int] NULL, 
[Alias] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[Name] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[helpText] [nvarchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[sortOrder] [int] NOT NULL CONSTRAINT [DF__cmsProper__sortO__1EA48E88] DEFAULT (0), 
[mandatory] [bit] NOT NULL CONSTRAINT [DF__cmsProper__manda__2180FB33] DEFAULT (0), 
[validationRegExp] [nvarchar] (255) COLLATE Danish_Norwegian_CI_AS NULL, 
[Description] [nvarchar] (2000) COLLATE Danish_Norwegian_CI_AS NULL 
) 
 
; 
ALTER TABLE [cmsPropertyType] ADD CONSTRAINT [PK_cmsPropertyType] PRIMARY KEY CLUSTERED  ([id]) 
; 

CREATE TABLE [cmsMacroProperty] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[macroPropertyHidden] [bit] NOT NULL CONSTRAINT [DF_macroProperty_macroPropertyHidden] DEFAULT (0), 
[macroPropertyType] [smallint] NOT NULL, 
[macro] [int] NOT NULL, 
[macroPropertySortOrder] [tinyint] NOT NULL CONSTRAINT [DF_macroProperty_macroPropertySortOrder] DEFAULT (0), 
[macroPropertyAlias] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[macroPropertyName] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) 
 
; 
ALTER TABLE [cmsMacroProperty] ADD CONSTRAINT [PK_macroProperty] PRIMARY KEY CLUSTERED  ([id]) 
; 
CREATE TABLE [cmsMacro] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[macroUseInEditor] [bit] NOT NULL CONSTRAINT [DF_macro_macroUseInEditor] DEFAULT (0), 
[macroRefreshRate] [int] NOT NULL CONSTRAINT [DF_macro_macroRefreshRate] DEFAULT (0), 
[macroAlias] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[macroName] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[macroScriptType] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[macroScriptAssembly] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[macroXSLT] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[macroCacheByPage] [bit] NOT NULL CONSTRAINT [DF_cmsMacro_macroCacheByPage] DEFAULT (1), 
[macroCachePersonalized] [bit] NOT NULL CONSTRAINT [DF_cmsMacro_macroCachePersonalized] DEFAULT (0), 
[macroDontRender] [bit] NOT NULL CONSTRAINT [DF_cmsMacro_macroDontRender] DEFAULT (0) 
) 
 
; 
ALTER TABLE [cmsMacro] ADD CONSTRAINT [PK_macro] PRIMARY KEY CLUSTERED  ([id]) 
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
[appAlias] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[treeAlias] [nvarchar] (150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[treeTitle] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[treeIconClosed] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[treeIconOpen] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[treeHandlerAssembly] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[treeHandlerType] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[action] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL  
) 
 
; 
ALTER TABLE [umbracoAppTree] ADD CONSTRAINT [PK_umbracoAppTree] PRIMARY KEY CLUSTERED  ([appAlias], [treeAlias]) 
; 

CREATE TABLE [cmsContentTypeAllowedContentType] 
( 
[Id] [int] NOT NULL, 
[AllowedId] [int] NOT NULL 
) 
 
; 
ALTER TABLE [cmsContentTypeAllowedContentType] ADD CONSTRAINT [PK_cmsContentTypeAllowedContentType] PRIMARY KEY CLUSTERED  ([Id], [AllowedId]) 
; 
CREATE TABLE [cmsContentXml] 
( 
[nodeId] [int] NOT NULL, 
[xml] [ntext] COLLATE Danish_Norwegian_CI_AS NOT NULL 
) 
 
; 
ALTER TABLE [cmsContentXml] ADD CONSTRAINT [PK_cmsContentXml] PRIMARY KEY CLUSTERED  ([nodeId]) 
; 
CREATE TABLE [cmsDataType] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[controlId] [uniqueidentifier] NOT NULL, 
[dbType] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) 
 
; 
ALTER TABLE [cmsDataType] ADD CONSTRAINT [PK_cmsDataType] PRIMARY KEY CLUSTERED  ([pk]) 
; 
CREATE TABLE [cmsDataTypePreValues] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[datatypeNodeId] [int] NOT NULL, 
[value] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[sortorder] [int] NOT NULL, 
[alias] [nvarchar] (50) COLLATE Danish_Norwegian_CI_AS NULL 
) 
 
; 
ALTER TABLE [cmsDataTypePreValues] ADD CONSTRAINT [PK_cmsDataTypePreValues] PRIMARY KEY CLUSTERED  ([id]) 
; 
CREATE TABLE [cmsDictionary] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[id] [uniqueidentifier] NOT NULL, 
[parent] [uniqueidentifier] NOT NULL, 
[key] [nvarchar] (1000) COLLATE Danish_Norwegian_CI_AS NOT NULL 
) 
 
; 
ALTER TABLE [cmsDictionary] ADD CONSTRAINT [PK_cmsDictionary] PRIMARY KEY CLUSTERED  ([pk]) 
; 
CREATE TABLE [cmsLanguageText] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[languageId] [int] NOT NULL, 
[UniqueId] [uniqueidentifier] NOT NULL, 
[value] [nvarchar] (1000) COLLATE Danish_Norwegian_CI_AS NOT NULL 
) 
 
; 
ALTER TABLE [cmsLanguageText] ADD CONSTRAINT [PK_cmsLanguageText] PRIMARY KEY CLUSTERED  ([pk]) 
; 
CREATE TABLE [cmsMember2MemberGroup] 
( 
[Member] [int] NOT NULL, 
[MemberGroup] [int] NOT NULL 
) 
 
; 
ALTER TABLE [cmsMember2MemberGroup] ADD CONSTRAINT [PK_cmsMember2MemberGroup] PRIMARY KEY CLUSTERED  ([Member], [MemberGroup]) 
; 
CREATE TABLE [cmsStylesheet] 
( 
[nodeId] [int] NOT NULL, 
[filename] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[content] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) 
 
; 
CREATE TABLE [cmsStylesheetProperty] 
( 
[nodeId] [int] NOT NULL, 
[stylesheetPropertyEditor] [bit] NULL, 
[stylesheetPropertyAlias] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[stylesheetPropertyValue] [nvarchar] (400) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) 
 
; 
CREATE TABLE [umbracoDomains] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[domainDefaultLanguage] [int] NULL, 
[domainRootStructureID] [int] NULL, 
[domainName] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) 
 
; 
ALTER TABLE [umbracoDomains] ADD CONSTRAINT [PK_domains] PRIMARY KEY CLUSTERED  ([id]) 
; 
CREATE TABLE [umbracoLanguage] 
( 
[id] [smallint] NOT NULL IDENTITY(1, 1), 
[languageISOCode] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, 
[languageCultureName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) 
 
; 
ALTER TABLE [umbracoLanguage] ADD CONSTRAINT [PK_language] PRIMARY KEY CLUSTERED  ([id]) 
; 
CREATE TABLE [umbracoRelationType] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[dual] [bit] NOT NULL, 
[parentObjectType] [uniqueidentifier] NOT NULL, 
[childObjectType] [uniqueidentifier] NOT NULL, 
[name] [nvarchar] (255) COLLATE Danish_Norwegian_CI_AS NOT NULL, 
[alias] [nvarchar] (100) COLLATE Danish_Norwegian_CI_AS NULL 
) 
 
; 
ALTER TABLE [umbracoRelationType] ADD CONSTRAINT [PK_umbracoRelationType] PRIMARY KEY CLUSTERED  ([id]) 
; 

/* TABLE IS NEVER USED, REMOVED FOR 4.1 

CREATE TABLE [umbracoStylesheet] 
( 
[nodeId] [int] NOT NULL, 
[filename] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, 
[content] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
)  
; 

ALTER TABLE [umbracoStylesheet] ADD CONSTRAINT [PK_umbracoStylesheet] PRIMARY KEY CLUSTERED  ([nodeId]) 
;

*/
 
CREATE TABLE [umbracoUser2NodeNotify] 
( 
[userId] [int] NOT NULL, 
[nodeId] [int] NOT NULL, 
[action] [char] (1) COLLATE Danish_Norwegian_CI_AS NOT NULL 
) 
 
; 
ALTER TABLE [umbracoUser2NodeNotify] ADD CONSTRAINT [PK_umbracoUser2NodeNotify] PRIMARY KEY CLUSTERED  ([userId], [nodeId], [action]) 
; 
CREATE TABLE [umbracoUser2NodePermission] 
( 
[userId] [int] NOT NULL, 
[nodeId] [int] NOT NULL, 
[permission] [char] (1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) 
 
; 
ALTER TABLE [umbracoUser2NodePermission] ADD CONSTRAINT [PK_umbracoUser2NodePermission] PRIMARY KEY CLUSTERED  ([userId], [nodeId], [permission]) 
;
ALTER TABLE [umbracoAppTree] ADD 
CONSTRAINT [FK_umbracoAppTree_umbracoApp] FOREIGN KEY ([appAlias]) REFERENCES [umbracoApp] ([appAlias]) 
; 
ALTER TABLE [cmsPropertyData] ADD 
CONSTRAINT [FK_cmsPropertyData_umbracoNode] FOREIGN KEY ([contentNodeId]) REFERENCES [umbracoNode] ([id]) 
; 

/* TABLES ARE NEVER USED, REMOVED FOR 4.1 

ALTER TABLE [umbracoUser2userGroup] ADD 
CONSTRAINT [FK_user2userGroup_user] FOREIGN KEY ([user]) REFERENCES [umbracoUser] ([id]), 
CONSTRAINT [FK_user2userGroup_userGroup] FOREIGN KEY ([userGroup]) REFERENCES [umbracoUserGroup] ([id]) 
;

*/
 
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

/* TABLE IS NEVER USED, REMOVED FOR 4.1

ALTER TABLE [umbracoUser2userGroup] DROP CONSTRAINT [FK_user2userGroup_user] 
; 
ALTER TABLE [umbracoUser2userGroup] DROP CONSTRAINT [FK_user2userGroup_userGroup] 
;

*/ 

ALTER TABLE [umbracoUser] DROP CONSTRAINT [FK_user_userType] 
; 
ALTER TABLE [cmsContentType] DROP CONSTRAINT [FK_cmsContentType_umbracoNode] 
; 
ALTER TABLE [cmsDocument] DROP CONSTRAINT [FK_cmsDocument_umbracoNode] 
; 
ALTER TABLE [umbracoNode] DROP CONSTRAINT [FK_umbracoNode_umbracoNode] 
; 
SET IDENTITY_INSERT [umbracoNode] ON 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-92, 0, -1, 0, 11, N'-1,-92', 37, 'f0bc4bfb-b499-40d6-ba86-058885a5178c', N'Label', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-90, 0, -1, 0, 11, N'-1,-90', 35, '84c6b441-31df-4ffe-b67e-67d5bc3ae65a', N'Upload', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-89, 0, -1, 0, 11, N'-1,-89', 34, 'c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3', N'Textarea', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-88, 0, -1, 0, 11, N'-1,-88', 33, '0cc0eba1-9960-42c9-bf9b-60e150b429ae', N'Textstring', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-87, 0, -1, 0, 11, N'-1,-87', 32, 'ca90c950-0aff-4e72-b976-a30b1ac57dad', N'Richtext editor', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-51, 0, -1, 0, 11, N'-1,-51', 4, '2e6d3631-066e-44b8-aec4-96f09099b2b5', N'Numeric', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-49, 0, -1, 0, 11, N'-1,-49', 2, '92897bc6-a5f3-4ffe-ae27-f2e7e33dda49', N'True/false', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-43, 0, -1, 0, 1, N'-1,-43', 2, 'fbaf13a8-4036-41f2-93a3-974f678c312a', N'Checkbox list', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:11:04.367') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-42, 0, -1, 0, 1, N'-1,-42', 2, '0b6a45e7-44ba-430d-9da5-4e46060b9e03', N'Dropdown', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:59.000') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-41, 0, -1, 0, 1, N'-1,-41', 2, '5046194e-4237-453c-a547-15db3a07c4e1', N'Date Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:54.303') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-40, 0, -1, 0, 1, N'-1,-40', 2, 'bb5f57c9-ce2b-4bb9-b697-4caca783a805', N'Radiobox', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:49.253') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-39, 0, -1, 0, 1, N'-1,-39', 2, 'f38f0ac7-1d27-439c-9f3f-089cd8825a53', N'Dropdown multiple', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:44.480') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-38, 0, -1, 0, 1, N'-1,-38', 2, 'fd9f1447-6c61-4a7c-9595-5aa39147d318', N'Folder Browser', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:37.020') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-37, 0, -1, 0, 1, N'-1,-37', 2, '0225af17-b302-49cb-9176-b9f35cab9c17', N'Approved Color', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:30.580') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-36, 0, -1, 0, 1, N'-1,-36', 2, 'e4d66c0f-b935-4200-81f0-025f7256b89a', N'Date Picker with time', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:23.007') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-1, 0, -1, 0, 0, N'-1', 0, '916724a5-173d-4619-b97e-b9de133dd6f5', N'SYSTEM DATA: umbraco master root', 'ea7d8624-4cfe-4578-a871-24aa946bf34d', '20040930 14:01:49.920') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1031, 0, -1, 1, 1, N'-1,1031', 2, 'f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d', N'Folder', '4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', '20041201 00:13:40.743') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1032, 0, -1, 1, 1, N'-1,1032', 2, 'cc07b313-0843-4aa8-bbda-871c8da728c8', N'Image', '4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', '20041201 00:13:43.737') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1033, 0, -1, 1, 1, N'-1,1033', 2, '4c52d8ab-54e6-40cd-999c-7a5f24903e4d', N'File', '4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', '20041201 00:13:46.210') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1034, 0, -1, 0, 1, N'-1,1034', 2, 'a6857c73-d6e9-480c-b6e6-f15f6ad11125', N'Content Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:29.203') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1035, 0, -1, 0, 1, N'-1,1035', 2, '93929b9a-93a2-4e2a-b239-d99334440a59', N'Media Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:36.143') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1036, 0, -1, 0, 1, N'-1,1036', 2, '2b24165f-9782-4aa3-b459-1de4a4d21f60', N'Member Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:40.260') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1038, 0, -1, 0, 1, N'-1,1038', 2, '1251c96c-185c-4e9b-93f4-b48205573cbd', N'Simple Editor', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:55.250') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1039, 0, -1, 0, 1, N'-1,1039', 2, '06f349a9-c949-4b6a-8660-59c10451af42', N'Ultimate Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:55.250') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1040, 0, -1, 0, 1, N'-1,1040', 2, '21e798da-e06e-4eda-a511-ed257f78d4fa', N'Related Links', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:55.250') 
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1041, 0, -1, 0, 1, N'-1,1041', 2, 'b6b73142-b9c1-4bf8-a16d-e1c23320b549', N'Tags', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:55.250')
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1042, 0, -1, 0, 1, N'-1,1042', 2, '0a452bd5-83f9-4bc3-8403-1286e13fb77e', N'Macro Container', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:55.250')
INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1043, 0, -1, 0, 1, N'-1,1043', 2, '1df9f033-e6d4-451f-b8d2-e0cbc50a836f', N'Image Cropper', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:55.250')

SET IDENTITY_INSERT [umbracoNode] OFF 
;
SET IDENTITY_INSERT [cmsContentType] ON 
INSERT INTO [cmsContentType] ([pk], [nodeId], [alias], [icon]) VALUES (532, 1031, N'Folder', N'folder.gif') 
INSERT INTO [cmsContentType] ([pk], [nodeId], [alias], [icon]) VALUES (533, 1032, N'Image', N'mediaPhoto.gif') 
INSERT INTO [cmsContentType] ([pk], [nodeId], [alias], [icon]) VALUES (534, 1033, N'File', N'mediaMulti.gif') 
SET IDENTITY_INSERT [cmsContentType] OFF 
;
SET IDENTITY_INSERT [umbracoUser] ON 
INSERT INTO [umbracoUser] ([id], [userDisabled], [userNoConsole], [userType], [startStructureID], [startMediaID], [userName], [userLogin], [userPassword], [userEmail], [userDefaultPermissions], [userLanguage]) VALUES (0, 0, 0, 1, -1, -1, N'Administrator', N'admin', N'default', N'', NULL, N'en') 
SET IDENTITY_INSERT [umbracoUser] OFF 
;
SET IDENTITY_INSERT [umbracoUserType] ON 
INSERT INTO [umbracoUserType] ([id], [userTypeAlias], [userTypeName], [userTypeDefaultPermissions]) VALUES (1, N'admin', N'Administrators', N'CADMOSKTPIURZ:') 
INSERT INTO [umbracoUserType] ([id], [userTypeAlias], [userTypeName], [userTypeDefaultPermissions]) VALUES (2, N'writer', N'Writer', N'CAH:') 
INSERT INTO [umbracoUserType] ([id], [userTypeAlias], [userTypeName], [userTypeDefaultPermissions]) VALUES (3, N'editor', N'Editors', N'CADMOSKTPUZ:') 
SET IDENTITY_INSERT [umbracoUserType] OFF 
;
INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'content') 
INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'developer') 
INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'media') 
INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'member') 
INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'settings') 
INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'users') 

INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'content', 0, N'.traycontent', N'Indhold', N'content') 
INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'developer', 7, N'.traydeveloper', N'Developer', NULL) 
INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'media', 1, N'.traymedia', N'Mediearkiv', NULL) 
INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'member', 8, N'.traymember', N'Medlemmer', NULL) 
INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'settings', 6, N'.traysettings', N'Indstillinger', NULL) 
INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'users', 5, N'.trayusers', N'Brugere', NULL)
 
 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'content', N'content', 1, 1, 0, N'Indhold', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadContent') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'cacheBrowser', 0, 1, 0, N'CacheBrowser', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadCache') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'CacheItem', 0, 0, 0, N'Cachebrowser', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadCacheItem') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'datatype', 0, 1, 1, N'Datatyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadDataTypes') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'macros', 0, 1, 2, N'Macros', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMacros') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'xslt', 0, 1, 5, N'XSLT Files', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadXslt') 

INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'media', N'media', 0, 1, 0, N'Medier', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMedia') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'member', N'member', 0, 1, 0, N'Medlemmer', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMembers') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'member', N'memberGroups', 0, 1, 1, N'MemberGroups', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMemberGroups') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'member', N'memberTypes', 0, 1, 2, N'Medlemstyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMemberTypes') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (N'settings', N'dictionary', 0, 1, 3, N'Dictionary', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadDictionary', N'openDictionary()') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'languages', 0, 1, 4, N'Languages', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadLanguages') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'mediaTypes', 0, 1, 5, N'Medietyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMediaTypes') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'documentTypes', 0, 1, 6, N'Dokumenttyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadNodeTypes') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'stylesheetProperty', 0, 0, 0, N'Stylesheet Property', N'', N'', N'umbraco', N'loadStylesheetProperty') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'stylesheets', 0, 1, 0, N'Stylesheets', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadStylesheets') 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'templates', 0, 1, 1, N'Templates', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadTemplates')
 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'users', N'users', 0, 1, 0, N'Brugere', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadUsers') 

SET IDENTITY_INSERT [cmsMacroPropertyType] ON 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (3, N'mediaCurrent', N'umbraco.macroRenderings', N'media', N'Int32') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (4, N'contentSubs', N'umbraco.macroRenderings', N'content', N'Int32') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (5, N'contentRandom', N'umbraco.macroRenderings', N'content', N'Int32') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (6, N'contentPicker', N'umbraco.macroRenderings', N'content', N'Int32') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (13, N'number', N'umbraco.macroRenderings', N'numeric', N'Int32') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (14, N'bool', N'umbraco.macroRenderings', N'yesNo', N'Boolean') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (16, N'text', N'umbraco.macroRenderings', N'text', N'String') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (17, N'contentTree', N'umbraco.macroRenderings', N'content', N'Int32') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (18, N'contentType', N'umbraco.macroRenderings', N'contentTypeSingle', N'Int32') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (19, N'contentTypeMultiple', N'umbraco.macroRenderings', N'contentTypeMultiple', N'Int32') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (20, N'contentAll', N'umbraco.macroRenderings', N'content', N'Int32') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (21, N'tabPicker', N'umbraco.macroRenderings', N'tabPicker', N'String') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (22, N'tabPickerMultiple', N'umbraco.macroRenderings', N'tabPickerMultiple', N'String') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (23, N'propertyTypePicker', N'umbraco.macroRenderings', N'propertyTypePicker', N'String') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (24, N'propertyTypePickerMultiple', N'umbraco.macroRenderings', N'propertyTypePickerMultiple', N'String') 
INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (25, N'textMultiLine', N'umbraco.macroRenderings', N'textMultiple', N'String') 
SET IDENTITY_INSERT [cmsMacroPropertyType] OFF 
;
SET IDENTITY_INSERT [cmsTab] ON 
INSERT INTO [cmsTab] ([id], [contenttypeNodeId], [text], [sortorder]) VALUES (3, 1032, N'Image', 1) 
INSERT INTO [cmsTab] ([id], [contenttypeNodeId], [text], [sortorder]) VALUES (4, 1033, N'File', 1) 
INSERT INTO [cmsTab] ([id], [contenttypeNodeId], [text], [sortorder]) VALUES (5, 1031, N'Contents', 1) 
SET IDENTITY_INSERT [cmsTab] OFF 
;
SET IDENTITY_INSERT [cmsPropertyType] ON 
INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (6, -90, 1032, 3, N'umbracoFile', N'Upload image', NULL, 0, 0, NULL, NULL) 
INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (7, -92, 1032, 3, N'umbracoWidth', N'Width', NULL, 0, 0, NULL, NULL) 
INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (8, -92, 1032, 3, N'umbracoHeight', N'Height', NULL, 0, 0, NULL, NULL) 
INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (9, -92, 1032, 3, N'umbracoBytes', N'Size', NULL, 0, 0, NULL, NULL) 
INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (10, -92, 1032, 3, N'umbracoExtension', N'Type', NULL, 0, 0, NULL, NULL) 
INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (24, -90, 1033, 4, N'umbracoFile', N'Upload file', NULL, 0, 0, NULL, NULL) 
INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (25, -92, 1033, 4, N'umbracoExtension', N'Type', NULL, 0, 0, NULL, NULL) 
INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (26, -92, 1033, 4, N'umbracoBytes', N'Size', NULL, 0, 0, NULL, NULL) 
INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (27, -38, 1031, 5, N'contents', N'Contents:', NULL, 0, 0, NULL, NULL) 
SET IDENTITY_INSERT [cmsPropertyType] OFF 
;
SET IDENTITY_INSERT [umbracoLanguage] ON 
INSERT INTO [umbracoLanguage] ([id], [languageISOCode], [languageCultureName]) VALUES (1, N'en-US', N'en-US') 
SET IDENTITY_INSERT [umbracoLanguage] OFF 
;
INSERT INTO [cmsContentTypeAllowedContentType] ([Id], [AllowedId]) VALUES (1031, 1031) 
INSERT INTO [cmsContentTypeAllowedContentType] ([Id], [AllowedId]) VALUES (1031, 1032) 
INSERT INTO [cmsContentTypeAllowedContentType] ([Id], [AllowedId]) VALUES (1031, 1033) 
SET IDENTITY_INSERT [cmsDataType] ON 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (4, -49, '38b352c1-e9f8-4fd8-9324-9a2eab06d97a', 'Integer') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (6, -51, '1413afcb-d19a-4173-8e9a-68288d2a73b8', 'Integer') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (8, -87, '5E9B75AE-FACE-41c8-B47E-5F4B0FD82F83', 'Ntext') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (9, -88, 'ec15c1e5-9d90-422a-aa52-4f7622c63bea', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (10, -89, '67db8357-ef57-493e-91ac-936d305e0f2a', 'Ntext') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (11, -90, '5032a6e6-69e3-491d-bb28-cd31cd11086c', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (12, -91, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (13, -92, '6c738306-4c17-4d88-b9bd-6546f3771597', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (14, -36, 'b6fb1622-afa5-4bbf-a3cc-d9672a442222', 'Date') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (15, -37, 'f8d60f68-ec59-4974-b43b-c46eb5677985', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (16, -38, 'cccd4ae9-f399-4ed2-8038-2e88d19e810c', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (17, -39, '928639ed-9c73-4028-920c-1e55dbb68783', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (18, -40, 'a52c7c1c-c330-476e-8605-d63d3b84b6a6', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (19, -41, '23e93522-3200-44e2-9f29-e61a6fcbb79a', 'Date') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (20, -42, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Integer') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (21, -43, 'b4471851-82b6-4c75-afa4-39fa9c6a75e9', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (22, -44, 'a3776494-0574-4d93-b7de-efdfdec6f2d1', 'Ntext') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (23, -128, 'a52c7c1c-c330-476e-8605-d63d3b84b6a6', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (24, -129, '928639ed-9c73-4028-920c-1e55dbb68783', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (25, -130, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (26, -131, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (27, -132, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (28, -133, '6c738306-4c17-4d88-b9bd-6546f3771597', 'Ntext') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (29, -134, '928639ed-9c73-4028-920c-1e55dbb68783', 'Nvarchar') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (30, -50, 'aaf99bb2-dbbe-444d-a296-185076bf0484', 'Date') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (31, 1034, '158aa029-24ed-4948-939e-c3da209e5fba', 'Integer') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (32, 1035, 'ead69342-f06d-4253-83ac-28000225583b', 'Integer') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (33, 1036, '39f533e4-0551-4505-a64b-e0425c5ce775', 'Integer') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (35, 1038, '60b7dabf-99cd-41eb-b8e9-4d2e669bbde9', 'Ntext') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (36, 1039, 'cdbf0b5d-5cb2-445f-bc12-fcaaec07cf2c', 'Ntext') 
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (37, 1040, '71b8ad1a-8dc2-425c-b6b8-faa158075e63', 'Ntext')
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (38, 1041, '4023e540-92f5-11dd-ad8b-0800200c9a66', 'Ntext')
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (39, 1042, '474FCFF8-9D2D-11DE-ABC6-AD7A56D89593', 'Ntext')
INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (40, 1043, '7A2D436C-34C2-410F-898F-4A23B3D79F54', 'Ntext')

SET IDENTITY_INSERT [cmsDataType] OFF 
;
ALTER TABLE [cmsTemplate] ADD CONSTRAINT [FK_cmsTemplate_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
ALTER TABLE [cmsPropertyType] ADD CONSTRAINT [FK_cmsPropertyType_cmsTab] FOREIGN KEY ([tabId]) REFERENCES [cmsTab] ([id]) 
ALTER TABLE [cmsContent] ADD CONSTRAINT [FK_cmsContent_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
ALTER TABLE [cmsMacroProperty] ADD CONSTRAINT [FK_umbracoMacroProperty_umbracoMacroPropertyType] FOREIGN KEY ([macroPropertyType]) REFERENCES [cmsMacroPropertyType] ([id]) 
ALTER TABLE [umbracoAppTree] ADD CONSTRAINT [FK_umbracoAppTree_umbracoApp] FOREIGN KEY ([appAlias]) REFERENCES [umbracoApp] ([appAlias]) 
ALTER TABLE [umbracoUser2app] ADD CONSTRAINT [FK_umbracoUser2app_umbracoApp] FOREIGN KEY ([app]) REFERENCES [umbracoApp] ([appAlias]) 
ALTER TABLE [umbracoUser2app] ADD CONSTRAINT [FK_umbracoUser2app_umbracoUser] FOREIGN KEY ([user]) REFERENCES [umbracoUser] ([id]) 
ALTER TABLE [cmsPropertyData] ADD CONSTRAINT [FK_cmsPropertyData_umbracoNode] FOREIGN KEY ([contentNodeId]) REFERENCES [umbracoNode] ([id]) 

/* TABLE IS NEVER USED, REMOVED FOR 4.1

ALTER TABLE [umbracoUser2userGroup] ADD CONSTRAINT [FK_user2userGroup_user] FOREIGN KEY ([user]) REFERENCES [umbracoUser] ([id]) 
ALTER TABLE [umbracoUser2userGroup] ADD CONSTRAINT [FK_user2userGroup_userGroup] FOREIGN KEY ([userGroup]) REFERENCES [umbracoUserGroup] ([id]) 

*/

ALTER TABLE [umbracoUser] ADD CONSTRAINT [FK_user_userType] FOREIGN KEY ([userType]) REFERENCES [umbracoUserType] ([id]) 
ALTER TABLE [cmsContentType] ADD CONSTRAINT [FK_cmsContentType_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
ALTER TABLE [cmsDocument] ADD CONSTRAINT [FK_cmsDocument_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
ALTER TABLE [umbracoNode] ADD CONSTRAINT [FK_umbracoNode_umbracoNode] FOREIGN KEY ([parentID]) REFERENCES [umbracoNode] ([id]) 
;
set identity_insert umbracoNode on
insert into umbracoNode
(id, trashed, parentID, nodeUser, level, path, sortOrder, uniqueID, text, nodeObjectType)
values
(-20, 0, -1, 0, 0, '-1,-20', 0, '0F582A79-1E41-4CF0-BFA0-76340651891A', 'Recycle Bin', '01BB7FF2-24DC-4C0C-95A2-C24EF72BBAC8')
set identity_insert umbracoNode off
;
ALTER TABLE cmsDataTypePreValues ALTER COLUMN value NVARCHAR(2500) NULL
;
CREATE TABLE [cmsTask]
(
[closed] [bit] NOT NULL CONSTRAINT [DF__cmsTask__closed__04E4BC85] DEFAULT ((0)),
[id] [int] NOT NULL IDENTITY(1, 1),
[taskTypeId] [tinyint] NOT NULL,
[nodeId] [int] NOT NULL,
[parentUserId] [int] NOT NULL,
[userId] [int] NOT NULL,
[DateTime] [datetime] NOT NULL CONSTRAINT [DF__cmsTask__DateTim__05D8E0BE] DEFAULT (getdate()),
[Comment] [nvarchar] (500) NULL
)
;
ALTER TABLE [cmsTask] ADD CONSTRAINT [PK_cmsTask] PRIMARY KEY CLUSTERED  ([id])
;
CREATE TABLE [cmsTaskType]
(
[id] [tinyint] NOT NULL IDENTITY(1, 1),
[alias] [nvarchar] (255) NOT NULL
)
;
ALTER TABLE [cmsTaskType] ADD CONSTRAINT [PK_cmsTaskType] PRIMARY KEY CLUSTERED  ([id])
;
ALTER TABLE [cmsTask] ADD
CONSTRAINT [FK_cmsTask_cmsTaskType] FOREIGN KEY ([taskTypeId]) REFERENCES [cmsTaskType] ([id])
;
insert into cmsTaskType (alias) values ('toTranslate')
;
/* Add send to translate actions to admins and editors */
update umbracoUserType set userTypeDefaultPermissions = userTypeDefaultPermissions + '5' where userTypeAlias in ('editor','admin')
;
/* Add translator usertype */
if not exists(select id from umbracoUserType where userTypeAlias = 'translator')
insert into umbracoUserType (userTypeAlias, userTypeName, userTypeDefaultPermissions) values ('translator', 'Translator', 'A')
;
insert into umbracoRelationType (dual, parentObjectType, childObjectType, name, alias) values (1, 'c66ba18e-eaf3-4cff-8a22-41b16d66a972', 'c66ba18e-eaf3-4cff-8a22-41b16d66a972', 'Relate Document On Copy','relateDocumentOnCopy')
;
ALTER TABLE cmsMacro ADD macroPython nvarchar(255)
;

INSERT INTO [umbracoAppTree]([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES(0, 1, 4, 'developer', 'python', 'Python Files', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadPython') 
;
INSERT INTO [umbracoAppTree]([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES(0, 1, 2, 'settings', 'scripts', 'Scripts', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadScripts') 
;
alter TABLE [cmsContentType]
add [thumbnail] nvarchar(255) NOT NULL CONSTRAINT
[DF_cmsContentType_thumbnail] DEFAULT ('folder.png')
;
alter TABLE [cmsContentType]
add [description] nvarchar(1500) NULL
;
ALTER TABLE umbracoLog ALTER COLUMN logComment NVARCHAR(4000) NULL
;
SET IDENTITY_INSERT [cmsDataTypePreValues] ON 
insert into cmsDataTypePreValues (id, dataTypeNodeId, [value], sortorder, alias)
values (3,-87,',code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,mcecharmap,' + char(124) + '1' + char(124) + '1,2,3,' + char(124) + '0' + char(124) + '500,400' + char(124) + '1049,' + char(124) + '', 0, '')

insert into cmsDataTypePreValues (id, dataTypeNodeId, [value], sortorder, alias)
values (4,1041,'default', 0, 'group')

SET IDENTITY_INSERT [cmsDataTypePreValues] OFF
;
/* 3.1 SQL changes */
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'packager', 0, 1, 3, N'Packages', N'folder.gif', N'folder_o.gif', N'umbraco', N'loadPackager') 
;
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'packagerPackages', 0, 0, 1, N'Packager Packages', N'folder.gif', N'folder_o.gif', N'umbraco', N'loadPackages');


/* Add ActionBrowse as a default permission to all user types that have ActionUpdate */
UPDATE umbracoUserType SET userTypeDefaultPermissions = userTypeDefaultPermissions + 'F' WHERE CHARINDEX('A',userTypeDefaultPermissions,0) >= 1
AND CHARINDEX('F',userTypeDefaultPermissions,0) < 1
;
/* Add ActionToPublish to all users types that have the alias 'writer' */
UPDATE umbracoUserType SET userTypeDefaultPermissions = userTypeDefaultPermissions + 'H' WHERE userTypeAlias = 'writer'
AND CHARINDEX('F',userTypeDefaultPermissions,0) < 1
;
/* Add ActionBrowse to all user permissions for nodes that have the ActionUpdate permission */
IF NOT EXISTS (SELECT permission FROM umbracoUser2NodePermission WHERE permission='F')
INSERT INTO umbracoUser2NodePermission (userID, nodeId, permission) 
SELECT userID, nodeId, 'F' FROM umbracoUser2NodePermission WHERE permission='A'
;
/* Add ActionToPublish permissions to all nodes for users that are of type 'writer' */
IF NOT EXISTS (SELECT permission FROM umbracoUser2NodePermission WHERE permission='H')
INSERT INTO umbracoUser2NodePermission (userID, nodeId, permission) 
SELECT DISTINCT userID, nodeId, 'H' FROM umbracoUser2NodePermission WHERE userId IN
(SELECT umbracoUser.id FROM umbracoUserType INNER JOIN umbracoUser ON umbracoUserType.id = umbracoUser.userType WHERE (umbracoUserType.userTypeAlias = 'writer'))
;
/* Add the contentRecycleBin tree type */
IF NOT EXISTS (SELECT treeAlias FROM umbracoAppTree WHERE treeAlias='contentRecycleBin')
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 0, 0, 'content', 'contentRecycleBin', 'RecycleBin', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.ContentRecycleBin')
;
/* Add the UserType tree type */
IF NOT EXISTS (SELECT treeAlias FROM umbracoAppTree WHERE treeAlias='userTypes')
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 1, 'users', 'userTypes', 'User Types', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.UserTypes')
;
/* Add the User Permission tree type */
IF NOT EXISTS (SELECT treeAlias FROM umbracoAppTree WHERE treeAlias='userPermissions')
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 2, 'users', 'userPermissions', 'User Permissions', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.UserPermissions');
 
 
/* TRANSLATION RELATED SQL */
INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'translation', 5, N'.traytranslation', N'Translation', NULL)
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 1, 'translation','openTasks', 'Tasks assigned to you', '.sprTreeFolder', '.sprTreeFolder_o', 'umbraco', 'loadOpenTasks');
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 2, 'translation','yourTasks', 'Tasks created by you', '.sprTreeFolder', '.sprTreeFolder_o', 'umbraco', 'loadYourTasks');
 
 
alter TABLE [cmsContentType]
add [masterContentType] int NULL CONSTRAINT
[DF_cmsContentType_masterContentType] DEFAULT (0)
;

CREATE TABLE [cmsTagRelationship](
	[nodeId] [int] NOT NULL,
	[tagId] [int] NOT NULL,
 CONSTRAINT [PK_cmsTagRelationship] PRIMARY KEY CLUSTERED 
(
	[nodeId] ASC,
	[tagId] ASC
)
) ON [PRIMARY]
;

CREATE TABLE [cmsTags](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[tag] [varchar](200) NULL,
	[parentId] [int] NULL,
	[group] [varchar](100) NULL,
 CONSTRAINT [PK_cmsTags] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)
) ON [PRIMARY]
;

ALTER TABLE [cmsTagRelationship]  WITH CHECK ADD  CONSTRAINT [umbracoNode_cmsTagRelationship] FOREIGN KEY([nodeId])
REFERENCES [umbracoNode] ([id])
ON DELETE CASCADE
;

ALTER TABLE [cmsTagRelationship] CHECK CONSTRAINT [umbracoNode_cmsTagRelationship]
;

ALTER TABLE [cmsTagRelationship]  WITH CHECK ADD  CONSTRAINT [cmsTags_cmsTagRelationship] FOREIGN KEY([tagId])
REFERENCES [cmsTags] ([id])
ON DELETE CASCADE
;

ALTER TABLE [cmsTagRelationship] CHECK CONSTRAINT [cmsTags_cmsTagRelationship]
;
alter TABLE [umbracoUser]
add [defaultToLiveEditing] bit NOT NULL CONSTRAINT
[DF_umbracoUser_defaultToLiveEditing] DEFAULT (0)
;

/* INSERT NEW MEDIA RECYCLE BIN NODE */
SET IDENTITY_INSERT [umbracoNode] ON
INSERT INTO umbracoNode (id, trashed, parentID, nodeUser, level, path, sortOrder, uniqueID, text, nodeObjectType)
VALUES (-21, 0, -1, 0, 0, '-1,-21', 0, 'BF7C7CBC-952F-4518-97A2-69E9C7B33842', 'Recycle Bin', 'CF3D8E34-1C1C-41e9-AE56-878B57B32113')
SET IDENTITY_INSERT [umbracoNode] OFF
;
/* Add the mediaRecycleBin tree type */
IF NOT EXISTS (SELECT treeAlias FROM umbracoAppTree WHERE treeAlias='mediaRecycleBin')
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 0, 0, 'media', 'mediaRecycleBin', 'RecycleBin', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.MediaRecycleBin')
;

/* PREVIEW */
CREATE TABLE [cmsPreviewXml](
	[nodeId] [int] NOT NULL,
	[versionId] [uniqueidentifier] NOT NULL,
	[timestamp] [datetime] NOT NULL,
	[xml] [ntext] NOT NULL,
 CONSTRAINT [PK_cmsContentPreviewXml] PRIMARY KEY CLUSTERED 
(
	[nodeId] ASC,
	[versionId] ASC
) WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
)
;




/***********************************************************************************************************************

ADD NEW PRIMARY KEYS, FOREIGN KEYS AND INDEXES FOR VERSION 4.1

IMPORTANT!!!!!
YOU MUST MAKE SURE THAT THE SCRIPT BELOW THIS MATCHES THE KeysIndexesAndConstraints.sql FILE FOR THE MANUAL UPGRADE

*/

/************************** CLEANUP ***********************************************/

/* DELETE NON-EXISTING DOCUMENTS */
delete from cmsDocument where nodeId not in (select id from umbracoNode)
;

/* CLEAN UNUSED CONTENT ROWS */
delete from cmsContent where nodeId not in (select id from umbracoNode)
;

/* CLEAN UNUSED VERSIONS */
delete from cmsContentVersion where contentid not in (select nodeId from cmsContent)
;

/* CLEAN UNUSED XML */
delete from cmsContentXml where nodeid not in (select nodeId from cmsContent)
;

/* CLEAN UNUSED DOCUMENT TYPES */
delete from cmsDocumentType where contentTypeNodeId not in (select nodeId from cmsContentType)
;
delete from cmsDocumentType where templateNodeId not in (select nodeid from cmsTemplate)
;

/* UPDATE EMPTY TEMPLATE REFERENCES IN DOCUMENTS */
update cmsDocument set templateId = NULL where templateId not in (select nodeId from cmsTemplate)
;

/* DELETE ALL NOTIFICATIONS THAT NO LONGER HAVE NODES */
delete from umbracoUser2NodeNotify where nodeId not in (select id from umbracoNode)
;

/* DELETE ALL NOTIFICATIONS THAT NO LONGER HAVE USERS */
delete from umbracoUser2NodeNotify where userId not in (select id from umbracoUser)
;

/* DELETE UMBRACO NODE DATA THAT IS FLAGGED AS A DOCUMENT OBJECT TYPE THAT DOESN'T EXIST IN THE CONTENT TABLE ANY LONGER */
delete from umbracoNode where id not in
(select nodeId from cmsContent) and nodeObjectType = 'c66ba18e-eaf3-4cff-8a22-41b16d66a972'
;

/* DELETE PERMISSIONS THAT RELATED TO NON-EXISTING USERS */ 
delete from umbracoUser2NodePermission where userId not in (select id from umbracoUser)
;

/* DELETE PERMISSIONS THAT RELATED TO NON-EXISTING NODES */
delete from umbracoUser2NodePermission where nodeId not in (select id from umbracoNode)
;

/* SET MASTER TEMPLATE TO NULL WHEN THERE ISN'T ONE SPECIFIED */
update cmsTemplate set [master] = NULL where [master] = 0

/* 
We need to remove any data type that doesn't exist in umbracoNode as these shouldn't actually exist 
I think they must be left over from how Umbraco used to show the types of data types registered instead
of using reflection. Here are the data types in the cmsDataType table that are not in umbracoNode:

12	-91	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Nvarchar
22	-44	A3776494-0574-4D93-B7DE-EFDFDEC6F2D1	Ntext
23	-128	A52C7C1C-C330-476E-8605-D63D3B84B6A6	Nvarchar
24	-129	928639ED-9C73-4028-920C-1E55DBB68783	Nvarchar
25	-130	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Nvarchar
26	-131	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Nvarchar
27	-132	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Nvarchar
28	-133	6C738306-4C17-4D88-B9BD-6546F3771597	Ntext
29	-134	928639ED-9C73-4028-920C-1E55DBB68783	Nvarchar
30	-50	AAF99BB2-DBBE-444D-A296-185076BF0484	Date
39	1042	5E9B75AE-FACE-41C8-B47E-5F4B0FD82F83	Ntext
40	1043	5E9B75AE-FACE-41C8-B47E-5F4B0FD82F83	Ntext
41	1044	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Ntext
42	1045	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Ntext
47	1194	D15E1281-E456-4B24-AA86-1DDA3E4299D5	Ntext

*/
DELETE FROM cmsDataType WHERE nodeId NOT IN (SELECT id FROM umbracoNode)
;

/* Need to remove any data type prevalues that aren't related to a data type */
DELETE FROM cmsDataTypePreValues WHERE dataTypeNodeID NOT IN (SELECT nodeId FROM cmsDataType)
;

/* Remove any domains that should not exist as they weren't deleted before when documents were deleted */
DELETE FROM umbracoDomains WHERE domainRootStructureId NOT IN (SELECT id FROM umbracoNode)
;

-- It would be good to add constraints from cmsLanguageText to umbracoLanguage but unfortunately, a 'zero' id 
-- is entered into cmsLanguageText when a new entry is made, since there's not language with id of zero this won't work.
-- However, we need to remove translations that aren't related to a language (these would be left over from deleting a language)
DELETE FROM cmsLanguageText
WHERE languageId <> 0 AND languageId NOT IN (SELECT id FROM umbracoLanguage)
;

/* need to remove any content restrictions that don't exist in cmsContent */

DELETE FROM cmsContentTypeAllowedContentType WHERE id NOT IN (SELECT nodeId FROM cmsContentType)
;
DELETE FROM cmsContentTypeAllowedContentType WHERE Allowedid NOT IN (SELECT nodeId FROM cmsContentType)
;

/* Though this should not have to be run because it's a new install, you need to clean the previews if you've been testing before the RC */
DELETE FROM cmsPreviewXml WHERE VersionID NOT IN (SELECT VersionId FROM cmsContentVersion)
;

/* Though this should not have to run because it's a new install, you need to remove this constraint if you've been testing with the RC */
/*IF EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_cmsMemberType')*/
/*ALTER TABLE [cmsMemberType] DROP CONSTRAINT [IX_cmsMemberType]*/

/************************** CLEANUP END ********************************************/


/* Create missing indexes and primary keys */
CREATE NONCLUSTERED INDEX [IX_Icon] ON CMSContenttype(nodeId, Icon)
;

ALTER TABLE cmsContentType ADD CONSTRAINT
	IX_cmsContentType UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsContent ADD CONSTRAINT
	IX_cmsContent UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsContentVersion ADD CONSTRAINT
	FK_cmsContentVersion_cmsContent FOREIGN KEY
	(
	ContentId
	) REFERENCES cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMember ADD CONSTRAINT
	PK_cmsMember PRIMARY KEY CLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsMember ADD CONSTRAINT
	FK_cmsMember_cmsContent FOREIGN KEY
	(
	nodeId
	) REFERENCES cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMember ADD CONSTRAINT
	FK_cmsMember_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsStylesheet ADD CONSTRAINT
	PK_cmsStylesheet PRIMARY KEY CLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsStylesheetProperty ADD CONSTRAINT
	PK_cmsStylesheetProperty PRIMARY KEY CLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsStylesheetProperty ADD CONSTRAINT
	FK_cmsStylesheetProperty_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsStylesheet ADD CONSTRAINT
	FK_cmsStylesheet_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsContentXml ADD CONSTRAINT
	FK_cmsContentXml_cmsContent FOREIGN KEY
	(
	nodeId
	) REFERENCES cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDataType ADD CONSTRAINT
	IX_cmsDataType UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;



ALTER TABLE cmsDataType ADD CONSTRAINT
	FK_cmsDataType_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;



ALTER TABLE cmsDataTypePreValues ADD CONSTRAINT
	FK_cmsDataTypePreValues_cmsDataType FOREIGN KEY
	(
	datatypeNodeId
	) REFERENCES cmsDataType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDocument ADD CONSTRAINT
	FK_cmsDocument_cmsContent FOREIGN KEY
	(
	nodeId
	) REFERENCES cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDocumentType ADD CONSTRAINT
	FK_cmsDocumentType_cmsContentType FOREIGN KEY
	(
	contentTypeNodeId
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDocumentType ADD CONSTRAINT
	FK_cmsDocumentType_umbracoNode FOREIGN KEY
	(
	contentTypeNodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMacroProperty ADD CONSTRAINT
	FK_cmsMacroProperty_cmsMacro FOREIGN KEY
	(
	macro
	) REFERENCES cmsMacro
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMemberType ADD CONSTRAINT
	FK_cmsMemberType_cmsContentType FOREIGN KEY
	(
	NodeId
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMemberType ADD CONSTRAINT
	FK_cmsMemberType_umbracoNode FOREIGN KEY
	(
	NodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMember2MemberGroup ADD CONSTRAINT
	FK_cmsMember2MemberGroup_cmsMember FOREIGN KEY
	(
	Member
	) REFERENCES cmsMember
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDocument ADD CONSTRAINT
	IX_cmsDocument UNIQUE NONCLUSTERED 
	(
	nodeId,
	versionId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsPropertyData ADD CONSTRAINT
	FK_cmsPropertyData_cmsPropertyType FOREIGN KEY
	(
	propertytypeid
	) REFERENCES cmsPropertyType
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsPropertyType ADD CONSTRAINT
	FK_cmsPropertyType_cmsContentType FOREIGN KEY
	(
	contentTypeId
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsPropertyType ADD CONSTRAINT
	FK_cmsPropertyType_cmsDataType FOREIGN KEY
	(
	dataTypeId
	) REFERENCES cmsDataType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTab ADD CONSTRAINT
	FK_cmsTab_cmsContentType FOREIGN KEY
	(
	contenttypeNodeId
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTemplate ADD CONSTRAINT
	IX_cmsTemplate UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsDocument ADD CONSTRAINT
	FK_cmsDocument_cmsTemplate FOREIGN KEY
	(
	templateId
	) REFERENCES cmsTemplate
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;


ALTER TABLE umbracoDomains ADD CONSTRAINT
	FK_umbracoDomains_umbracoNode FOREIGN KEY
	(
	domainRootStructureID
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDictionary ADD CONSTRAINT
	IX_cmsDictionary UNIQUE NONCLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsLanguageText ADD CONSTRAINT
	FK_cmsLanguageText_cmsDictionary FOREIGN KEY
	(
	UniqueId
	) REFERENCES cmsDictionary
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;



ALTER TABLE umbracoUser2NodeNotify ADD CONSTRAINT
	FK_umbracoUser2NodeNotify_umbracoUser FOREIGN KEY
	(
	userId
	) REFERENCES umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoUser2NodeNotify ADD CONSTRAINT
	FK_umbracoUser2NodeNotify_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoUser2NodePermission ADD CONSTRAINT
	FK_umbracoUser2NodePermission_umbracoUser FOREIGN KEY
	(
	userId
	) REFERENCES umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoUser2NodePermission ADD CONSTRAINT
	FK_umbracoUser2NodePermission_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTask ADD CONSTRAINT
	FK_cmsTask_umbracoUser FOREIGN KEY
	(
	parentUserId
	) REFERENCES umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTask ADD CONSTRAINT
	FK_cmsTask_umbracoUser1 FOREIGN KEY
	(
	userId
	) REFERENCES umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTask ADD CONSTRAINT
	FK_cmsTask_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

CREATE NONCLUSTERED INDEX IX_umbracoLog ON umbracoLog
	(
	NodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE umbracoRelation ADD CONSTRAINT
	FK_umbracoRelation_umbracoRelationType FOREIGN KEY
	(
	relType
	) REFERENCES umbracoRelationType
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoRelation ADD CONSTRAINT
	FK_umbracoRelation_umbracoNode FOREIGN KEY
	(
	parentId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoRelation ADD CONSTRAINT
	FK_umbracoRelation_umbracoNode1 FOREIGN KEY
	(
	childId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;



ALTER TABLE cmsContentTypeAllowedContentType ADD CONSTRAINT
	FK_cmsContentTypeAllowedContentType_cmsContentType FOREIGN KEY
	(
	Id
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsContentTypeAllowedContentType ADD CONSTRAINT
	FK_cmsContentTypeAllowedContentType_cmsContentType1 FOREIGN KEY
	(
	AllowedId
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoLanguage ADD CONSTRAINT
	IX_umbracoLanguage UNIQUE NONCLUSTERED 
	(
	languageISOCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE umbracoUser ADD CONSTRAINT
	IX_umbracoUser UNIQUE NONCLUSTERED 
	(
	userLogin
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsTaskType ADD CONSTRAINT
	IX_cmsTaskType UNIQUE NONCLUSTERED 
	(
	alias
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsDocumentType ADD CONSTRAINT
	FK_cmsDocumentType_cmsTemplate FOREIGN KEY
	(
	templateNodeId
	) REFERENCES cmsTemplate
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTemplate ADD CONSTRAINT
	FK_cmsTemplate_cmsTemplate FOREIGN KEY
	(
	master
	) REFERENCES cmsTemplate
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsContentVersion ADD CONSTRAINT
	IX_cmsContentVersion UNIQUE NONCLUSTERED 
	(
	VersionId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsPreviewXml ADD CONSTRAINT
	FK_cmsPreviewXml_cmsContentVersion FOREIGN KEY
	(
	versionId
	) REFERENCES cmsContentVersion
	(
	VersionId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsPreviewXml ADD CONSTRAINT
	FK_cmsPreviewXml_cmsContent FOREIGN KEY
	(
	nodeId
	) REFERENCES cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;


ALTER TABLE cmsMember2MemberGroup ADD CONSTRAINT
	FK_cmsMember2MemberGroup_umbracoNode FOREIGN KEY
	(
	MemberGroup
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;


/***********************************************************************************************************************

END OF NEW CONSTRAINTS

***********************************************************************************************************************/