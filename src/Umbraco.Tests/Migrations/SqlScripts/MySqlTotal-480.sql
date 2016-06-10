/*******************************************************************************************







    Umbraco database installation script for MySQL
 
IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
 
    Database version: 4.8.0.4
    
    Please increment this version number if ANY change is made to this script,
    so compatibility with scripts for other database systems can be verified easily.
    The first 3 digits depict the Umbraco version, the last digit is the database version.
    (e.g. version 4.0.0.3 means "Umbraco version 4.0.0, database version 3")
    
    Check-in policy: only commit this script if
     * you ran the Umbraco installer completely;
     * you ran it on the targetted database system;
     * you ran the Boost and Nitro installation;
     * you were able to browse the Boost site;
     * you were able to open the Umbraco administration panel;
     * you have documented the code change in this script;
     * you have incremented the version number in this script.
 
IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT







********************************************************************************************/

CREATE TABLE umbracoRelation 
( 
id int NOT NULL AUTO_INCREMENT PRIMARY KEY, 
parentId int NOT NULL, 
childId int NOT NULL, 
relType int NOT NULL, 
datetime TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, 
comment nvarchar (1000) NOT NULL 
)

; 
CREATE TABLE cmsDocument 
( 
nodeId int NOT NULL, 
published bit NOT NULL, 
documentUser int NOT NULL, 
versionId CHAR(36) NOT NULL PRIMARY KEY, 
text nvarchar (255) NOT NULL, 
releaseDate datetime NULL, 
expireDate datetime NULL, 
updateDate TIMESTAMP ON UPDATE CURRENT_TIMESTAMP DEFAULT CURRENT_TIMESTAMP, 
templateId int NULL, 
alias nvarchar (255) NULL ,
newest bit NOT NULL DEFAULT 0
) 
 
; 
CREATE TABLE umbracoLog 
( 
id int NOT NULL AUTO_INCREMENT PRIMARY KEY, 
userId int NOT NULL, 
NodeId int NOT NULL, 
Datestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
logHeader nvarchar (50) NOT NULL, 
logComment nvarchar (4000) NULL 
) 
 
; 
CREATE TABLE umbracoUserGroup 
( 
id smallint NOT NULL AUTO_INCREMENT PRIMARY KEY, 
userGroupName nvarchar (255) NOT NULL 
) 
 
; 

/* TABLE IS NEVER USED, REMOVED FOR 4.1

CREATE TABLE umbracoUser2userGroup 
( 
user int NOT NULL, 
userGroup smallint NOT NULL 
)  
; 
ALTER TABLE umbracoUser2userGroup ADD CONSTRAINT PK_user2userGroup PRIMARY KEY CLUSTERED (user, userGroup) 
; 

*/

CREATE TABLE umbracoApp 
( 
sortOrder tinyint NOT NULL DEFAULT 0, 
appAlias nvarchar (50) PRIMARY KEY NOT NULL, 
appIcon nvarchar (255) NOT NULL, 
appName nvarchar (255)  NOT NULL, 
appInitWithTreeAlias nvarchar (255) NULL 
) 
 
; 
CREATE TABLE cmsPropertyData 
( 
id int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
contentNodeId int NOT NULL, 
versionId CHAR(36) NULL, 
propertytypeid int NOT NULL, 
dataInt int NULL, 
dataDate datetime NULL, 
dataNvarchar nvarchar (500) NULL, 
dataNtext LONGTEXT NULL 
) 
; 
CREATE INDEX IX_cmsPropertyData_1 ON cmsPropertyData (contentNodeId) 
; 
CREATE INDEX IX_cmsPropertyData_2 ON cmsPropertyData (versionId) 
; 
CREATE INDEX IX_cmsPropertyData_3 ON cmsPropertyData (propertytypeid) 
; 
CREATE TABLE cmsContent 
( 
pk int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
nodeId int NOT NULL, 
contentType int NOT NULL 
) 
 
; 
CREATE TABLE cmsContentType 
( 
pk int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
nodeId int NOT NULL, 
alias nvarchar (255) NULL, 
icon nvarchar (255) NULL 
) 
 
; 
CREATE TABLE cmsMacroPropertyType 
( 
id smallint NOT NULL PRIMARY KEY AUTO_INCREMENT, 
macroPropertyTypeAlias nvarchar (50) NULL, 
macroPropertyTypeRenderAssembly nvarchar (255) NULL, 
macroPropertyTypeRenderType nvarchar (255) NULL, 
macroPropertyTypeBaseType nvarchar (255) NULL 
) 
; 

/* TABLE IS NEVER USED, REMOVED FOR 4.1

CREATE TABLE umbracoStylesheetProperty 
( 
id smallint NOT NULL PRIMARY KEY AUTO_INCREMENT, 
stylesheetPropertyEditor bit NOT NULL DEFAULT 0, 
stylesheet tinyint NOT NULL, 
stylesheetPropertyAlias nvarchar (50) NULL, 
stylesheetPropertyName nvarchar (255) NULL, 
stylesheetPropertyValue nvarchar (400) NULL 
)  
; 

*/

CREATE TABLE cmsTab 
( 
id int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
contenttypeNodeId int NOT NULL, 
text nvarchar (255) NOT NULL, 
sortorder int NOT NULL 
) 
 
