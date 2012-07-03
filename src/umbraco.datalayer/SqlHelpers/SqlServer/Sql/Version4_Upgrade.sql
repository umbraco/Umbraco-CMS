/*******************************************************************************************

    Umbraco database installation script for SQL Server (upgrade from Umbraco 4.0.x)
 
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


CREATE TABLE [cmsPreviewXml](
	[nodeId] [int] NOT NULL,
	[versionId] [uniqueidentifier] NOT NULL,
	[timestamp] [datetime] NOT NULL,
	[xml] [ntext] NOT NULL,
 CONSTRAINT [PK_cmsContentPreviewXml] PRIMARY KEY CLUSTERED 
(
	[nodeId] ASC,
	[versionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)


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

/************************** CLEANUP END ********************************************/
