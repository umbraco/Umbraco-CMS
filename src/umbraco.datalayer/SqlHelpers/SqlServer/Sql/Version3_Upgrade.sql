/*******************************************************************************************







    Umbraco database installation script for SQL Server (upgrade from Umbraco 3.x)
 
IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
 
    Database version: 4.0.0.12
    
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

INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'packager', 0, 1, 1, N'Packages', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadPackager') 
;
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'packagerPackages', 0, 0, 1, N'Packager Packages', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadPackages')
;
alter TABLE umbracoAppTree
add [action] nvarchar(255) NULL
;
update umbracoAppTree set [action] = 'openDictionary()' where [treeAlias] =  'dictionary'
;
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
/* Add ActionToPublish permissions to the writer user type */
UPDATE umbracoUserType SET userTypeDefaultPermissions = userTypeDefaultPermissions + 'H' WHERE userTypeAlias='writer' AND CHARINDEX('H',userTypeDefaultPermissions,0) < 1
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
VALUES (0, 0, 0, 'content', 'contentRecycleBin', 'RecycleBin', '.sprTreeFolder', '.sprTreeFolder_o', 'umbraco', 'cms.presentation.Trees.ContentRecycleBin')
;
/* Add the UserType tree type */
IF NOT EXISTS (SELECT treeAlias FROM umbracoAppTree WHERE treeAlias='userTypes')
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 1, 'users', 'userTypes', 'User Types', '.sprTreeFolder', '.sprTreeFolder_o', 'umbraco', 'cms.presentation.Trees.UserTypes')
;
/* Add the User Permission tree type */
IF NOT EXISTS (SELECT treeAlias FROM umbracoAppTree WHERE treeAlias='userPermissions')
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 2, 'users', 'userPermissions', 'User Permissions', '.sprTreeFolder', '.sprTreeFolder_o', 'umbraco', 'cms.presentation.Trees.UserPermissions')
; 
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
update cmsDataType set controlId = '5E9B75AE-FACE-41c8-B47E-5F4B0FD82F83' where controlId = '83722133-F80C-4273-BDB6-1BEFAA04A612'
;

/* TRANSLATION RELATED SQL */
INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'translation', 5, N'.traytranslation', N'Translation', NULL)
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 1, 'translation','openTasks', 'Tasks assigned to you', '.sprTreeFolder', '.sprTreeFolder_o', 'umbraco', 'loadOpenTasks');
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 2, 'translation','yourTasks', 'Tasks created by you', '.sprTreeFolder', '.sprTreeFolder_o', 'umbraco', 'loadYourTasks');
/* UPDATE SECTION CSS SPRITES*/
update umbracoApp set appIcon = '.tray' + appAlias where appAlias IN ('content','media','users','settings','developer','member');

/* primary key on cmsContentVersion */
ALTER TABLE [cmsContentVersion] ADD CONSTRAINT [PK_ContentVersion] PRIMARY KEY CLUSTERED  ([id]); 

/* add Canvas editing to existing user types */
UPDATE [umbracoUserType] set [userTypeDefaultPermissions] = [userTypeDefaultPermissions] + ':' where [userTypeDefaultPermissions] not like '%:%';