; 
CREATE TABLE cmsTemplate 
( 
pk int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
nodeId int NOT NULL, 
master int NULL, 
alias nvarchar (100) NULL, 
design LONGTEXT NOT NULL 
) 
 
; 
CREATE TABLE umbracoUser2app 
( 
user int NOT NULL, 
app nvarchar (50) NOT NULL 
) 
 
; 
ALTER TABLE umbracoUser2app ADD CONSTRAINT PK_user2app PRIMARY KEY CLUSTERED  (user, app) 
; 
CREATE TABLE umbracoUserType 
( 
id smallint NOT NULL PRIMARY KEY AUTO_INCREMENT, 
userTypeAlias nvarchar (50) NULL, 
userTypeName nvarchar (255) NOT NULL, 
userTypeDefaultPermissions nvarchar (50) NULL 
) 
 
; 
CREATE TABLE umbracoUser 
( 
id int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
userDisabled bit NOT NULL DEFAULT 0, 
userNoConsole bit NOT NULL DEFAULT 0, 
userType smallint NOT NULL, 
startStructureID int NOT NULL, 
startMediaID int NULL, 
userName nvarchar (255) NOT NULL, 
userLogin nvarchar (125) NOT NULL, 
userPassword nvarchar (125) NOT NULL, 
userEmail nvarchar (255) NOT NULL, 
userDefaultPermissions nvarchar (50) NULL, 
userLanguage nvarchar (10) NULL ,
defaultToLiveEditing bit NOT NULL DEFAULT 0
) 
 
; 
CREATE TABLE cmsDocumentType 
( 
contentTypeNodeId int NOT NULL, 
templateNodeId int NOT NULL, 
IsDefault bit NOT NULL DEFAULT 0 
) 
 
; 
ALTER TABLE cmsDocumentType ADD CONSTRAINT PK_cmsDocumentType PRIMARY KEY CLUSTERED  (contentTypeNodeId, templateNodeId) 
; 
CREATE TABLE cmsMemberType 
( 
pk int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
NodeId int NOT NULL, 
propertytypeId int NOT NULL, 
memberCanEdit bit NOT NULL DEFAULT 0, 
viewOnProfile bit NOT NULL DEFAULT 0 
) 
 
; 
CREATE TABLE cmsMember 
( 
nodeId int NOT NULL, 
Email nvarchar (1000) NOT NULL DEFAULT '', 
LoginName nvarchar (1000) NOT NULL DEFAULT '', 
Password nvarchar (1000) NOT NULL DEFAULT '' 
) 
 
; 
CREATE TABLE umbracoNode 
( 
id int NOT NULL PRIMARY KEY, 
trashed bit NOT NULL DEFAULT 0, 
parentID int NOT NULL, 
nodeUser int NULL, 
level smallint NOT NULL, 
path nvarchar (150) NOT NULL, 
sortOrder int NOT NULL, 
uniqueID CHAR(36) NULL, 
text nvarchar (255) NULL, 
nodeObjectType CHAR(36) NULL, 
createDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) 
 
; 
CREATE INDEX IX_umbracoNodeParentId ON umbracoNode (parentID) 
; 
CREATE INDEX IX_umbracoNodeObjectType ON umbracoNode (nodeObjectType) 
; 
CREATE TABLE cmsPropertyType 
( 
id int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
dataTypeId int NOT NULL, 
contentTypeId int NOT NULL, 
tabId int NULL, 
Alias nvarchar (255) NOT NULL, 
Name nvarchar (255) NULL, 
helpText nvarchar (1000) NULL, 
sortOrder int NOT NULL DEFAULT 0, 
mandatory bit NOT NULL DEFAULT 0, 
validationRegExp nvarchar (255) NULL, 
Description nvarchar (2000) NULL 
) 
 
; 
CREATE TABLE cmsMacroProperty 
( 
id int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
macroPropertyHidden bit NOT NULL DEFAULT 0, 
macroPropertyType smallint NOT NULL, 
macro int NOT NULL, 
macroPropertySortOrder tinyint NOT NULL DEFAULT 0, 
macroPropertyAlias nvarchar (50) NOT NULL, 
macroPropertyName nvarchar (255) NOT NULL 
) 
 
; 
CREATE TABLE cmsMacro 
( 
id int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
macroUseInEditor bit NOT NULL DEFAULT 0, 
macroRefreshRate int NOT NULL DEFAULT 0, 
macroAlias nvarchar (255) NOT NULL, 
macroName nvarchar (255) NULL, 
macroScriptType nvarchar (255) NULL, 
macroScriptAssembly nvarchar (255) NULL, 
macroXSLT nvarchar (255) NULL, 
macroCacheByPage bit NOT NULL DEFAULT 1, 
macroCachePersonalized bit NOT NULL DEFAULT 0, 
macroDontRender bit NOT NULL DEFAULT 0 
) 
 
; 
CREATE TABLE cmsContentVersion 
( 
id int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
ContentId int NOT NULL, 
VersionId CHAR(36) NOT NULL, 
VersionDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) 
 
; 
CREATE TABLE umbracoAppTree 
( 
treeSilent bit NOT NULL DEFAULT 0, 
treeInitialize bit NOT NULL DEFAULT 1, 
treeSortOrder tinyint NOT NULL, 
appAlias nvarchar (50) NOT NULL, 
treeAlias nvarchar (150) NOT NULL, 
treeTitle nvarchar (255) NOT NULL, 
treeIconClosed nvarchar (255) NOT NULL, 
treeIconOpen nvarchar (255) NOT NULL, 
treeHandlerAssembly nvarchar (255) NOT NULL, 
treeHandlerType nvarchar (255) NOT NULL,
action nvarchar (300) NULL 
) 
 
; 
ALTER TABLE umbracoAppTree ADD CONSTRAINT PK_umbracoAppTree PRIMARY KEY CLUSTERED  (appAlias, treeAlias) 
; 
CREATE TABLE cmsContentTypeAllowedContentType 
( 
Id int NOT NULL, 
AllowedId int NOT NULL 
) 
 
; 
ALTER TABLE cmsContentTypeAllowedContentType ADD CONSTRAINT PK_cmsContentTypeAllowedContentType PRIMARY KEY CLUSTERED  (Id, AllowedId) 
; 
CREATE TABLE cmsContentXml 
( 
nodeId int NOT NULL PRIMARY KEY, 
xml LONGTEXT NOT NULL 
) 
 
; 
CREATE TABLE cmsDataType 
( 
pk int NOT NULL PRIMARY KEY AUTO_INCREMENT PRIMARY KEY, 
nodeId int NOT NULL, 
controlId CHAR(36) NOT NULL, 
dbType varchar (50) NOT NULL 
) 
 
; 
CREATE TABLE cmsDataTypePreValues 
( 
id int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
datatypeNodeId int NOT NULL, 
value NVARCHAR(2500) NULL, 
sortorder int NOT NULL, 
alias nvarchar (50) NULL 
) 
 
; 
CREATE TABLE cmsDictionary 
( 
pk int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
id CHAR(36) NOT NULL, 
parent CHAR(36) NOT NULL, 
`key` nvarchar (1000) NOT NULL 
) 
 
; 
CREATE TABLE cmsLanguageText 
( 
pk int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
languageId int NOT NULL, 
UniqueId CHAR(36) NOT NULL, 
value nvarchar (1000) NOT NULL 
) 
 
; 
CREATE TABLE cmsMember2MemberGroup 
( 
Member int NOT NULL, 
MemberGroup int NOT NULL 
) 
 
; 
ALTER TABLE cmsMember2MemberGroup ADD CONSTRAINT PK_cmsMember2MemberGroup PRIMARY KEY CLUSTERED  (Member, MemberGroup) 
; 
CREATE TABLE cmsStylesheet 
( 
nodeId int NOT NULL, 
filename nvarchar (100) NOT NULL, 
content LONGTEXT NULL 
) 
 
; 
CREATE TABLE cmsStylesheetProperty 
( 
nodeId int NOT NULL, 
stylesheetPropertyEditor bit NULL, 
stylesheetPropertyAlias nvarchar (50) NULL, 
stylesheetPropertyValue nvarchar (400) NULL 
) 
 
; 
CREATE TABLE umbracoDomains 
( 
id int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
domainDefaultLanguage int NULL, 
domainRootStructureID int NULL, 
domainName nvarchar (255) NOT NULL 
) 
 
; 
CREATE TABLE umbracoLanguage 
( 
id smallint NOT NULL PRIMARY KEY AUTO_INCREMENT, 
languageISOCode nvarchar (10) NULL, 
languageCultureName nvarchar (100) NULL 
) 
 
; 
CREATE TABLE umbracoRelationType 
( 
id int NOT NULL PRIMARY KEY AUTO_INCREMENT, 
`dual` bit NOT NULL, 
parentObjectType CHAR(36) NOT NULL, 
childObjectType CHAR(36) NOT NULL, 
name nvarchar (255) NOT NULL, 
alias nvarchar (100) NULL 
)
;

/* TABLE IS NEVER USED, REMOVED FOR 4.1
 
CREATE TABLE umbracoStylesheet 
( 
nodeId int NOT NULL PRIMARY KEY, 
filename nvarchar (100) NOT NULL, 
content LONGTEXT NULL 
)  
; 

*/

CREATE TABLE umbracoUser2NodeNotify 
( 
userId int NOT NULL, 
nodeId int NOT NULL, 
action char (1) NOT NULL 
) 
 
; 
ALTER TABLE umbracoUser2NodeNotify ADD CONSTRAINT PK_umbracoUser2NodeNotify PRIMARY KEY CLUSTERED  (userId, nodeId, action) 
; 
CREATE TABLE umbracoUser2NodePermission 
( 
userId int NOT NULL, 
nodeId int NOT NULL, 
permission char (1) NOT NULL 
) 
 
; 
ALTER TABLE umbracoUser2NodePermission ADD CONSTRAINT PK_umbracoUser2NodePermission PRIMARY KEY CLUSTERED  (userId, nodeId, permission) 
; 
INSERT INTO umbracoNode (id, trashed, parentID, nodeUser, level, path, sortOrder, uniqueID, text, nodeObjectType, createDate) VALUES 
	(-92, 0, -1, 0, 11, '-1,-92', 37, 'f0bc4bfb-b499-40d6-ba86-058885a5178c', 'Label', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/09/30 14:01:49.920'),
	(-90, 0, -1, 0, 11, '-1,-90', 35, '84c6b441-31df-4ffe-b67e-67d5bc3ae65a', 'Upload', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/09/30 14:01:49.920'),
	(-89, 0, -1, 0, 11, '-1,-89', 34, 'c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3', 'Textarea', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/09/30 14:01:49.920'),
	(-88, 0, -1, 0, 11, '-1,-88', 33, '0cc0eba1-9960-42c9-bf9b-60e150b429ae', 'Textstring', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/09/30 14:01:49.920'),
	(-87, 0, -1, 0, 11, '-1,-87', 32, 'ca90c950-0aff-4e72-b976-a30b1ac57dad', 'Richtext editor', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/09/30 14:01:49.920'),
	(-51, 0, -1, 0, 11, '-1,-51', 4, '2e6d3631-066e-44b8-aec4-96f09099b2b5', 'Numeric', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/09/30 14:01:49.920'),
	(-49, 0, -1, 0, 11, '-1,-49', 2, '92897bc6-a5f3-4ffe-ae27-f2e7e33dda49', 'True/false', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/09/30 14:01:49.920'),
	(-43, 0, -1, 0, 1, '-1,-43', 2, 'fbaf13a8-4036-41f2-93a3-974f678c312a', 'Checkbox list', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/10/15 14:11:04.367'),
	(-42, 0, -1, 0, 1, '-1,-42', 2, '0b6a45e7-44ba-430d-9da5-4e46060b9e03', 'Dropdow', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/10/15 14:10:59.000'),
	(-41, 0, -1, 0, 1, '-1,-41', 2, '5046194e-4237-453c-a547-15db3a07c4e1', 'Date Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/10/15 14:10:54.303'),
	(-40, 0, -1, 0, 1, '-1,-40', 2, 'bb5f57c9-ce2b-4bb9-b697-4caca783a805', 'Radiobox', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/10/15 14:10:49.253'),
	(-39, 0, -1, 0, 1, '-1,-39', 2, 'f38f0ac7-1d27-439c-9f3f-089cd8825a53', 'Dropdown multiple', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/10/15 14:10:44.480'),
	(-38, 0, -1, 0, 1, '-1,-38', 2, 'fd9f1447-6c61-4a7c-9595-5aa39147d318', 'Folder Browser', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/10/15 14:10:37.020'),
	(-37, 0, -1, 0, 1, '-1,-37', 2, '0225af17-b302-49cb-9176-b9f35cab9c17', 'Approved Color', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/10/15 14:10:30.580'),
	(-36, 0, -1, 0, 1, '-1,-36', 2, 'e4d66c0f-b935-4200-81f0-025f7256b89a', 'Date Picker with time', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2004/10/15 14:10:23.007'),
	(-20, 0, -1, 0, 0, '-1,-20', 0, '0F582A79-1E41-4CF0-BFA0-76340651891A', 'Recycle Bin', '01BB7FF2-24DC-4C0C-95A2-C24EF72BBAC8', '2004/09/30 14:01:49.920'),
	(-1, 0, -1, 0, 0, '-1', 0, '916724a5-173d-4619-b97e-b9de133dd6f5', 'SYSTEM DATA: umbraco master root', 'ea7d8624-4cfe-4578-a871-24aa946bf34d', '2004/09/30 14:01:49.920'),
	(1031, 0, -1, 1, 1, '-1,1031', 2, 'f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d', 'Folder', '4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', '2004/12/01 00:13:40.743'),
	(1032, 0, -1, 1, 1, '-1,1032', 2, 'cc07b313-0843-4aa8-bbda-871c8da728c8', 'Image', '4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', '2004/12/01 00:13:43.737'),
	(1033, 0, -1, 1, 1, '-1,1033', 2, '4c52d8ab-54e6-40cd-999c-7a5f24903e4d', 'File', '4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', '2004/12/01 00:13:46.210'),
	(1034, 0, -1, 0, 1, '-1,1034', 2, 'a6857c73-d6e9-480c-b6e6-f15f6ad11125', 'Content Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2006/01/03 13:07:29.203'),
	(1035, 0, -1, 0, 1, '-1,1035', 2, '93929b9a-93a2-4e2a-b239-d99334440a59', 'Media Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2006/01/03 13:07:36.143'),
	(1036, 0, -1, 0, 1, '-1,1036', 2, '2b24165f-9782-4aa3-b459-1de4a4d21f60', 'Member Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2006/01/03 13:07:40.260'),
	(1038, 0, -1, 0, 1, '-1,1038', 2, '1251c96c-185c-4e9b-93f4-b48205573cbd', 'Simple Editor', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2006/01/03 13:07:55.250'),
	(1039, 0, -1, 0, 1, '-1,1039', 2, '06f349a9-c949-4b6a-8660-59c10451af42', 'Ultimate Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2006/01/03 13:07:55.250'),
	(1040, 0, -1, 0, 1, '-1,1040', 2, '21e798da-e06e-4eda-a511-ed257f78d4fa', 'Related Links', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2006/01/03 13:07:55.250'),
	(1041, 0, -1, 0, 1, '-1,1041', 2, 'b6b73142-b9c1-4bf8-a16d-e1c23320b549', 'Tags', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2006/01/03 13:07:55.250'),
	(1042, 0, -1, 0, 1, '-1,1042', 2, '0a452bd5-83f9-4bc3-8403-1286e13fb77e', 'Macro Container', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2006/01/03 13:07:55.250'),
	(1043, 0, -1, 0, 1, '-1,1042', 2, '1df9f033-e6d4-451f-b8d2-e0cbc50a836f', 'Image Cropper', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '2006/01/03 13:07:55.250')
;

INSERT INTO cmsContentType (pk, nodeId, alias, icon) VALUES
	(532, 1031, 'Folder', 'folder.gif'),
	(533, 1032, 'Image', 'mediaPhoto.gif'),
	(534, 1033, 'File', 'mediaMulti.gif')
;
INSERT INTO umbracoUserType (id, userTypeAlias, userTypeName, userTypeDefaultPermissions) VALUES
	(1, 'admin', 'Administrators', 'CADMOSKTPIURZ5:'),
	(2, 'writer', 'Writer', 'CAH:'),
	(3, 'editor', 'Editors', 'CADMOSKTPUZ5:'),
	(4, 'translator', 'Translator', 'A')
;
INSERT INTO umbracoUser (id, userDisabled, userNoConsole, userType, startStructureID, startMediaID, userName, userLogin, userPassword, userEmail, userDefaultPermissions, userLanguage) VALUES (0, 0, 0, 1, -1, -1, 'Administrator', 'admin', 'default', '', NULL, 'en') 
;
UPDATE umbracoUser SET id=0 WHERE id=1 AND userLogin='admin'
;
INSERT INTO umbracoApp (appAlias, sortOrder, appIcon, appName, appInitWithTreeAlias) VALUES
	('content', 0, '.traycontent', 'Indhold', 'content'),
	('developer', 7, '.traydeveloper', 'Developer', NULL),
	('media', 1, '.traymedia', 'Mediearkiv', NULL),
	('member', 8, '.traymember', 'Medlemmer', NULL),
	('settings', 6, '.traysettings', 'Indstillinger', NULL),
	('users', 5, '.trayusers', 'Brugere', NULL)
;
INSERT INTO umbracoUser2app (user, app) VALUES
	(0, 'content'),
	(0, 'developer'),
	(0, 'media'),
	(0, 'member'),
	(0, 'settings'),
	(0, 'users')
;
INSERT INTO umbracoAppTree (appAlias, treeAlias, treeSilent, treeInitialize, treeSortOrder, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType) VALUES
	('content', 'content', 1, 1, 0, 'Indhold', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadContent'),
	('developer', 'cacheBrowser', 0, 1, 0, 'CacheBrowser', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadCache'),
	('developer', 'CacheItem', 0, 0, 0, 'Cachebrowser', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadCacheItem'),
	('developer', 'datatype', 0, 1, 1, 'Datatyper', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadDataTypes'),
	('developer', 'macros', 0, 1, 2, 'Macros', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadMacros'),
	('developer', 'xslt', 0, 1, 5, 'XSLT Files', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadXslt'),
	
	('developer', 'packager', 0, 1, 3, 'Packages', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadPackager'),
	('developer', 'packagerPackages', 0, 0, 1, 'Packager Packages', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadPackages'),
	
	('media', 'media', 0, 1, 0, 'Medier', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadMedia'),
	('member', 'member', 0, 1, 0, 'Medlemmer', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadMembers'),
	('member', 'memberGroups', 0, 1, 1, 'MemberGroups', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadMemberGroups'),
	('member', 'memberTypes', 0, 1, 2, 'Medlemstyper', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadMemberTypes'),
	('settings', 'languages', 0, 1, 4, 'Languages', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadLanguages'),
	('settings', 'mediaTypes', 0, 1, 5, 'Medietyper', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadMediaTypes'),
	('settings', 'documentTypes', 0, 1, 6, 'Dokumenttyper', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadNodeTypes'),
	('settings', 'stylesheetProperty', 0, 0, 0, 'Stylesheet Property', '', '', 'umbraco', 'loadStylesheetProperty'),
	('settings', 'stylesheets', 0, 1, 0, 'Stylesheets', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadStylesheets'),
	('settings', 'templates', 0, 1, 1, 'Templates', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadTemplates'),
	('users', 'users', 0, 1, 0, 'Brugere', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadUsers')
;

INSERT INTO umbracoAppTree (appAlias, treeAlias, treeSilent, treeInitialize, treeSortOrder, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType, action) VALUES
	('settings', 'dictionary', 0, 1, 3, 'Dictionary', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadDictionary','openDictionary()')
;
	
INSERT INTO cmsMacroPropertyType (id, macroPropertyTypeAlias, macroPropertyTypeRenderAssembly, macroPropertyTypeRenderType, macroPropertyTypeBaseType) VALUES
	(3, 'mediaCurrent', 'umbraco.macroRenderings', 'media', 'Int32'),
	(4, 'contentSubs', 'umbraco.macroRenderings', 'content', 'Int32'),
	(5, 'contentRandom', 'umbraco.macroRenderings', 'content', 'Int32'),
	(6, 'contentPicker', 'umbraco.macroRenderings', 'content', 'Int32'),
	(13, 'number', 'umbraco.macroRenderings', 'numeric', 'Int32'),
	(14, 'bool', 'umbraco.macroRenderings', 'yesNo', 'Boolean'),
	(16, 'text', 'umbraco.macroRenderings', 'text', 'String'),
	(17, 'contentTree', 'umbraco.macroRenderings', 'content', 'Int32'),
	(18, 'contentType', 'umbraco.macroRenderings', 'contentTypeSingle', 'Int32'),
	(19, 'contentTypeMultiple', 'umbraco.macroRenderings', 'contentTypeMultiple', 'Int32'),
	(20, 'contentAll', 'umbraco.macroRenderings', 'content', 'Int32'),
	(21, 'tabPicker', 'umbraco.macroRenderings', 'tabPicker', 'String'),
	(22, 'tabPickerMultiple', 'umbraco.macroRenderings', 'tabPickerMultiple', 'String'),
	(23, 'propertyTypePicker', 'umbraco.macroRenderings', 'propertyTypePicker', 'String'),
	(24, 'propertyTypePickerMultiple', 'umbraco.macroRenderings', 'propertyTypePickerMultiple', 'String'),
	(25, 'textMultiLine', 'umbraco.macroRenderings', 'textMultiple', 'String')
;
INSERT INTO cmsTab (id, contenttypeNodeId, text, sortorder) VALUES
	(3, 1032, 'Image', 1),
	(4, 1033, 'File', 1),
	(5, 1031, 'Contents', 1) 
;
INSERT INTO cmsPropertyType (id, dataTypeId, contentTypeId, tabId, Alias, Name, helpText, sortOrder, mandatory, validationRegExp, Description) VALUES
	(6, -90, 1032, 3, 'umbracoFile', 'Upload image', NULL, 0, 0, NULL, NULL),
	(7, -92, 1032, 3, 'umbracoWidth', 'Width', NULL, 0, 0, NULL, NULL),
	(8, -92, 1032, 3, 'umbracoHeight', 'Height', NULL, 0, 0, NULL, NULL),
	(9, -92, 1032, 3, 'umbracoBytes', 'Size', NULL, 0, 0, NULL, NULL),
	(10, -92, 1032, 3, 'umbracoExtension', 'Type', NULL, 0, 0, NULL, NULL),
	(24, -90, 1033, 4, 'umbracoFile', 'Upload file', NULL, 0, 0, NULL, NULL),
	(25, -92, 1033, 4, 'umbracoExtension', 'Type', NULL, 0, 0, NULL, NULL),
	(26, -92, 1033, 4, 'umbracoBytes', 'Size', NULL, 0, 0, NULL, NULL),
	(27, -38, 1031, 5, 'contents', 'Contents:', NULL, 0, 0, NULL, NULL)
;
INSERT INTO umbracoLanguage (id, languageISOCode, languageCultureName) VALUES (1, 'en-US', 'en-US')
;
INSERT INTO cmsContentTypeAllowedContentType (Id, AllowedId) VALUES (1031, 1031),(1031, 1032),(1031, 1033) 
;
INSERT INTO cmsDataType (pk, nodeId, controlId, dbType) VALUES
	(4, -49, '38b352c1-e9f8-4fd8-9324-9a2eab06d97a', 'Integer'),
	(6, -51, '1413afcb-d19a-4173-8e9a-68288d2a73b8', 'Integer'),
	(8, -87, '5E9B75AE-FACE-41c8-B47E-5F4B0FD82F83', 'Ntext'),
	(9, -88, 'ec15c1e5-9d90-422a-aa52-4f7622c63bea', 'Nvarchar'),
	(10, -89, '67db8357-ef57-493e-91ac-936d305e0f2a', 'Ntext'),
	(11, -90, '5032a6e6-69e3-491d-bb28-cd31cd11086c', 'Nvarchar'),
	(12, -91, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar'),
	(13, -92, '6c738306-4c17-4d88-b9bd-6546f3771597', 'Nvarchar'),
	(14, -36, 'b6fb1622-afa5-4bbf-a3cc-d9672a442222', 'Date'),
	(15, -37, 'f8d60f68-ec59-4974-b43b-c46eb5677985', 'Nvarchar'),
	(16, -38, 'cccd4ae9-f399-4ed2-8038-2e88d19e810c', 'Nvarchar'),
	(17, -39, '928639ed-9c73-4028-920c-1e55dbb68783', 'Nvarchar'),
	(18, -40, 'a52c7c1c-c330-476e-8605-d63d3b84b6a6', 'Nvarchar'),
	(19, -41, '23e93522-3200-44e2-9f29-e61a6fcbb79a', 'Date'),
	(20, -42, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Integer'),
	(21, -43, 'b4471851-82b6-4c75-afa4-39fa9c6a75e9', 'Nvarchar'),
	(22, -44, 'a3776494-0574-4d93-b7de-efdfdec6f2d1', 'Ntext'),
	(23, -128, 'a52c7c1c-c330-476e-8605-d63d3b84b6a6', 'Nvarchar'),
	(24, -129, '928639ed-9c73-4028-920c-1e55dbb68783', 'Nvarchar'),
	(25, -130, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar'),
	(26, -131, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar'),
	(27, -132, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar'),
	(28, -133, '6c738306-4c17-4d88-b9bd-6546f3771597', 'Ntext'),
	(29, -134, '928639ed-9c73-4028-920c-1e55dbb68783', 'Nvarchar'),
	(30, -50, 'aaf99bb2-dbbe-444d-a296-185076bf0484', 'Date'),
	(31, 1034, '158aa029-24ed-4948-939e-c3da209e5fba', 'Integer'),
	(32, 1035, 'ead69342-f06d-4253-83ac-28000225583b', 'Integer'),
	(33, 1036, '39f533e4-0551-4505-a64b-e0425c5ce775', 'Integer'),
	(35, 1038, '60b7dabf-99cd-41eb-b8e9-4d2e669bbde9', 'Ntext'),
	(36, 1039, 'cdbf0b5d-5cb2-445f-bc12-fcaaec07cf2c', 'Ntext'),
	(37, 1040, '71b8ad1a-8dc2-425c-b6b8-faa158075e63', 'Ntext'),
	(38, 1041, '4023e540-92f5-11dd-ad8b-0800200c9a66', 'Ntext'),
	(39, 1042, '474FCFF8-9D2D-11DE-ABC6-AD7A56D89593', 'Ntext'),
	(40, 1043, '7A2D436C-34C2-410F-898F-4A23B3D79F54', 'Ntext')  
;
ALTER TABLE umbracoAppTree ADD FOREIGN KEY (appAlias) REFERENCES umbracoApp (appAlias) 
; 
ALTER TABLE cmsPropertyData ADD FOREIGN KEY (contentNodeId) REFERENCES umbracoNode (id) 
; 

/* TABLE IS NEVER USED, REMOVED FOR 4.1

ALTER TABLE umbracoUser2userGroup ADD FOREIGN KEY (user) REFERENCES umbracoUser (id)
;
ALTER TABLE umbracoUser2userGroup ADD FOREIGN KEY (userGroup) REFERENCES umbracoUserGroup (id) 
; 

*/

ALTER TABLE cmsDocument ADD FOREIGN KEY (nodeId) REFERENCES umbracoNode (id) 
; 
ALTER TABLE cmsMacroProperty ADD FOREIGN KEY (macroPropertyType) REFERENCES cmsMacroPropertyType (id) 
; 
ALTER TABLE umbracoUser ADD FOREIGN KEY (userType) REFERENCES umbracoUserType (id) 
; 
ALTER TABLE cmsTemplate ADD FOREIGN KEY (nodeId) REFERENCES umbracoNode (id) 
; 
ALTER TABLE cmsContentType ADD FOREIGN KEY (nodeId) REFERENCES umbracoNode (id)
;
ALTER TABLE umbracoNode ADD FOREIGN KEY (parentID) REFERENCES umbracoNode (id) 
; 
ALTER TABLE cmsPropertyType ADD FOREIGN KEY (tabId) REFERENCES cmsTab (id) 
; 
ALTER TABLE cmsContent ADD FOREIGN KEY (nodeId) REFERENCES umbracoNode (id) 
; 
ALTER TABLE umbracoUser2app ADD FOREIGN KEY (app) REFERENCES umbracoApp (appAlias)
;

/* TABLE IS NEVER USED, REMOVED FOR 4.1

ALTER TABLE umbracoUser2userGroup ADD FOREIGN KEY (user) REFERENCES umbracoUser (id) 
;

*/

CREATE TABLE cmsTask
(
closed bit NOT NULL DEFAULT 0,
id int NOT NULL PRIMARY KEY AUTO_INCREMENT,
taskTypeId tinyint NOT NULL,
nodeId int NOT NULL,
parentUserId int NOT NULL,
userId int NOT NULL,
DateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
Comment nvarchar (500) NULL
)
;
CREATE TABLE cmsTaskType
(
id tinyint NOT NULL PRIMARY KEY AUTO_INCREMENT,
alias nvarchar (255) NOT NULL
)
;
insert into cmsTaskType (alias) values ('toTranslate')
;
insert into umbracoRelationType (`dual`, parentObjectType, childObjectType, name, alias) values (1, 'c66ba18e-eaf3-4cff-8a22-41b16d66a972', 'c66ba18e-eaf3-4cff-8a22-41b16d66a972', 'Relate Document On Copy','relateDocumentOnCopy')
;
ALTER TABLE cmsMacro ADD macroPython nvarchar(255)
;
INSERT INTO umbracoAppTree(treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType) VALUES(0, 1, 4, 'developer', 'python', 'Python Files', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadPython') 
;
INSERT INTO umbracoAppTree(treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType) VALUES(0, 1, 2, 'settings', 'scripts', 'Scripts', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadScripts') 
;
alter TABLE cmsContentType add thumbnail nvarchar(255) NOT NULL DEFAULT 'folder.png'
;
alter TABLE cmsContentType add description nvarchar(1500) NULL
;
alter TABLE cmsContentType add masterContentType int NULL
;
insert into cmsDataTypePreValues (id, dataTypeNodeId, value, sortorder, alias) values 
(3,-87,',code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|', 0, ''),
(4,1041,'default', 0, 'group')
;

UPDATE umbracoUserType SET userTypeDefaultPermissions = CONCAT(userTypeDefaultPermissions, 'F') WHERE INSTR(userTypeDefaultPermissions,'A') >= 1
AND INSTR(userTypeDefaultPermissions,'F') < 1
;

UPDATE umbracoUserType SET userTypeDefaultPermissions = CONCAT(userTypeDefaultPermissions, 'H') WHERE userTypeAlias = 'writer'
AND INSTR(userTypeDefaultPermissions,'F') < 1
;

INSERT IGNORE INTO umbracoUser2NodePermission (userID, nodeId, permission) 
SELECT userID, nodeId, 'F' FROM umbracoUser2NodePermission WHERE permission='A'
;

INSERT IGNORE INTO umbracoUser2NodePermission (userID, nodeId, permission) 
SELECT DISTINCT userID, nodeId, 'H' FROM umbracoUser2NodePermission WHERE userId IN
(SELECT umbracoUser.id FROM umbracoUserType INNER JOIN umbracoUser ON umbracoUserType.id = umbracoUser.userType WHERE (umbracoUserType.userTypeAlias = 'writer'))
;

INSERT IGNORE INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 0, 0, 'content', 'contentRecycleBin', 'RecycleBin', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.ContentRecycleBin')
;

INSERT IGNORE INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 1, 'users', 'userTypes', 'User Types', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.UserTypes')
;

INSERT IGNORE INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 2, 'users', 'userPermissions', 'User Permissions', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.UserPermissions')
;

CREATE TABLE cmsTagRelationship
(
	nodeId int NOT NULL,
	tagId int NOT NULL
);
	
ALTER TABLE cmsTagRelationship ADD CONSTRAINT PK_user2app PRIMARY KEY CLUSTERED (nodeId, tagId);

CREATE TABLE cmsTags(
	id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
	tag VARCHAR(200) NULL,
	parentId INT NULL,
	`group` VARCHAR(100) NULL
);

ALTER TABLE cmsTagRelationship ADD CONSTRAINT umbracoNode_cmsTagRelationship FOREIGN KEY(nodeId)
REFERENCES umbracoNode (id)
ON DELETE CASCADE;

ALTER TABLE cmsTagRelationship ADD CONSTRAINT cmsTags_cmsTagRelationship FOREIGN KEY(tagId)
REFERENCES cmsTags (id)
ON DELETE CASCADE;

/* TRANSLATION RELATED SQL */
INSERT INTO umbracoApp (appAlias, sortOrder, appIcon, appName, appInitWithTreeAlias) 
VALUES ('translation', 5, '.traytranslation', 'Translation', NULL)
;
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 1, 'translation','openTasks', 'Tasks assigned to you', '.sprTreeFolder', '.sprTreeFolder_o', 'umbraco', 'loadOpenTasks')
;
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 2, 'translation','yourTasks', 'Tasks created by you', '.sprTreeFolder', '.sprTreeFolder_o', 'umbraco', 'loadYourTasks')
;

ALTER TABLE umbraconode MODIFY COLUMN id INTEGER NOT NULL AUTO_INCREMENT; /* fix for MySQL bug 36411 */

/* remove auto increment so we can insert identity */
ALTER TABLE umbraconode MODIFY COLUMN id INTEGER NOT NULL; 
/* INSERT NEW MEDIA RECYCLE BIN NODE */
INSERT INTO umbracoNode (id, trashed, parentID, nodeUser, level, path, sortOrder, uniqueID, text, nodeObjectType, createDate) 
VALUES (-21, 0, -1, 0, 0, '-1,-21', 0, 'BF7C7CBC-952F-4518-97A2-69E9C7B33842', 'Recycle Bin', 'CF3D8E34-1C1C-41e9-AE56-878B57B32113', '2009/08/28 00:28:28.920')
;
/* re-add auto increment */
ALTER TABLE umbraconode MODIFY COLUMN id INTEGER NOT NULL AUTO_INCREMENT; 
/* Add the mediaRecycleBin tree type */
INSERT IGNORE INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 0, 0, 'media', 'mediaRecycleBin', 'RecycleBin', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.MediaRecycleBin')
;

CREATE TABLE cmsPreviewXml(
	nodeId int NOT NULL,
	versionId CHAR(36) NOT NULL,
	timestamp datetime NOT NULL,
	xml LONGTEXT NOT NULL)
;
ALTER TABLE cmsPreviewXml ADD CONSTRAINT PK_cmsContentPreviewXml PRIMARY KEY CLUSTERED (nodeId, versionId) 
; 


/* Create missing indexes and primary keys */
CREATE INDEX IX_Icon ON cmsContentType(nodeId, icon)
;

/* CHANGE:Allan Stegelmann Laustsen */
/* Create Custom Index to speed up tree loading */
CREATE INDEX IX_contentid_versiondate ON cmscontentversion(CONTENTID, VERSIONDATE)
;
/* CHANGE:End */

